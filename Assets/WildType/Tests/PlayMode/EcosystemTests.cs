using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace WildType.Tests
{
    public sealed class EcosystemTests
    {
        StageSession session;
        Keyboard keyboard;
        Gamepad pad;
        Mouse mouse;
        InputSettings originalInputSettings, testInputSettings;
        int checks;
        void Check(bool condition, string description) { Assert.That(condition, Is.True, description); checks++; Debug.Log("WILDTYPE_CHECK " + checks + ": " + description); }
        [UnitySetUp] public IEnumerator Setup()
        {
            originalInputSettings = InputSystem.settings;
            testInputSettings = Object.Instantiate(originalInputSettings);
            testInputSettings.hideFlags = HideFlags.DontSave;
            testInputSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            testInputSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            InputSystem.settings = testInputSettings;
            yield return SceneManager.LoadSceneAsync("CreatureStage_Prototype");
            session = Object.FindAnyObjectByType<StageSession>();
            // Isolate the original locomotion/survival regression; GenerationTests covers the live lifecycle.
            session.Generations.enabled = false;
            session.autoPauseOnFocusLoss = false; session.SetPaused(false);
            keyboard = InputSystem.AddDevice<Keyboard>(); pad = InputSystem.AddDevice<Gamepad>(); mouse = InputSystem.AddDevice<Mouse>();
            yield return new WaitForSeconds(1);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            if (pad != null) InputSystem.RemoveDevice(pad);
            if (mouse != null) InputSystem.RemoveDevice(mouse);
            if (originalInputSettings) InputSystem.settings = originalInputSettings;
            if (testInputSettings) Object.Destroy(testInputSettings);
            Time.timeScale = 1; if (session) session.SetPaused(false);
            yield return null;
        }
        void Keys(params Key[] keys) { InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys)); }
        [UnityTest, Timeout(240000)] public IEnumerator FullVerticalSliceAndBoundedSoak()
        {
            var a = session.Player;
            Check(session.Population == 13, "Player plus twelve autonomous herbivores");
            Check(session.World.Foods.Count == 72, "Fixed food population");
            Keys(Key.Escape); yield return null; yield return null; Keys();
            Check(session.Paused, "Escape input pauses the simulation");
            yield return null; Keys(Key.Escape); yield return null; yield return null; Keys();
            Check(!session.Paused, "Escape input resumes the simulation");
            Check(a.Motor.Grounded, "Player grounded on rolling terrain");
            Check(a.Visual.GetComponentsInChildren<Renderer>().Length >= 30, "Replaceable model is a multi-part creature (including nested limbs and coat)");
            Check(session.Creatures[2].Stats.Size > session.Creatures[3].Stats.Size, "Presets create visibly different bodies");
            Check(session.Creatures[2].Stats.WalkSpeed < session.Creatures[3].Stats.WalkSpeed, "Presets change actual movement limits");
            Vector3 start = a.transform.position; float initialEnergy = a.Vitals.Energy;
            Keys(Key.W); yield return new WaitForSeconds(1);
            Debug.Log($"Input diagnostic: key={keyboard.wKey.isPressed} enabled={keyboard.enabled} current={Keyboard.current == keyboard} direction={a.DesiredDirection} speed={a.Motor.Speed} displacement={Vector3.Distance(start,a.transform.position)}");
            Check(Vector3.Distance(start, a.transform.position) > 1.5f, "Keyboard W moves the creature");
            float walk = a.Motor.Speed;
            Keys(Key.W, Key.LeftShift); yield return new WaitForSeconds(.8f);
            Check(a.Motor.Speed > walk * 1.25f, "Shift sprints faster than walking");
            Check(a.Vitals.Stamina < a.Stats.MaxStamina - 5, "Sprint spends stamina");
            Check(a.Vitals.Energy < initialEnergy, "Survival energy drains during movement");
            Keys(); yield return new WaitForSeconds(2);
            Check(a.Motor.Speed < .1f, "Brakes to rest"); Check(a.Vitals.Stamina >= a.Stats.MaxStamina - .1f, "Rest recovers stamina");
            start = a.transform.position;
            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = Vector2.right, rightStick = new Vector2(.8f, 0) });
            float yaw = session.orbit.Yaw; yield return new WaitForSeconds(.7f);
            Check(Vector3.Distance(start, a.transform.position) > 1, "Gamepad left stick moves");
            Check(Mathf.Abs(session.orbit.Yaw - yaw) > 20, "Gamepad right stick orbits");
            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = Vector2.right }.WithButton(GamepadButton.LeftStick));
            float beforePadSprint = a.Vitals.Stamina;
            yield return new WaitForSeconds(.5f);
            Check(a.WantsSprint && a.Vitals.Stamina < beforePadSprint, "Gamepad L3 sprints and spends stamina");
            InputSystem.QueueStateEvent(pad, new GamepadState());
            yield return null;
            InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(GamepadButton.Start));
            yield return null; yield return null;
            Check(session.Paused, "Gamepad Start pauses");
            InputSystem.QueueStateEvent(pad, new GamepadState()); yield return null;
            InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(GamepadButton.Start));
            yield return null; yield return null;
            Check(!session.Paused, "Gamepad Start resumes");
            InputSystem.QueueStateEvent(pad, new GamepadState());
            float zoom = session.orbit.DesiredDistance;
            InputSystem.QueueStateEvent(mouse, new MouseState { scroll = new Vector2(0, -120) });
            yield return null; yield return null;
            Check(session.orbit.DesiredDistance > zoom, "Mouse wheel zooms out");
            // The same camera sweep handles terrain, trunks, rocks and this regression obstacle.
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.layer = 9;
            wall.transform.position = a.transform.position + Vector3.up * a.Stats.Height * .72f - session.orbit.transform.forward * 3;
            wall.transform.rotation = session.orbit.transform.rotation; wall.transform.localScale = new Vector3(5, 5, .4f);
            yield return new WaitForSeconds(.3f);
            Check(session.orbit.ActualDistance < 4, "Camera retracts before a solid obstacle");
            Object.Destroy(wall); yield return new WaitForSeconds(.5f);
            Check(session.orbit.ActualDistance > 5, "Camera recovers after obstacle removal");
            for (int i = 0; i < 3; i++)
            {
                var food = session.World.Foods[i];
                a.Vitals.Tick(25, a.Stats.WalkSpeed, false);
                a.Motor.Teleport(food.transform.position + Vector3.back * .7f + Vector3.up * .2f);
                yield return new WaitForSeconds(.25f);
                float before = a.Vitals.Energy;
                if (i == 2) InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(GamepadButton.South));
                else Keys(Key.E);
                yield return null; yield return null; Keys(); InputSystem.QueueStateEvent(pad, new GamepadState());
                Check(!food.Available && a.Vitals.Energy > before, (i == 2 ? "Gamepad South" : "E") + " consumes plant " + i + " and restores energy");
                Check(food.Consume() == 0, "Depleted plant " + i + " cannot be consumed twice");
            }
            Time.timeScale = 8; yield return new WaitForSeconds(36); Time.timeScale = 1;
            Check(session.World.Foods[0].Available, "Food regrows after bounded timer");
            Check(session.World.Foods.Count == Ecosystem.FoodCap, "Regrowth does not create more objects");
            var ai = session.Creatures[1]; var brain = ai.GetComponent<HerbivoreBrain>(); var target = session.World.Foods[4];
            // Isolate this interaction from competing herbivores consuming the test target.
            foreach (var creature in session.Creatures)
                if (!creature.IsPlayer && creature != ai) { creature.GetComponent<HerbivoreBrain>().enabled = false; creature.DesiredDirection = Vector3.zero; }
            target.Configure(session.World, 25);
            float drain = ai.Stats.PassiveDrain + ai.Stats.MoveCost;
            ai.Vitals.Tick((ai.Vitals.Energy - ai.Stats.MaxEnergy * .35f) / drain, ai.Stats.WalkSpeed, false);
            ai.Motor.Teleport(target.transform.position + Vector3.right * 5 + Vector3.up * .3f);
            float hungryEnergy = ai.Vitals.Energy;
            yield return new WaitForSeconds(.55f);
            Debug.Log($"AI diagnostic: energy={ai.Vitals.Energy}/{ai.Stats.MaxEnergy} target={brain.Target} position={ai.transform.position} available={target.Available} state={ai.State}");
            Check(brain.Target || ai.Vitals.Energy > hungryEnergy, "Hungry AI senses food using vision");
            float aiEnergy = ai.Vitals.Energy;
            yield return new WaitForSeconds(6);
            Check(ai.Vitals.Energy > aiEnergy, "AI reaches and consumes food under shared survival rules");
            ai.Vitals.Tick(30, ai.Stats.WalkSpeed, false);
            yield return new WaitForSeconds(.6f);
            var stale = brain.Target;
            if (stale) { Object.Destroy(stale.gameObject); yield return new WaitForSeconds(.7f); }
            Check(!brain.Target || brain.Target.Available, "AI drops destroyed or depleted targets");
            foreach (var creature in session.Creatures) if (!creature.IsPlayer) creature.GetComponent<HerbivoreBrain>().enabled = true;
            session.SetPaused(true); start = a.transform.position; float energy = a.Vitals.Energy;
            yield return new WaitForSecondsRealtime(.3f);
            Check(a.transform.position == start && a.Vitals.Energy == energy, "Pause freezes survival and motion");
            int oldSeed = session.seed; session.Restart(false); yield return null; yield return new WaitForSeconds(.8f);
            Check(session.Ready && !session.Paused && session.seed == oldSeed && session.Population == 13, "Restart works while paused with original seed");
            session.SetPaused(true); session.Restart(true); yield return null; yield return new WaitForSeconds(.8f);
            Check(!session.Paused && session.seed != oldSeed && session.World.Foods.Count == 72, "Reseed restarts a bounded ecosystem");
            a = session.Player; initialEnergy = a.Vitals.Energy;
            // Keep the player fed while the AI ecosystem runs for ten simulated minutes.
            int baselineObjects = Object.FindObjectsByType<Transform>().Length;
            Time.timeScale = 12;
            for (int i = 0; i < 60; i++)
            {
                a.Vitals.Eat(a.Stats.MaxEnergy);
                yield return new WaitForSeconds(10);
                Check(session.Population <= 13 && session.World.Foods.Count <= 72 && session.Fx.ParticleCount <= 512, "Soak bounds at " + (i + 1) * 10 + " simulation seconds");
            }
            Time.timeScale = 1;
            int finalObjects = Object.FindObjectsByType<Transform>().Length;
            Check(finalObjects <= baselineObjects + 20, "No runaway object growth after ten simulated minutes");
            Check(CreatureMotor.Finite(a.transform.position), "No nonfinite player transform");
            ai = session.Creatures[1]; ai.Vitals.Tick(10000, 0, false);
            yield return new WaitForSeconds(4.5f); Check(!ai, "Starved AI dies and its corpse is removed");
            a.Vitals.Tick(10000, 0, false);
            yield return null;
            Check(a.Vitals.Dead && session.GameOver && session.Paused, "Player starvation enters paused game-over state");
            session.SetPaused(false); Check(session.Paused, "Game-over cannot resume a dead player");
            session.Restart(true); yield return null; yield return new WaitForSeconds(.7f);
            Check(!session.GameOver && session.Player.Vitals.Health == 100 && session.Population == 13, "Game-over restart restores living ecosystem");
            Debug.Log("WILDTYPE_PLAYTEST_COMPLETE | " + checks + " checks | 600 simulated seconds bounded soak");
        }
    }
}
