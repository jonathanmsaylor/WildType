using System.Collections;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace WildType.Tests
{
    public sealed class ClarityPlayTests
    {
        StageSession s;StageHud hud;Keyboard keyboard;Gamepad pad;InputSettings original,settings;
        [UnitySetUp] public IEnumerator Setup(){
            original=InputSystem.settings;settings=Object.Instantiate(original);settings.hideFlags=HideFlags.DontSave;settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            InputSystem.settings=settings;keyboard=InputSystem.AddDevice<Keyboard>();pad=InputSystem.AddDevice<Gamepad>();
            yield return SceneManager.LoadSceneAsync("CreatureStage_Prototype");s=Object.FindAnyObjectByType<StageSession>();s.autoPauseOnFocusLoss=false;s.SetPaused(false);s.Names.ShowPrompts=false;hud=s.GetComponent<StageHud>();yield return null;Freeze();
        }
        [UnityTearDown] public IEnumerator Cleanup(){InputSystem.RemoveDevice(keyboard);InputSystem.RemoveDevice(pad);InputSystem.settings=original;Object.Destroy(settings);Time.timeScale=1;yield return null;}
        void Freeze(){foreach(var a in s.Creatures){var b=a.GetComponent<HerbivoreBrain>();if(b)b.enabled=false;a.DesiredDirection=Vector3.zero;}}
        void Place(CreatureAgent a,Vector3 p)=>a.Motor.Teleport(new Vector3(p.x,Ecosystem.Height(p.x,p.z)+.15f,p.z));
        Button Button(string name)=>s.GetComponentsInChildren<Button>().First(b=>b.name==name);
        IEnumerator Refresh(){yield return new WaitForSecondsRealtime(.2f);Canvas.ForceUpdateCanvases();}
        void Fit(){foreach(var t in s.GetComponentsInChildren<TMP_Text>()){t.ForceMeshUpdate();Assert.False(t.isTextOverflowing,$"Overflow {t.name}: {t.text}");}}
        IEnumerator Birth(){Place(s.Player,Vector3.zero);Place(s.Creatures[1],Vector3.right*2);s.Player.Vitals.Eat(1000);s.Creatures[1].Vitals.Eat(1000);Assert.True(s.Generations.TryMate(s.Player,s.Creatures[1],out var why),why);yield return new WaitForSeconds(2.2f);Freeze();}
        [UnityTest] public IEnumerator EatMessagesAndDisplayedCareCostMatchRealTransactions()
        {
            var a=s.Player;a.Vitals.Eat(1000);Assert.False(a.Interaction.TryEat());StringAssert.Contains("full",s.Notice);
            var food=s.World.Foods.First(f=>f.Available);a.Vitals.SpendEnergy(40);Place(a,food.transform.position-Vector3.forward);
            Assert.AreEqual("",a.Interaction.EatReason(food));float before=a.Vitals.Energy;Assert.True(a.Interaction.TryEat(food));Assert.Greater(a.Vitals.Energy,before);StringAssert.Contains("energy",s.Notice);Assert.AreEqual(1,a.Interaction.MealsEaten);
            a.Vitals.SpendEnergy(10);StringAssert.Contains("regrowing",a.Interaction.EatReason(food));Place(a,new Vector3(110,0,0));Assert.False(a.Interaction.TryEat());StringAssert.Contains("No ripe food",s.Notice);
            yield return Birth();var child=s.Creatures.Last();Place(child,s.Player.transform.position+Vector3.forward*2);child.Vitals.SpendEnergy(20);a.Vitals.Eat(1000);
            Assert.True(FamilyCareRules.Transfer(a.Vitals.Energy,a.Stats.MaxEnergy,child.Vitals.Energy,child.Stats.MaxEnergy,out float cost,out float gain));
            string quote=GameplayText.Care(s,a,child,"R");StringAssert.Contains($"spend {JournalReadout.Approx(cost)}",quote);StringAssert.Contains($"gains {JournalReadout.Approx(gain)}",quote);
            float p=a.Vitals.Energy,c=child.Vitals.Energy;Assert.True(s.Care.TryShare(a,child,out _));Assert.AreEqual(cost,p-a.Vitals.Energy,.0001f);Assert.AreEqual(gain,child.Vitals.Energy-c,.0001f);Assert.AreEqual("",GameplayText.Care(s,a,child,"R"));
            Assert.False(s.Care.TryPlayerShare());StringAssert.Contains("Share again",s.Notice);
            a.Vitals.SpendEnergy(a.Vitals.Energy);StringAssert.Contains("eat ripe fruit",s.Generations.IndividualReason(a));
        }
        [UnityTest] public IEnumerator GuidePauseNavigationDetailsAndResetRemainReadable()
        {
            Assert.True(hud.TipsVisible);hud.Show(StageHud.JournalView.Guide);yield return Refresh();Assert.True(s.Paused);float energy=s.Player.Vitals.Energy;double clock=s.Generations.Clock;yield return Refresh();Assert.AreEqual(energy,s.Player.Vitals.Energy);Assert.AreEqual(clock,s.Generations.Clock);Fit();
            Button("Hide tips").onClick.Invoke();Assert.False(hud.TipsVisible);
            EventSystem.current.SetSelectedGameObject(Button("Resume").gameObject);
            InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.DpadDown));yield return null;yield return null;
            InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;
            Assert.AreEqual("Living descendants",EventSystem.current.currentSelectedGameObject.name);
            InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South));yield return null;yield return null;InputSystem.QueueStateEvent(pad,new GamepadState());yield return Refresh();Assert.AreEqual(StageHud.JournalView.Living,hud.View);
            hud.ShowDetails(s.Player.Life.Id);yield return Refresh();Fit();Button("More details").onClick.Invoke();yield return Refresh();Fit();Button("More details").onClick.Invoke();yield return Refresh();Fit();
            foreach(var resolution in new[]{new Vector2(1920,1080),new Vector2(1280,720)}){
                var canvas=s.GetComponentInChildren<Canvas>();var rect=canvas.GetComponent<RectTransform>();float scale=resolution.y/1080f;var panel=GameObject.Find("Pause").GetComponent<RectTransform>();Assert.LessOrEqual(panel.sizeDelta.x*scale,resolution.x);Assert.LessOrEqual(panel.sizeDelta.y*scale,resolution.y);
                foreach(var t in panel.GetComponentsInChildren<TMP_Text>())Assert.GreaterOrEqual(t.fontSize*scale,13.3f);Fit();
            }
            s.Restart(false);yield return null;yield return Refresh();Assert.True(hud.TipsVisible);Assert.AreEqual(StageHud.JournalView.Living,hud.View);Assert.False(s.Paused);Assert.AreEqual(0,s.Player.Interaction.MealsEaten);
            hud.Show(StageHud.JournalView.Guide);int seed=s.seed;s.Restart(true);yield return null;yield return Refresh();Assert.AreNotEqual(seed,s.seed);Assert.True(hud.TipsVisible);Assert.AreEqual(13,s.Population);Assert.AreEqual(72,s.World.Foods.Count);
        }
        [UnityTest] public IEnumerator CardsSeparateHighlightControlAndSiblingEligibility()
        {
            yield return Birth();var first=s.Creatures.Last();Place(first,new Vector3(-8,0,0));Time.timeScale=20;
            while(s.Player.Life.Cooldown>0||s.Creatures[1].Life.Cooldown>0){foreach(var a in s.Creatures)a.Vitals.Eat(1000);yield return new WaitForSeconds(2);}Time.timeScale=1;
            yield return Birth();var second=s.Creatures.Last();hud.Show(StageHud.JournalView.Living);yield return Refresh();Fit();
            var before=s.Player;Button("Locate").onClick.Invoke();Assert.AreSame(before,s.Player);Assert.False(s.Paused);Assert.AreSame(first,s.Locator.Target);
            hud.Show(StageHud.JournalView.Living);yield return Refresh();Button("Take control").onClick.Invoke();Assert.AreSame(first,s.Player);Assert.False(s.Locator.Target);
            yield return Refresh();hud.Show(StageHud.JournalView.Records);yield return Refresh();Button("Next page").onClick.Invoke();yield return Refresh();Fit();
            var row=s.GetComponentsInChildren<TMP_Text>().First(t=>t.name=="Relative identity"&&t.text.Contains("Sibling"));StringAssert.Contains(s.Names.PersonalName(second.Life.Id),row.text);
            var buttons=row.transform.parent.GetComponentsInChildren<Button>(true);var take=buttons.First(b=>b.name=="Take control");Assert.False(take.interactable);Assert.False(take.gameObject.activeSelf);take.onClick.Invoke();Assert.AreSame(first,s.Player);
            buttons.First(b=>b.name=="Locate").onClick.Invoke();Assert.AreSame(second,s.Locator.Target);Assert.False(s.Care.IsChild(first,second));Assert.AreEqual("",GameplayText.Care(s,first,second,"R"));
            first.Vitals.Damage(100);yield return Refresh();Fit();StringAssert.Contains("cause unknown",GameObject.Find("Journal title").GetComponent<TMP_Text>().text);Assert.True(s.GameOver);Assert.AreEqual(0,s.Generations.LivingDescendants(first.Life.Id).Count);
        }
        [UnityTest] public IEnumerator EatKeyboardGamepadAndHeldInputDoNotRepeat()
        {
            var food=s.World.Foods.First(f=>f.Available);Place(s.Player,food.transform.position-Vector3.forward);s.Player.Vitals.SpendEnergy(70);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.E));yield return null;yield return null;
            Assert.AreEqual(1,s.Player.Interaction.MealsEaten);yield return null;Assert.AreEqual(1,s.Player.Interaction.MealsEaten);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            food=s.World.Foods.First(f=>f.Available);Place(s.Player,food.transform.position-Vector3.forward);s.Player.Vitals.SpendEnergy(40);
            InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South));yield return null;yield return null;
            Assert.AreEqual(2,s.Player.Interaction.MealsEaten);yield return null;Assert.AreEqual(2,s.Player.Interaction.MealsEaten);
            InputSystem.QueueStateEvent(pad,new GamepadState());yield return Refresh();Fit();
            s.Player.Vitals.SpendEnergy(s.Player.Vitals.Energy);s.Player.Vitals.Damage(50);yield return Refresh();Fit();
            hud.Show(StageHud.JournalView.Guide);yield return Refresh();Fit();float energy=s.Player.Vitals.Energy;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.E));yield return null;yield return null;Assert.AreEqual(2,s.Player.Interaction.MealsEaten);Assert.AreEqual(energy,s.Player.Vitals.Energy);
        }
    }
}
