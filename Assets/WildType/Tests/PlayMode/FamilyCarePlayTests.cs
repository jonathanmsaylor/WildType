using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace WildType.Tests
{
    public sealed class FamilyCarePlayTests
    {
        StageSession session;
        CreatureAgent parent, child, partner;
        Keyboard keyboard; Gamepad pad; InputSettings original, settings;
        [UnitySetUp] public IEnumerator Setup()
        {
            original = InputSystem.settings; settings = Object.Instantiate(original); settings.hideFlags = HideFlags.DontSave;
            settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            InputSystem.settings = settings;
            yield return SceneManager.LoadSceneAsync("CreatureStage_Prototype");
            session = Object.FindAnyObjectByType<StageSession>(); session.autoPauseOnFocusLoss = false; session.SetPaused(false);
            session.Names.ShowPrompts = false; // Non-naming fixtures keep simulating; dedicated naming test enables the real modal.
            keyboard = InputSystem.AddDevice<Keyboard>(); pad = InputSystem.AddDevice<Gamepad>();
            yield return null; Freeze(); parent = session.Player; partner = session.Creatures[1]; Place(parent, 0, 0); Place(partner, 2, 0);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(pad); InputSystem.settings = original; Object.Destroy(settings);
            Time.timeScale = 1; yield return null;
        }
        void Freeze() { foreach (var a in session.Creatures) { var brain = a.GetComponent<HerbivoreBrain>(); if (brain) brain.enabled = false; a.DesiredDirection = Vector3.zero; } }
        void Place(CreatureAgent a, float x, float z) => a.Motor.Teleport(new Vector3(x, Ecosystem.Height(x, z) + .15f, z));
        IEnumerator Birth()
        {
            Assert.True(session.Generations.TryMate(parent, partner, out string reason), reason);
            yield return new WaitForSeconds(2.2f); Freeze();
            child = session.Generations.LivingDescendants(parent.Life.Id)[0]; Place(child, 0, 2);
            yield return null;
        }
        void Caps()
        {
            Assert.LessOrEqual(session.Population + session.Generations.PendingBirths, 24);
            Assert.LessOrEqual(session.Creatures.Count + session.Generations.PendingBirths, 32);
            Assert.AreEqual(72, session.World.Foods.Count); Assert.LessOrEqual(session.Generations.Archive.Count, 512);
            foreach (var a in session.Creatures) { Assert.True(CreatureMotor.Finite(a.transform.position)); Assert.That(a.Vitals.Energy, Is.InRange(0, a.Stats.MaxEnergy)); }
        }
        [UnityTest] public IEnumerator KeyboardShareHasCostNoHealNoRepeatAndPauseFreezesCooldown()
        {
            yield return Birth(); child.Vitals.Damage(20); child.Vitals.SpendEnergy(child.Vitals.Energy * .5f);
            float p = parent.Vitals.Energy, c = child.Vitals.Energy, health = child.Vitals.Health;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R)); yield return null; yield return null;
            Assert.AreEqual(1, session.Care.Shares); Assert.GreaterOrEqual(p - parent.Vitals.Energy, 18);
            Assert.That(child.Vitals.Energy - c, Is.InRange(13.9f, 14.401f)); Assert.AreEqual(health, child.Vitals.Health);
            yield return new WaitForSeconds(.2f); Assert.AreEqual(1, session.Care.Shares, "Held input must not repeatedly give");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R)); yield return null; yield return null;
            Assert.AreEqual(1, session.Care.Shares); StringAssert.Contains("Share again", session.Notice);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Tab)); yield return null; yield return null;
            Assert.True(session.Paused); float cooldown = parent.Life.CareCooldown;
            Assert.False(session.Care.TryShare(parent, child, out _)); yield return new WaitForSecondsRealtime(.2f);
            Assert.AreEqual(cooldown, parent.Life.CareCooldown);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape)); yield return null; yield return null; Assert.False(session.Paused);
            Caps();
        }
        [UnityTest] public IEnumerator InvalidTargetsCannotConsumeResourcesAndDeathInvalidatesTheSelection()
        {
            yield return Birth(); float p = parent.Vitals.Energy;
            Assert.False(session.Care.TryShare(parent, partner, out _)); Assert.AreEqual(p, parent.Vitals.Energy);
            Place(child, 0, 8); Assert.False(session.Care.TryShare(parent, child, out string reason)); StringAssert.Contains("3.5", reason);
            Assert.AreEqual(p, parent.Vitals.Energy); Place(child, 0, 2);
            var obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube); obstacle.layer = 9;
            obstacle.transform.position = (parent.transform.position + child.transform.position) * .5f + Vector3.up;
            obstacle.transform.localScale = new Vector3(1, 5, .25f); Physics.SyncTransforms();
            Assert.False(session.Care.TryShare(parent, child, out reason)); StringAssert.Contains("obstacle", reason);
            Assert.AreEqual(p, parent.Vitals.Energy); Object.Destroy(obstacle); yield return null;
            // Destruction yields a frame: normal metabolism may run before this next synchronous rejection.
            p = parent.Vitals.Energy;
            child.Vitals.Eat(1000); Assert.False(session.Care.TryShare(parent, child, out _)); Assert.AreEqual(p, parent.Vitals.Energy);
            child.Vitals.SpendEnergy(20); parent.Vitals.SpendEnergy(parent.Vitals.Energy);
            Assert.False(session.Care.TryShare(parent, child, out _)); Assert.AreEqual(0, parent.Vitals.Energy);
            parent.Vitals.Eat(1000); Assert.True(session.Locator.Select(child));
            child.Vitals.Damage(100); p = parent.Vitals.Energy;
            Assert.False(session.Locator.Target); Assert.False(session.Care.IsChild(parent, child));
            Assert.False(session.Care.TryShare(parent, child, out _)); Assert.AreEqual(p, parent.Vitals.Energy);
            Object.Destroy(child.gameObject); yield return null; yield return null;
            Assert.False(session.Care.TryShare(parent, child, out _)); Caps();
        }
        [UnityTest] public IEnumerator GamepadAndAutonomousParentsUseSamePaidTransferAndResetSafely()
        {
            yield return Birth();
            InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(GamepadButton.RightShoulder)); yield return null; yield return null;
            InputSystem.QueueStateEvent(pad, new GamepadState()); Assert.AreEqual(1, session.Care.Shares);
            child.Vitals.SpendEnergy(child.Vitals.Energy * .8f); partner.Vitals.Eat(1000);
            float p = partner.Vitals.Energy, c = child.Vitals.Energy;
            Place(partner, 2, 2); partner.GetComponent<HerbivoreBrain>().enabled = true;
            yield return new WaitForSeconds(.55f); partner.GetComponent<HerbivoreBrain>().enabled = false; partner.DesiredDirection = Vector3.zero;
            Assert.AreEqual(2, session.Care.Shares); Assert.AreEqual(1, partner.Life.CareGiven);
            Assert.GreaterOrEqual(p - partner.Vitals.Energy, 18); Assert.That(child.Vitals.Energy - c, Is.InRange(13, 14.401f));
            Assert.False(session.Care.TryAutonomous(partner));
            InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(GamepadButton.North)); yield return null; yield return null;
            InputSystem.QueueStateEvent(pad, new GamepadState()); yield return new WaitForSecondsRealtime(.2f);
            Button find = null; foreach (var b in session.GetComponentsInChildren<Button>()) if (b.name == "Locate") { find = b; break; }
            Assert.True(find); UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(find.gameObject);
            InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(GamepadButton.South)); yield return null; yield return null;
            InputSystem.QueueStateEvent(pad, new GamepadState()); Assert.AreSame(child, session.Locator.Target); Assert.False(session.Paused);
            Assert.True(session.TakeControl(child)); Assert.False(session.Locator.Target); Assert.False(session.Care.IsChild(child, parent));
            Freeze(); Caps();
            session.SetPaused(true); session.Restart(false); yield return null; yield return new WaitForSeconds(.2f); Freeze();
            Assert.AreEqual(0, session.Care.Shares); Assert.False(session.Locator.Target); Assert.AreEqual(0, session.Player.Life.CareGiven); Caps();
            int seed = session.seed; session.SetPaused(true); session.Restart(true); yield return null; yield return new WaitForSeconds(.2f); Freeze();
            Assert.AreNotEqual(seed, session.seed); Assert.AreEqual(0, session.Care.Shares); Assert.False(session.Care.IsChild(parent, child)); Caps();
        }
        [UnityTest] public IEnumerator HeartsRecognitionFindAndDeathMenuReflectActualFamily()
        {
            var cues = Object.FindAnyObjectByType<FamilyWorldCues>();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.F)); yield return null; yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return new WaitForSeconds(.2f);
            Assert.False(session.Paused); Assert.AreEqual(1, session.Generations.PendingBirths); Assert.AreEqual(1, cues.VisibleHearts);
            Canvas.ForceUpdateCanvases();
            var heart = Object.FindAnyObjectByType<CourtshipHeart>(); Assert.True(heart && heart.canvasRenderer);
            var heartMesh = heart.canvasRenderer.GetMesh(); Assert.True(heartMesh); Assert.Greater(heartMesh.vertexCount, 3);
            yield return new WaitForSeconds(2); Freeze(); child = session.Generations.LivingDescendants(parent.Life.Id)[0]; Place(child, 0, 2);
            yield return new WaitForSeconds(.2f); Assert.AreEqual(0, cues.VisibleHearts); Assert.AreEqual(1, cues.VisibleChildren);
            Assert.True(session.Care.IsChild(parent, child)); Assert.True(session.Care.IsChild(partner, child));
            Assert.False(session.Care.IsChild(session.Creatures[2], child));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Tab)); yield return null; yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return new WaitForSecondsRealtime(.2f);
            Assert.True(session.Paused); bool found = false;
            foreach (var button in session.GetComponentsInChildren<Button>()) if (button.name == "Locate") { button.onClick.Invoke(); found = true; break; }
            Assert.True(found); Assert.AreSame(child, session.Locator.Target); Assert.AreSame(parent, session.Player); Assert.False(session.Paused);
            Place(child, 0, 35); yield return new WaitForSeconds(.2f); Assert.True(cues.LocateCueVisible, "One temporary locator may extend beyond nearby range");
            parent.Vitals.Damage(100); yield return new WaitForSecondsRealtime(.2f);
            var title = GameObject.Find("Journal title").GetComponent<TMP_Text>(); StringAssert.Contains("Choose a descendant", title.text);
            Assert.True(session.TakeControl(child)); child.Vitals.Damage(100); yield return new WaitForSecondsRealtime(.2f);
            StringAssert.Contains("No living descendants", title.text); StringAssert.DoesNotContain("Choose a descendant", title.text);
            Caps();
        }
        [UnityTest] public IEnumerator MultipleChildrenGrowRemainRecognizableAndCareBuysAMealGap()
        {
            yield return Birth(); var first = child;
            // Same phenotype and initial usable reserves, without any food or healing.
            child.Vitals.SpendEnergy(child.Vitals.Energy * .8f);
            float originalEnergy = child.Vitals.Energy;
            var comparison = new GameObject("Uncared matched vitals").AddComponent<CreatureVitals>(); comparison.Configure(child.Stats);
            comparison.SpendEnergy(comparison.Energy - originalEnergy);
            Assert.True(session.Care.TryShare(parent, child, out _));
            float dt = (originalEnergy + 1) / child.Stats.PassiveDrain;
            comparison.Tick(dt, 0, false); child.Vitals.Tick(dt, 0, false);
            Assert.Less(comparison.Health, child.Vitals.Health); Assert.Greater(child.Vitals.Energy, comparison.Energy);
            Assert.AreEqual(100, child.Vitals.Health); Object.Destroy(comparison.gameObject);
            // Controlled growth/cooldown fixture feeds actors; this is not a balance survey.
            Time.timeScale = 12;
            while (parent.Life.Cooldown > 0 || partner.Life.Cooldown > 0)
            { foreach (var a in session.Creatures) a.Vitals.Eat(1000); yield return new WaitForSeconds(2); }
            Time.timeScale = 1; Freeze(); Assert.True(first.Life.Adult);
            Place(parent, 0, 0); Place(partner, 2, 0); Place(first, -2, 3);
            parent.Vitals.Eat(1000); partner.Vitals.Eat(1000);
            Assert.True(session.Generations.TryMate(parent, partner, out _)); yield return new WaitForSeconds(2.2f); Freeze();
            var second = session.Creatures[session.Creatures.Count - 1]; Place(second, 2, 2.5f);
            Assert.AreNotSame(first, second); Assert.AreEqual(2, session.Generations.LivingChildren(parent.Life.Id));
            Assert.AreEqual(1, session.Names.BirthOrder(first.Life.Id)); Assert.AreEqual(2, session.Names.BirthOrder(second.Life.Id));
            Assert.True(session.Care.IsChild(parent, first)); Assert.True(session.Care.IsChild(parent, second));
            Assert.False(session.Care.TryShare(parent, first, out string reason)); StringAssert.Contains("juvenile", reason);
            Assert.True(session.Locator.Select(second)); Assert.AreSame(second, session.Care.NearbyChild(parent));
            yield return new WaitForSeconds(.2f);
            var cues = Object.FindAnyObjectByType<FamilyWorldCues>(); Assert.That(cues.VisibleChildren, Is.InRange(2, FamilyWorldCues.CueCap));
            first.Vitals.Damage(100); Assert.False(session.Care.IsChild(parent, first)); Assert.True(session.Care.IsChild(parent, second));
            Caps();
        }
        [UnityTest] public IEnumerator NamingModalUsesNumericBirthOrderAndJournalCardsStayReadable()
        {
            session.Names.ShowPrompts = true;
            Assert.True(session.Generations.TryMate(parent, partner, out _));
            yield return new WaitForSecondsRealtime(2.6f);
            Assert.True(session.Names.HasPrompt); Assert.True(session.Paused); Freeze();
            child = session.Generations.LivingDescendants(parent.Life.Id)[0]; var id = child.Life.Id;
            var field = Object.FindAnyObjectByType<TMP_InputField>(); Assert.True(field && field.isFocused);
            float energyBefore = parent.Vitals.Energy; double clock = session.Generations.Clock;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.F, Key.R, Key.E));
            yield return null; yield return null;
            Assert.AreEqual(0, session.Care.Shares); Assert.AreEqual(0, session.Generations.PendingBirths);
            Assert.AreEqual(energyBefore, parent.Vitals.Energy); Assert.AreEqual(clock, session.Generations.Clock);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null;
            field.text = "Dave"; field.onSubmit.Invoke(field.text); yield return null;
            Assert.AreEqual("Dave 1", session.Names.PersonalName(id)); Assert.AreEqual(id, child.Life.Id);
            Assert.False(session.Paused); Assert.False(session.Names.HasPrompt);
            session.SetPaused(true); yield return new WaitForSecondsRealtime(.2f); Canvas.ForceUpdateCanvases();
            var selection = GameObject.Find("Descendants").GetComponent<RectTransform>(); Assert.AreEqual(1184, selection.sizeDelta.x);
            bool found = false;
            foreach (var label in selection.GetComponentsInChildren<TMP_Text>())
            {
                if (!label.text.Contains("Dave 1")) continue;
                found = true; Assert.GreaterOrEqual(label.fontSize, 18); label.ForceMeshUpdate(); Assert.False(label.isTextOverflowing);
                Assert.AreEqual(152, label.transform.parent.GetComponent<RectTransform>().sizeDelta.y);
            }
            Assert.True(found); Assert.True(session.TakeControl(child)); Assert.AreEqual("Dave 1", session.Names.PersonalName(id));
            session.SetPaused(true); session.Restart(false); yield return null; yield return new WaitForSecondsRealtime(.3f); Freeze();
            Assert.AreEqual(13, session.Names.Count); Assert.False(session.Names.HasPrompt);
            parent = session.Player; partner = session.Creatures[1]; Place(parent, 0, 0); Place(partner, 2, 0);
            Assert.True(session.Generations.TryMate(parent, partner, out _)); yield return new WaitForSecondsRealtime(2.6f);
            Assert.True(session.Names.HasPrompt);
            InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(GamepadButton.East)); yield return null; yield return null;
            Assert.False(session.Names.HasPrompt); Assert.False(session.Paused); Caps();
        }
        [UnityTest] public IEnumerator ContextPromptsOnlyOfferUsableActionsAndCompactPanelsFit()
        {
            var cues = Object.FindAnyObjectByType<FamilyWorldCues>();
            yield return new WaitForSeconds(.2f); Assert.True(cues.MatePromptVisible);
            Assert.False(GameObject.Find("Controls"), "No permanent bottom-left control legend");
            var lineage = GameObject.Find("Lineage").GetComponent<RectTransform>(); Assert.AreEqual(340, lineage.sizeDelta.x);
            Assert.LessOrEqual(lineage.GetComponent<Image>().color.a, .66f);
            var family = GameObject.Find("Family identity").GetComponent<TMP_Text>();
            Assert.AreEqual(24, family.fontSize); family.ForceMeshUpdate(); Assert.False(family.isTextOverflowing);
            var food = session.World.Foods[0];
            Place(parent, food.transform.position.x, food.transform.position.z - 1.5f); session.orbit.Configure(parent, session);
            parent.Vitals.Eat(1000); yield return new WaitForSeconds(.2f); Assert.False(cues.FoodPromptVisible, "Full creature has no eat prompt");
            parent.Vitals.SpendEnergy(25); yield return new WaitForSeconds(.2f); Assert.True(cues.FoodPromptVisible);
            var prompt = GameObject.Find("Food interaction prompt").GetComponent<TMP_Text>(); StringAssert.Contains("E eat", prompt.text);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E)); yield return null; yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); Assert.False(food.Available);
            yield return new WaitForSeconds(.2f); Assert.False(cues.FoodPromptVisible);
            session.SetPaused(true); yield return new WaitForSecondsRealtime(.2f); Assert.False(cues.FoodPromptVisible); Assert.False(cues.MatePromptVisible);
            family.ForceMeshUpdate(); Assert.False(family.isTextOverflowing); Caps();
        }
        [UnityTest] public IEnumerator NearbyFoodAndChildHaveExplicitSeparateActions()
        {
            yield return Birth();
            FoodPlant food = null;
            foreach (var plant in session.World.Foods) if (plant.Available) { food = plant; break; }
            Assert.True(food, "The newborn may have already eaten the first food slot"); var pos = food.transform.position;
            Place(parent, pos.x, pos.z - 1.5f); Place(child, pos.x + 1.5f, pos.z);
            session.orbit.Configure(parent, session); parent.Vitals.SpendEnergy(25); child.Vitals.SpendEnergy(30);
            yield return new WaitForSeconds(.2f);
            Assert.AreSame(food, parent.Interaction.Nearest()); Assert.AreSame(child, session.Care.NearbyChild(parent));
            Assert.AreEqual("", session.Care.Reason(parent, child));
            var prompt = GameObject.Find("Food interaction prompt").GetComponent<TMP_Text>();
            StringAssert.Contains("E eat", prompt.text); StringAssert.Contains(food.DisplayName, prompt.text);
            bool shareVisible = false;
            foreach (var text in session.GetComponentsInChildren<TMP_Text>())
                if (text.name == "Child relationship cue" && text.text.Contains("R share") && text.text.Contains(GenerationLoop.ShortId(child.Life.Id))) shareVisible = true;
            Assert.True(shareVisible, "Both usable actions identify their targets before input");
            float childEnergy = child.Vitals.Energy;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E)); yield return null; yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null;
            Assert.False(food.Available); Assert.AreEqual(0, session.Care.Shares); Assert.LessOrEqual(child.Vitals.Energy, childEnergy);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R)); yield return null; yield return null;
            Assert.AreEqual(1, session.Care.Shares); Assert.Greater(child.Vitals.Energy, childEnergy); Caps();
        }
    }
}
