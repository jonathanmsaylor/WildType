using System.Collections;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace WildType.Tests
{
    public sealed class LineageLocatePlayTests
    {
        StageSession session; CreatureAgent parent, child, grandchild;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return SceneManager.LoadSceneAsync("CreatureStage_Prototype");
            session = Object.FindAnyObjectByType<StageSession>(); session.autoPauseOnFocusLoss = false; session.SetPaused(false);
            session.GetComponent<PlayerInputBridge>().enabled = false;
            yield return null; Freeze(); parent = session.Player;
        }
        [UnityTearDown] public IEnumerator Cleanup() { if (session) { session.Locator.Clear(); session.SetPaused(false); } Time.timeScale = 1; yield return null; }
        void Freeze() { foreach (var a in session.Creatures) { var brain = a.GetComponent<HerbivoreBrain>(); if (brain) brain.enabled = false; a.DesiredDirection = Vector3.zero; a.WantsSprint = false; } }
        static void Place(CreatureAgent a, float x, float z) => a.Motor.Teleport(new Vector3(x, Ecosystem.Height(x, z) + .15f, z));
        IEnumerator Birth(CreatureAgent first, CreatureAgent second, string name = "")
        {
            Place(first, 0, 0); Place(second, 2, 0); first.Vitals.Eat(1000); second.Vitals.Eat(1000);
            int births = session.Generations.Births;
            Assert.True(session.Generations.TryMate(first, second, out string reason), reason);
            yield return new WaitForSecondsRealtime(2.5f); Freeze(); Assert.AreEqual(births + 1, session.Generations.Births);
            if (first.IsPlayer || second.IsPlayer) { Assert.True(session.Names.HasPrompt); Assert.True(session.Names.Submit(name)); }
            else Assert.False(session.Names.HasPrompt);
        }
        IEnumerator Grow(CreatureAgent a)
        { Time.timeScale = 10; yield return new WaitForSeconds(a.Stats.MaturityAge + .6f); Time.timeScale = 1; Freeze(); Assert.True(a.Life.Adult); }
        IEnumerator Hierarchy()
        {
            yield return Birth(parent, session.Creatures[1], "Dave"); child = session.Creatures.Last();
            string childName = session.Names.PersonalName(child.Life.Id); yield return Grow(child); Assert.AreEqual(childName, session.Names.PersonalName(child.Life.Id));
            Place(parent, -5, 0); yield return Birth(child, session.Creatures[4]); grandchild = session.Creatures.Last();
            Place(parent, 0, 0); Place(child, -3, 5); Place(grandchild, 3, 6); session.orbit.Configure(parent, session); yield return null;
            Assert.AreEqual(2, session.Generations.Archive.Get(grandchild.Life.Id).Generation);
            Assert.IsNotEmpty(session.Names.PersonalName(grandchild.Life.Id)); Caps();
        }
        void Caps()
        {
            Assert.LessOrEqual(session.Population + session.Generations.PendingBirths, 24); Assert.LessOrEqual(session.Creatures.Count + session.Generations.PendingBirths, 32);
            Assert.AreEqual(72, session.World.Foods.Count); Assert.LessOrEqual(session.Names.Count, session.Generations.Archive.Count); Assert.LessOrEqual(session.Names.Count, 512);
            foreach (var a in session.Creatures) { Assert.True(CreatureMotor.Finite(a.transform.position)); Assert.That(a.Vitals.Energy, Is.InRange(0, a.Stats.MaxEnergy)); }
        }
        [UnityTest] public IEnumerator JournalLocatesChildAndGrandchildWithoutControlOrCareChanges()
        {
            yield return Hierarchy(); session.SetPaused(true); yield return new WaitForSecondsRealtime(.2f);
            var buttons = session.GetComponentsInChildren<Button>().Where(b => b.name == "Locate").ToArray(); Assert.AreEqual(2, buttons.Length);
            buttons[0].onClick.Invoke(); Assert.AreSame(child, session.Locator.Target); Assert.AreSame(parent, session.Player); Assert.False(session.Paused);
            StringAssert.Contains("Locating Dave 1", session.Notice); StringAssert.DoesNotContain("Following", session.Notice);
            yield return new WaitForSeconds(.4f); float remaining = session.Locator.Remaining;
            session.SetPaused(true); yield return new WaitForSecondsRealtime(.2f); Assert.AreEqual(remaining, session.Locator.Remaining);
            buttons[0].onClick.Invoke(); Assert.Greater(session.Locator.Remaining, remaining);
            session.SetPaused(true); yield return new WaitForSecondsRealtime(.2f); buttons[1].onClick.Invoke();
            Assert.AreSame(grandchild, session.Locator.Target); Assert.AreSame(parent, session.Player); Assert.AreEqual(Vector3.zero, parent.DesiredDirection);
            Place(grandchild, 0, 2); grandchild.Vitals.SpendEnergy(25); float energy = parent.Vitals.Energy;
            Assert.False(session.Care.IsChild(parent, grandchild)); Assert.False(session.Care.TryShare(parent, grandchild, out _));
            Assert.AreEqual(energy, parent.Vitals.Energy); Assert.AreNotSame(grandchild, session.Care.NearbyChild(parent));
            Assert.True(session.TakeControl(child)); Assert.False(session.Locator.Target); Assert.True(session.Care.IsChild(child, grandchild)); Caps();
        }
        [UnityTest] public IEnumerator PulseRestoresAppearanceAndOffscreenCueExpiresOrClearsWithDeath()
        {
            yield return Hierarchy(); var renderer = grandchild.Visual.GetComponentInChildren<Renderer>();
            var original = new MaterialPropertyBlock(); renderer.GetPropertyBlock(original); Color color = original.GetColor("_BaseColor"); var material = renderer.sharedMaterial;
            Assert.True(session.Locator.Select(grandchild)); yield return new WaitForSeconds(.6f);
            var changed = new MaterialPropertyBlock(); renderer.GetPropertyBlock(changed); Assert.AreNotEqual(color, changed.GetColor("_BaseColor")); Assert.AreSame(material, renderer.sharedMaterial);
            session.Locator.Select(child); yield return null; yield return null; renderer.GetPropertyBlock(changed); Assert.AreEqual(color, changed.GetColor("_BaseColor"));
            var cam = session.orbit.transform; var behind = cam.position - cam.forward * 30; Place(grandchild, behind.x, behind.z);
            Assert.True(session.Locator.Select(grandchild)); yield return new WaitForSeconds(.2f);
            var cues = Object.FindAnyObjectByType<FamilyWorldCues>(); Assert.True(cues.DirectionCueVisible); Assert.True(cues.LocateCueVisible);
            var label = GameObject.Find("Descendant locate cue").GetComponent<TMP_Text>(); StringAssert.Contains("Behind", label.text);
            StringAssert.Contains(session.Names.PersonalName(grandchild.Life.Id), label.text); StringAssert.Contains("Gen 2", label.text);
            yield return new WaitForSeconds(4.1f); Assert.False(session.Locator.Target); Assert.False(cues.LocateCueVisible);
            renderer.GetPropertyBlock(changed); Assert.AreEqual(color, changed.GetColor("_BaseColor"));
            Assert.True(session.Locator.Select(grandchild)); grandchild.Vitals.Damage(100); Assert.False(session.Locator.Target);
            yield return null; yield return null; Assert.False(cues.LocateCueVisible); Assert.False(Object.FindAnyObjectByType<DescendantPulse>().Shown); Caps();
        }
        [UnityTest] public IEnumerator DuplicateNamesKeepIdsAndSurviveCleanupWhileResetClearsNamesAndTargets()
        {
            yield return Birth(parent, session.Creatures[1], "Dave"); child = session.Creatures.Last(); var childId = child.Life.Id;
            yield return Grow(child); Assert.True(session.TakeControl(child)); Freeze();
            yield return Birth(child, session.Creatures[4], "Dave"); grandchild = session.Creatures.Last(); var grandId = grandchild.Life.Id;
            Assert.AreEqual("Dave 1", session.Names.PersonalName(childId)); Assert.AreEqual("Dave 1", session.Names.PersonalName(grandId));
            Assert.AreNotEqual(session.Names.Label(childId), session.Names.Label(grandId));
            session.Names.RecordBirth(grandchild, child, session.Creatures[4]); Assert.False(session.Names.HasPrompt, "Duplicate birth recording must not rename/re-prompt");
            Assert.True(session.Locator.Select(grandchild)); Assert.True(session.TakeControl(grandchild)); Assert.False(session.Locator.Target);
            child.Vitals.Damage(100); yield return new WaitForSeconds(4.3f); Assert.False(child);
            Assert.AreEqual("Dave 1", session.Names.PersonalName(childId)); Assert.False(session.Generations.Archive.Get(childId).Alive);
            session.SetPaused(true); session.Restart(false); yield return null; yield return new WaitForSecondsRealtime(.3f); Freeze();
            Assert.AreEqual(0, session.Names.Count); Assert.False(session.Locator.Target);
            parent = session.Player; yield return Birth(parent, session.Creatures[1]); child = session.Creatures.Last();
            Assert.AreEqual(FamilyNames.Format(FamilyNames.DefaultName(child.Life.Id), 1), session.Names.PersonalName(child.Life.Id));
            session.Locator.Select(child); session.SetPaused(true); int seed = session.seed; session.Restart(true); yield return null; yield return new WaitForSecondsRealtime(.3f); Freeze();
            Assert.AreNotEqual(seed, session.seed); Assert.AreEqual(0, session.Names.Count); Assert.False(session.Locator.Target); Assert.False(session.Locator.Select(child)); Caps();
        }
        [UnityTest] public IEnumerator AutonomousBrainBirthReceivesNameWithoutPlayerPrompt()
        {
            var first = session.Creatures[1]; var second = session.Creatures[4]; Place(parent, -45, 0);
            foreach (var a in session.Creatures) if (a != first && a != second && a != parent) Place(a, 70, 30);
            first.Vitals.Eat(1000); second.Vitals.Eat(1000); first.GetComponent<HerbivoreBrain>().enabled = true;
            // Controlled co-location; mate decision/courtship/birth still use the real AI flow.
            for (int n = 0; n < 120 && session.Generations.Births == 0; n++)
            { Place(first, 0, 0); Place(second, 2, 0); yield return new WaitForSeconds(.25f); }
            Freeze(); Assert.Greater(session.Generations.Births, 0); var baby = session.Creatures.Last();
            Assert.False(session.Names.HasPrompt); Assert.False(session.Paused); Assert.IsNotEmpty(session.Names.PersonalName(baby.Life.Id));
            Assert.AreEqual(FamilyNames.Format(FamilyNames.DefaultName(baby.Life.Id), 1), session.Names.PersonalName(baby.Life.Id)); Caps();
        }
    }
}
