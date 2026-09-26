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
    public sealed class WelcomingPlayTests
    {
        StageSession s; StageHud hud;
        [UnitySetUp] public IEnumerator Setup(){TestFounderNaming.Automatic=false;yield return SceneManager.LoadSceneAsync("CreatureStage_Prototype");s=Object.FindAnyObjectByType<StageSession>();s.autoPauseOnFocusLoss=false;hud=s.GetComponent<StageHud>();yield return Refresh();Freeze();}
        [UnityTearDown] public IEnumerator Cleanup(){TestFounderNaming.Automatic=true;if(s&&s.Names.HasPrompt)s.Names.Submit("Eddy");Time.timeScale=1;yield return null;}
        void Freeze(){foreach(var a in s.Creatures){var brain=a.GetComponent<HerbivoreBrain>();if(brain)brain.enabled=false;a.DesiredDirection=Vector3.zero;}}
        IEnumerator Refresh(){yield return new WaitForSecondsRealtime(.2f);Canvas.ForceUpdateCanvases();}
        Button B(string name)=>s.GetComponentsInChildren<Button>().First(b=>b.name==name);
        string Text(string name)=>s.GetComponentsInChildren<TMP_Text>().First(t=>t.name==name).text;
        void Fit(){foreach(var t in s.GetComponentsInChildren<TMP_Text>()){t.ForceMeshUpdate();Assert.False(t.isTextOverflowing,$"Overflow {t.name}: {t.text}");}}
        void Accept(){Assert.True(s.Names.Submit("Eddy"));s.Names.ShowPrompts=false;}
        void Place(CreatureAgent a,float x)=>a.Motor.Teleport(new Vector3(x,Ecosystem.Height(x,0)+.15f,0));
        IEnumerator Birth(){s.SetPaused(false);Place(s.Player,0);Place(s.Creatures[1],2);s.Player.Vitals.Eat(1000);s.Creatures[1].Vitals.Eat(1000);Assert.True(s.Generations.TryMate(s.Player,s.Creatures[1],out var why),why);yield return new WaitForSecondsRealtime(2.5f);Freeze();}
        [UnityTest] public IEnumerator FounderEntryPausesAndDefaultRepeatsOnRestartAndReseed()
        {
            for(int run=0;run<3;run++){
                Assert.True(s.Names.IsFounderPrompt);Assert.True(s.Paused);Assert.AreEqual("Eddy",s.Names.PersonalName(s.Player.Life.Id));
                float energy=s.Player.Vitals.Energy;double clock=s.Generations.Clock;s.SetPaused(false);yield return Refresh();Assert.True(s.Paused);Assert.AreEqual(energy,s.Player.Vitals.Energy);Assert.AreEqual(clock,s.Generations.Clock);Fit();
                var others=s.Creatures.Skip(1).Select(a=>s.Names.PersonalName(a.Life.Id)).ToArray();B("Skip").onClick.Invoke();yield return Refresh();Assert.False(s.Paused);Assert.AreEqual("Eddy",s.Names.PersonalName(s.Player.Life.Id));CollectionAssert.AreEqual(others,s.Creatures.Skip(1).Select(a=>s.Names.PersonalName(a.Life.Id)).ToArray());
                int seed=s.seed;if(run<2){s.Restart(run==1);yield return Refresh();Freeze();Assert.AreEqual(run==1,seed!=s.seed);}
            }
        }
        [UnityTest] public IEnumerator AllGenesHaveSameMouseFocusAndSubmitHelpWithUnchangedValues()
        {
            Accept();hud.ShowDetails(s.Player.Life.Id);yield return Refresh();string genes=JsonUtility.ToJson(s.Player.Genome);float energy=s.Player.Vitals.Energy;Fit();
            Assert.AreEqual(16,s.GetComponentsInChildren<JournalHelpTarget>().Length);
            foreach(var target in s.GetComponentsInChildren<JournalHelpTarget>()){
                target.OnPointerEnter(new PointerEventData(EventSystem.current));Assert.AreEqual(target.Explanation,Text("Gene explanation"));Fit();
                EventSystem.current.SetSelectedGameObject(target.gameObject);Assert.AreEqual(target.Explanation,Text("Gene explanation"));
                target.GetComponent<Button>().onClick.Invoke();Assert.AreEqual(target.Explanation,Text("Gene explanation"));Assert.NotNull(target.GetComponent<Button>().navigation.selectOnDown);
            }
            for(int i=0;i<3;i++){B("More details").onClick.Invoke();yield return Refresh();B("Explain record").onClick.Invoke();Fit();}
            StringAssert.Contains(s.Player.Life.Id.Value,Text("Inherited traits"));Assert.AreEqual(genes,JsonUtility.ToJson(s.Player.Genome));Assert.AreEqual(energy,s.Player.Vitals.Energy);
        }
        [UnityTest] public IEnumerator WorldLabelUsesBaseNameAndDetailsDisambiguateDuplicateNames()
        {
            Accept();s.Names.ShowPrompts=true;yield return Birth();Assert.True(s.Names.HasPrompt);Assert.True(s.Names.Submit("AlexandertheGreat"));var child=s.Creatures.Last();
            Assert.AreEqual("AlexandertheGrea 1",s.Names.PersonalName(child.Life.Id));Assert.AreEqual("AlexandertheGrea",s.Names.WorldName(child.Life.Id));
            string label=FamilyWorldCues.ChildLabel(s,child,12.4f);StringAssert.Contains("AlexandertheGrea\n",label);StringAssert.Contains("12 m",label);StringAssert.Contains("Gen 1",label);Assert.False(label.Contains("energy"));Assert.False(label.Contains(child.Life.Id.Value));
            hud.Show(StageHud.JournalView.Living);yield return Refresh();Fit();hud.ShowDetails(child.Life.Id);yield return Refresh();Fit();for(int i=0;i<3;i++){B("More details").onClick.Invoke();yield return Refresh();Fit();}
            StringAssert.Contains(child.Life.Id.Value,Text("Inherited traits"));Assert.True(s.TakeControl(child));Assert.AreEqual("AlexandertheGrea 1",s.Names.PersonalName(s.Player.Life.Id));
            s.Player.Vitals.Damage(100);yield return Refresh();hud.ShowDetails(child.Life.Id);yield return Refresh();for(int i=0;i<3;i++){B("More details").onClick.Invoke();yield return Refresh();}StringAssert.Contains("Recorded death",Text("Inherited traits"));StringAssert.Contains("Age at death",Text("Inherited traits"));Fit();
        }
        [UnityTest] public IEnumerator KeyboardAndGamepadNavigateGeneExplanationsAndTree()
        {
            var original=InputSystem.settings;var settings=Object.Instantiate(original);settings.hideFlags=HideFlags.DontSave;
            settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            InputSystem.settings=settings;var keyboard=InputSystem.AddDevice<Keyboard>();var pad=InputSystem.AddDevice<Gamepad>();
            try{
                Accept();hud.ShowDetails(s.Player.Life.Id);yield return Refresh();
                var first=s.GetComponentsInChildren<JournalHelpTarget>().First();EventSystem.current.SetSelectedGameObject(first.gameObject);
                Assert.True(first.GetComponent<JournalFocusMarker>().Marked);
                var next=first.GetComponent<Button>().navigation.selectOnDown;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.DownArrow));yield return null;yield return null;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return Refresh();
                Assert.AreSame(next.gameObject,EventSystem.current.currentSelectedGameObject);Assert.True(next.GetComponent<JournalFocusMarker>().Marked);Assert.False(first.GetComponent<JournalFocusMarker>().Marked);Assert.AreEqual(next.GetComponent<JournalHelpTarget>().Explanation,Text("Gene explanation"));
                var third=next.navigation.selectOnDown;
                InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.DpadDown));yield return null;yield return null;
                InputSystem.QueueStateEvent(pad,new GamepadState());yield return Refresh();Assert.AreSame(third.gameObject,EventSystem.current.currentSelectedGameObject);Assert.AreEqual(third.GetComponent<JournalHelpTarget>().Explanation,Text("Gene explanation"));Fit();
                hud.ShowTree(s.Player.Life.Id);yield return Refresh();EventSystem.current.SetSelectedGameObject(B("Tree record").gameObject);
                InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South));yield return null;yield return null;
                InputSystem.QueueStateEvent(pad,new GamepadState());yield return Refresh();Assert.AreEqual(StageHud.JournalView.Details,hud.View);Fit();
            }finally{InputSystem.RemoveDevice(keyboard);InputSystem.RemoveDevice(pad);InputSystem.settings=original;Object.Destroy(settings);}
        }
        [UnityTest] public IEnumerator TreeIsBoundedPagesChildrenAndRetainsSiblingWithoutCareOrControl()
        {
            Accept();var parent=s.Player;yield return Birth();var first=s.Creatures.Last();
            Time.timeScale=30;while(parent.Life.Cooldown>0||s.Creatures[1].Life.Cooldown>0){foreach(var a in s.Creatures)a.Vitals.Eat(1000);yield return new WaitForSeconds(1);}Time.timeScale=1;
            yield return Birth();var sibling=s.Creatures.Last();Time.timeScale=30;while(parent.Life.Cooldown>0||s.Creatures[1].Life.Cooldown>0){foreach(var a in s.Creatures)a.Vitals.Eat(1000);yield return new WaitForSeconds(1);}Time.timeScale=1;yield return Birth();
            hud.ShowTree(parent.Life.Id);yield return Refresh();Fit();Assert.LessOrEqual(s.GetComponentsInChildren<Button>().Count(b=>b.name=="Tree record"),5);Assert.True(B("Next children").interactable);B("Next children").onClick.Invoke();yield return Refresh();Fit();Assert.True(B("Previous children").interactable);
            Assert.True(s.TakeControl(first));yield return Refresh();hud.ShowTree(sibling.Life.Id);yield return Refresh();Fit();Assert.AreEqual(3,s.GetComponentsInChildren<Button>().Count(b=>b.name=="Tree record"));Assert.IsFalse(s.GetComponentsInChildren<Button>().Any(b=>b.name=="Tree Take control"));Assert.False(s.Care.IsChild(first,sibling));
            B("Tree Highlight").onClick.Invoke();Assert.AreSame(first,s.Player);Assert.AreSame(sibling,s.Locator.Target);
            hud.ShowTree(first.Life.Id);yield return Refresh();B("Tree index").onClick.Invoke();yield return Refresh();Assert.AreEqual(StageHud.JournalView.Records,hud.View);Fit();
            parent.Vitals.Damage(100);yield return Refresh();hud.ShowTree(first.Life.Id);yield return Refresh();Assert.True(s.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("Deceased")));Fit();
        }
    }
}
