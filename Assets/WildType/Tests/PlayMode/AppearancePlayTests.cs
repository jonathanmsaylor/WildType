using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using TMPro;
namespace WildType.Tests
{
    public sealed class AppearancePlayTests
    {
        StageSession session;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return SceneManager.LoadSceneAsync("CreatureStage_Prototype");
            session = Object.FindAnyObjectByType<StageSession>(); session.autoPauseOnFocusLoss = false; session.SetPaused(false);
            session.GetComponent<PlayerInputBridge>().enabled = false;
            yield return null; FreezeBrains();
        }
        [UnityTearDown] public IEnumerator Cleanup() { Time.timeScale = 1; if (session) session.SetPaused(false); yield return null; }
        void FreezeBrains()
        {
            foreach (var actor in session.Creatures)
            { var brain = actor.GetComponent<HerbivoreBrain>(); if (brain) brain.enabled = false; actor.DesiredDirection = Vector3.zero; actor.WantsSprint = false; }
        }
        void Place(CreatureAgent actor, float x, float z) => actor.Motor.Teleport(new Vector3(x, Ecosystem.Height(x, z) + .15f, z));
        IEnumerator Mate(CreatureAgent a, CreatureAgent b)
        {
            Place(a, 0, 0); Place(b, 2, 0); a.Vitals.Eat(1000); b.Vitals.Eat(1000);
            Assert.True(session.Generations.TryMate(a, b, out string reason), reason);
            yield return new WaitForSeconds(2.2f); FreezeBrains();
        }
        static void CheckAppearance(CreatureAgent actor)
        {
            Assert.AreEqual(new CreatureAppearance(actor.Genome), actor.Visual.Appearance);
            Assert.AreEqual(actor.Visual.Appearance.BodyScale, actor.Visual.transform.Find("Torso").localScale);
            Assert.AreEqual(375, actor.Visual.MarkingsMesh.vertexCount);
            Assert.AreEqual(1728, actor.Visual.MarkingsMesh.triangles.Length);
            foreach (var renderer in actor.Visual.GetComponentsInChildren<Renderer>()) Assert.AreSame(actor.Session.prototypeMaterial, renderer.sharedMaterial);
        }
        [UnityTest] public IEnumerator CoatMeshIsStableAcrossFramesAndReleasedWithCreature()
        {
            var actor = session.Creatures[2]; CheckAppearance(actor);
            var mesh = actor.Visual.MarkingsMesh; var vertices = mesh.vertices;
            yield return new WaitForSeconds(.3f);
            Assert.AreSame(mesh, actor.Visual.MarkingsMesh); CollectionAssert.AreEqual(vertices, mesh.vertices);
            foreach (var creature in session.Creatures) CheckAppearance(creature);
            Object.Destroy(actor.gameObject); yield return null; yield return null;
            Assert.False(mesh, "Owned procedural mesh must be released, not accumulated across deaths/reseeds");
        }
        [UnityTest] public IEnumerator TwoGenerationsKeepInheritedAdultShapeThroughGrowthAndControl()
        {
            var parent = session.Player; var partner = session.Creatures[2];
            yield return Mate(parent, partner);
            var child = session.Creatures[session.Creatures.Count - 1]; CheckAppearance(child);
            Assert.AreEqual(1, session.Generations.Archive.Get(child.Life.Id).Generation);
            var shape = child.Visual.Appearance; var mesh = child.Visual.MarkingsMesh; var vertices = mesh.vertices;
            Assert.Less(child.Visual.transform.localScale.x, .6f); string snapshot = session.Generations.Archive.Changes(child.Life.Id);
            Assert.True(session.TakeControl(child)); FreezeBrains();
            Time.timeScale = 10; yield return new WaitForSeconds(child.Stats.MaturityAge + .5f); Time.timeScale = 1;
            Assert.True(child.Life.Adult); Assert.AreEqual(Vector3.one, child.Visual.transform.localScale);
            Assert.AreEqual(shape, child.Visual.Appearance); Assert.AreSame(mesh, child.Visual.MarkingsMesh); CollectionAssert.AreEqual(vertices, mesh.vertices);
            // Compare a real inherited adult's measured motion and reserve drain with its phenotype.
            Place(child, 0, 0); child.Vitals.Eat(1000); child.DesiredDirection = Vector3.forward;
            yield return new WaitForSeconds(1);
            Assert.AreEqual(child.Stats.WalkSpeed, child.Motor.Speed, .3f);
            float startEnergy = child.Vitals.Energy; child.DesiredDirection = Vector3.zero;
            yield return new WaitForSeconds(1); Assert.Less(child.Vitals.Energy, startEnergy);
            yield return Mate(child, session.Creatures[4]);
            var grandchild = session.Creatures[session.Creatures.Count - 1]; CheckAppearance(grandchild);
            Assert.AreEqual(2, session.Generations.Archive.Get(grandchild.Life.Id).Generation);
            Assert.True(session.TakeControl(grandchild)); FreezeBrains();
            parent.Vitals.Damage(100); yield return new WaitForSeconds(.2f);
            Assert.AreEqual(snapshot, session.Generations.Archive.Changes(child.Life.Id), "Parent values must survive death");
            TMP_Text family = null;
            foreach (var label in session.GetComponentsInChildren<TMP_Text>()) if (label.name == "Inherited traits") family = label;
            Assert.NotNull(family); StringAssert.Contains("GENERATION 2", family.text);
            StringAssert.Contains("ADULT: parent A / B -> child", family.text);
            family.ForceMeshUpdate(); Assert.False(family.isTextOverflowing, "Inheritance panel must remain readable");
            Assert.LessOrEqual(session.Population + session.Generations.PendingBirths, GenerationLoop.PopulationCap);
            Debug.Log($"WILDTYPE_VISIBLE_INHERITANCE: actual gen1/gen2 births, growth, mesh stability, descendant control and walk speed verified; gen1 legs={child.Genome.legLength:F3} speed={child.Stats.WalkSpeed:F3}");
        }
        [UnityTest] public IEnumerator RestartReproducesFounderShapesAndReseedKeepsValidBoundedPresentation()
        {
            var playerShape = session.Player.Visual.Appearance; var otherShape = session.Creatures[2].Visual.Appearance;
            var oldMesh = session.Player.Visual.MarkingsMesh;
            session.SetPaused(true); session.Restart(false); yield return null; yield return new WaitForSeconds(.2f); FreezeBrains();
            Assert.False(oldMesh); Assert.AreEqual(playerShape, session.Player.Visual.Appearance);
            Assert.AreEqual(otherShape, session.Creatures[2].Visual.Appearance); Assert.AreEqual(13, session.Population);
            session.SetPaused(true); session.Restart(true); yield return null; yield return new WaitForSeconds(.2f); FreezeBrains();
            Assert.AreEqual(13, session.Population); Assert.AreEqual(0, session.Generations.Births); Assert.AreEqual(0, session.Generations.Deaths);
            foreach (var actor in session.Creatures) CheckAppearance(actor);
            Assert.AreEqual(playerShape, session.Player.Visual.Appearance, "Same founder genome stays deterministic across world seeds");
        }
    }
}
