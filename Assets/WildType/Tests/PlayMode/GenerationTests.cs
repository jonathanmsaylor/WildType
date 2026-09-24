using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace WildType.Tests
{
    public sealed class GenerationTests
    {
        StageSession session;
        GenerationLoop loop;
        Keyboard keyboard;
        Gamepad pad;
        InputSettings original, input;
        [UnitySetUp] public IEnumerator Setup()
        {
            original = InputSystem.settings; input = Object.Instantiate(original); input.hideFlags = HideFlags.DontSave;
            input.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            input.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            InputSystem.settings = input;
            yield return SceneManager.LoadSceneAsync("CreatureStage_Prototype");
            session = Object.FindAnyObjectByType<StageSession>(); loop = session.Generations;
            session.autoPauseOnFocusLoss = false; session.SetPaused(false);
            keyboard = InputSystem.AddDevice<Keyboard>(); pad = InputSystem.AddDevice<Gamepad>();
            yield return null; FreezeBrains();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            if (pad != null) InputSystem.RemoveDevice(pad);
            if (original) InputSystem.settings = original;
            if (input) Object.Destroy(input);
            Time.timeScale = 1; yield return null;
        }
        void FreezeBrains()
        {
            foreach (var actor in session.Creatures)
            { var brain = actor.GetComponent<HerbivoreBrain>(); if (brain) brain.enabled = false; actor.DesiredDirection = Vector3.zero; actor.WantsSprint = false; }
        }
        void Place(CreatureAgent actor, float x, float z) => actor.Motor.Teleport(new Vector3(x, Ecosystem.Height(x, z) + .15f, z));
        CreatureAgent ChildOf(CreatureAgent parent)
        { var children = loop.LivingDescendants(parent.Life.Id); Assert.Greater(children.Count, 0); return children[children.Count - 1]; }
        IEnumerator Mate(CreatureAgent a, CreatureAgent b)
        {
            Place(a, 0, 0); Place(b, 2, 0); a.Vitals.Eat(1000); b.Vitals.Eat(1000);
            Assert.True(loop.TryMate(a, b, out string reason), reason);
            yield return new WaitForSeconds(2.2f); FreezeBrains();
        }
        [UnityTest] public IEnumerator PlayerMatingGrowthFoodAndDescendantControl()
        {
            var parent = session.Player; var partner = session.Creatures[1]; Place(partner, 2, 0);
            float energy = parent.Vitals.Energy; string genome = JsonUtility.ToJson(parent.Genome);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.M)); yield return null; yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); Assert.AreEqual(1, loop.PendingBirths);
            session.SetPaused(true); double clock = loop.Clock;
            yield return new WaitForSecondsRealtime(.2f); Assert.AreEqual(clock, loop.Clock); Assert.AreEqual(0, loop.Births);
            session.SetPaused(false); yield return new WaitForSeconds(2.2f); FreezeBrains();
            Assert.AreEqual(1, loop.Births); Assert.AreEqual(14, session.Population);
            var child = ChildOf(parent); var record = loop.Archive.Get(child.Life.Id);
            Assert.AreEqual(parent.Life.Id, record.FirstParentId); Assert.AreEqual(partner.Life.Id, record.SecondParentId);
            Assert.AreEqual(1, record.Generation); Assert.AreEqual(genome, JsonUtility.ToJson(parent.Genome));
            Assert.AreNotSame(parent.Genome, child.Genome);
            Assert.Less(parent.Vitals.Energy, energy - parent.Stats.MaxEnergy * .29f); Assert.Greater(parent.Life.Cooldown, 0);
            Assert.False(child.Life.Adult); Assert.Less(child.Visual.transform.localScale.x, .6f);
            Assert.Less(child.GetComponent<CharacterController>().height, child.Stats.Height * .6f);
            Assert.False(loop.TryMate(parent, partner, out _)); Assert.False(loop.TryMate(child, partner, out _));
            var food = session.World.Foods[0]; food.Configure(session.World, 25); Place(child, food.transform.position.x + .5f, food.transform.position.z);
            float before = child.Vitals.Energy; Assert.True(child.Interaction.TryEat(food)); Assert.Greater(child.Vitals.Energy, before);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.F)); yield return null; yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); Assert.True(session.Paused);
            float savedEnergy = child.Vitals.Energy, savedHealth = child.Vitals.Health;
            Assert.True(session.TakeControl(child)); Assert.AreSame(child, session.Player); Assert.False(session.Paused);
            Assert.AreEqual(savedEnergy, child.Vitals.Energy); Assert.AreEqual(savedHealth, child.Vitals.Health);
            Assert.False(child.GetComponent<HerbivoreBrain>().enabled); Assert.True(parent.GetComponent<HerbivoreBrain>().enabled);
            Assert.False(session.TakeControl(parent)); FreezeBrains();
            Time.timeScale = 10; yield return new WaitForSeconds(child.Stats.MaturityAge + 1); Time.timeScale = 1;
            Assert.True(child.Life.Adult); Assert.AreEqual(1, child.Visual.transform.localScale.x, .001f);
            Assert.Less(child.Vitals.Energy, savedEnergy); Assert.Less(child.Motor.Speed, .1f);
            // An unrelated founder avoids relying on a parent's still-active cooldown.
            var nextPartner = session.Creatures[4];
            yield return Mate(child, nextPartner);
            Assert.AreEqual(2, loop.Births); var grandchild = ChildOf(child);
            Assert.AreEqual(2, loop.Archive.Get(grandchild.Life.Id).Generation);
            Debug.Log("WILDTYPE_GENERATIONS: M/F input, paused courtship, birth, juvenile eating/growth, transfer without refill, generation 2 verified");
        }
        [UnityTest] public IEnumerator GamepadMatingAndActualFamilyButtonTransfer()
        {
            var parent = session.Player; Place(session.Creatures[1], 2, 0);
            InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(GamepadButton.West));
            yield return null; yield return null;
            InputSystem.QueueStateEvent(pad, new GamepadState()); Assert.AreEqual(1, loop.PendingBirths);
            yield return new WaitForSeconds(2.2f); FreezeBrains(); var child = ChildOf(parent);
            InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(GamepadButton.North));
            yield return null; yield return null; InputSystem.QueueStateEvent(pad, new GamepadState());
            yield return new WaitForSecondsRealtime(.15f); Assert.True(session.Paused);
            var buttons = session.GetComponentsInChildren<UnityEngine.UI.Button>(); bool clicked = false;
            foreach (var button in buttons)
            {
                var label = button.GetComponentInChildren<TMPro.TMP_Text>();
                if (label && label.text.StartsWith(GenerationLoop.ShortId(child.Life.Id) + " ·"))
                { button.onClick.Invoke(); clicked = true; break; }
            }
            Assert.True(clicked, "The family journal must render and wire a real descendant button");
            Assert.AreSame(child, session.Player); Assert.False(session.Paused);
            Assert.True(CreatureMotor.Finite(session.orbit.transform.position));
        }
        [UnityTest, Timeout(90000)] public IEnumerator FedCreaturesStillReachTheirInheritedLifespan()
        {
            var shortLived = session.Creatures[3]; var id = shortLived.Life.Id;
            double deadline = loop.Clock + shortLived.Stats.LifespanSeconds - shortLived.Life.Age;
            Time.timeScale = 15;
            while (loop.Clock < deadline + .1)
            {
                foreach (var actor in session.Creatures) if (actor) actor.Vitals.Eat(1000);
                yield return new WaitForSeconds(5);
            }
            Time.timeScale = 1;
            Assert.False(loop.Archive.Get(id).Alive, "A well-fed creature still dies at its inherited lifespan");
            Assert.True(!shortLived || shortLived.Vitals.Dead);
            Assert.False(session.GameOver); Assert.AreSame(session.Creatures[0], session.Player);
            Debug.Log($"WILDTYPE_LIFESPAN: natural AI death verified at simulation clock {loop.Clock:0.0}; ancestry retained");
        }
        [UnityTest] public IEnumerator DeathOffersDescendantsAndNoSilentReplacement()
        {
            var parent = session.Player; yield return Mate(parent, session.Creatures[1]);
            var child = ChildOf(parent); var id = parent.Life.Id;
            parent.Vitals.Damage(100); yield return null;
            Assert.True(session.GameOver && session.Paused); Assert.AreSame(parent, session.Player);
            Assert.False(loop.Archive.Get(id).Alive); Assert.AreEqual(1, loop.LivingDescendants(id).Count);
            Assert.True(session.TakeControl(child)); Assert.False(session.GameOver); Assert.False(session.Paused);
            child.Vitals.Damage(100); yield return null;
            Assert.True(session.GameOver); Assert.AreSame(child, session.Player);
            Assert.AreEqual(0, loop.LivingDescendants(child.Life.Id).Count); Assert.False(session.TakeControl(parent));
            session.SetPaused(false); Assert.True(session.Paused);
            int seed = session.seed; session.Restart(false); yield return null; yield return new WaitForSeconds(.3f);
            Assert.AreEqual(seed, session.seed); Assert.AreEqual(13, session.Population); Assert.AreEqual(13, loop.Archive.Count);
            Assert.AreEqual(0, loop.Births); Assert.AreEqual(0, loop.PendingBirths); Assert.False(session.GameOver);
        }
        [UnityTest] public IEnumerator InterruptedCourtshipAndPausedReseedAreSafe()
        {
            var parent = session.Player; var partner = session.Creatures[1]; Place(partner, 2, 0);
            float energy = parent.Vitals.Energy; Assert.True(loop.TryMate(parent, partner, out _));
            partner.Vitals.Damage(100); yield return new WaitForSeconds(.1f);
            Assert.AreEqual(0, loop.PendingBirths); Assert.AreEqual(0, loop.Births); Assert.Greater(parent.Vitals.Energy, energy - 1);
            partner = session.Creatures[4]; Place(partner, 2, 0); Assert.True(loop.TryMate(parent, partner, out _));
            Object.Destroy(partner.gameObject); yield return new WaitForSeconds(.1f);
            Assert.AreEqual(0, loop.PendingBirths); Assert.AreEqual(0, loop.Births);
            partner = session.Creatures[7]; Place(partner, 2, 0); Assert.True(loop.TryMate(parent, partner, out _));
            Place(partner, 15, 0); yield return new WaitForSeconds(.1f); Assert.AreEqual(0, loop.PendingBirths);
            Place(partner, 2, 0); Assert.True(loop.TryMate(parent, partner, out _)); session.SetPaused(true);
            int seed = session.seed; var oldParent = parent;
            session.Restart(true); session.Restart(true); yield return null; yield return new WaitForSeconds(.3f);
            Assert.AreEqual(unchecked(seed + 7919), session.seed); Assert.False(oldParent);
            Assert.AreEqual(0, loop.PendingBirths); Assert.AreEqual(13, session.Population); Assert.AreEqual(13, loop.Archive.Count);
            Assert.False(session.Paused); Assert.False(loop.TryMate(session.Player, oldParent, out _));
        }
        [UnityTest, Timeout(120000)] public IEnumerator AutonomousMatingCapacityAndLongRunningPopulation()
        {
            // Keep player alive without injecting resources into the AI ecosystem.
            foreach (var actor in session.Creatures)
                if (!actor.IsPlayer) actor.GetComponent<HerbivoreBrain>().enabled = true;
            int peak = session.Population, peakObjects = session.Creatures.Count, playerDeaths = 0;
            Time.timeScale = 12;
            for (int i = 0; i < 70; i++)
            {
                session.Player.Vitals.Eat(1000);
                yield return new WaitForSecondsRealtime(.15f);
                if (session.GameOver)
                {
                    playerDeaths++;
                    // The player is not artificially replaced. The soak stops at genuine extinction/old age.
                    break;
                }
                peak = Mathf.Max(peak, session.Population); peakObjects = Mathf.Max(peakObjects, session.Creatures.Count);
                Assert.LessOrEqual(session.Population + loop.PendingBirths, GenerationLoop.PopulationCap);
                Assert.LessOrEqual(session.Creatures.Count + loop.PendingBirths, GenerationLoop.ObjectCap);
                Assert.LessOrEqual(loop.Archive.Count, LineageArchive.Capacity);
                foreach (var actor in session.Creatures) if (actor) Assert.True(CreatureMotor.Finite(actor.transform.position));
            }
            Time.timeScale = 1;
            Assert.Greater(loop.Births, 0, "Autonomous AI must produce offspring in the actual ecosystem");
            Debug.Log($"WILDTYPE_GENERATION_SOAK births={loop.Births} population={session.Population} peak={peak} objectsPeak={peakObjects} records={loop.Archive.Count} elapsed={loop.Clock - 1000:0.0}s playerDeaths={playerDeaths}");
        }
        [UnityTest, Timeout(120000)] public IEnumerator CapacityDoesNotSpendEnergyAndJuvenilesCanStarve()
        {
            // Controlled capacity fixture uses normal inherited births; never direct child spawning.
            var parent = session.Player; CreatureAgent juvenile = null;
            for (int n = 0; n < 11; n++)
            {
                FreezeBrains(); CreatureAgent first = null, partner = null;
                for (int retry = 0; retry < 25 && !partner; retry++)
                {
                    foreach (var actor in session.Creatures) actor.Vitals.Eat(1000);
                    foreach (var actor in session.Creatures)
                        if (loop.IndividualReason(actor).Length == 0)
                        { var candidate = loop.FindPartner(actor, 300, true); if (candidate) { first = actor; partner = candidate; break; } }
                    if (!partner) { Time.timeScale = 12; yield return new WaitForSeconds(5); Time.timeScale = 1; }
                }
                Assert.True(partner, "Capacity fixture must find another eligible pair");
                yield return Mate(first, partner);
                juvenile = session.Creatures[session.Creatures.Count - 1]; Place(juvenile, 15 + n * 3, 8);
            }
            Assert.AreEqual(24, session.Population);
            float energy = parent.Vitals.Energy;
            Assert.False(loop.TryMate(parent, session.Creatures[1], out string reason)); StringAssert.Contains("Population full", reason);
            Assert.AreEqual(energy, parent.Vitals.Energy);
            Assert.False(juvenile.Life.Adult);
            var childId = juvenile.Life.Id; juvenile.Vitals.Tick(10000, 0, false); yield return new WaitForSeconds(4.2f);
            Assert.False(juvenile); Assert.False(loop.Archive.Get(childId).Alive); Assert.AreEqual(23, session.Population);
            Debug.Log("WILDTYPE_CAPACITY: reached 24 through 11 inherited births; full capacity rejected without resource spend; juvenile starvation freed slot");
        }
    }
}
