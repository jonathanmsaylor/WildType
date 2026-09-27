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
    public sealed class ScrollableTreePlayTests
    {
        StageSession s;StageHud hud;
        [UnitySetUp] public IEnumerator Setup(){yield return SceneManager.LoadSceneAsync("CreatureStage_Prototype");s=Object.FindAnyObjectByType<StageSession>();s.autoPauseOnFocusLoss=false;yield return null;s.Names.ShowPrompts=false;hud=s.GetComponent<StageHud>();foreach(var a in s.Creatures){var b=a.GetComponent<HerbivoreBrain>();if(b)b.enabled=false;}hud.ShowTree(s.Player.Life.Id);yield return Refresh();}
        [UnityTearDown] public IEnumerator Cleanup(){Time.timeScale=1;yield return null;}
        IEnumerator Refresh(){yield return new WaitForSecondsRealtime(.2f);Canvas.ForceUpdateCanvases();}
        Button B(string name)=>s.GetComponentsInChildren<Button>().First(b=>b.name==name);
        void Fit(){foreach(var t in s.GetComponentsInChildren<TMP_Text>()){t.ForceMeshUpdate();Assert.False(t.isTextOverflowing,t.name+": "+t.text);}}
        [UnityTest] public IEnumerator All512RecordsReachableWithBoundedPoolAndNoInventedDeath()
        {
            var a=s.Generations.Archive;var parent=s.Player.Life.Id;var partner=s.Creatures[1].Life.Id;var ids=new System.Collections.Generic.List<CreatureId>();
            while(a.Count<512){int n=ids.Count;var p=n<20?parent:ids[n-20];var id=CreatureId.From("fixture-"+n.ToString("D4"));Assert.True(a.Add(CreatureLineageRecord.Child(id,a.Get(p),a.Get(partner),n,null)));ids.Add(id);}
            a.RecordDeath(ids[5],CreatureDeathCause.OldAge,600);hud.ShowTree(parent);yield return Refresh();
            Assert.AreEqual(501,hud.TreeLayout.Records.Count);Assert.AreEqual(StageHud.TreePoolCapacity,s.GetComponentsInChildren<FamilyTreeCard>(true).Length);
            var clock=System.Diagnostics.Stopwatch.StartNew();
            foreach(var id in ids){hud.CenterTree(id,false);Assert.True(s.GetComponentsInChildren<FamilyTreeCard>().Any(c=>c.Id==id));Assert.LessOrEqual(s.GetComponentsInChildren<FamilyTreeCard>().Length,12);}
            clock.Stop();Debug.Log("SCROLL_TREE 499 historical targets centered ms="+clock.ElapsedMilliseconds+" pool="+StageHud.TreePoolCapacity);
            hud.CenterTree(ids[5],true);yield return Refresh();Fit();Assert.True(s.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("Deceased")));
            hud.CenterTree(ids[6],true);yield return Refresh();Assert.False(a.TryDeath(ids[6],out _));Assert.True(s.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("Unavailable")));
            var previous=EventSystem.current.currentSelectedGameObject;var selected=hud.TreeFocus;
            hud.TreeScroll.verticalNormalizedPosition=0;yield return Refresh();
            Assert.AreEqual(selected,hud.TreeFocus,"Scrolling must not retarget actions");Assert.AreNotSame(previous,EventSystem.current.currentSelectedGameObject,"Recycled cards must release focus");
            foreach(var marker in s.GetComponentsInChildren<JournalFocusMarker>())if(marker.Marked)Assert.AreSame(marker.gameObject,EventSystem.current.currentSelectedGameObject);
            foreach(var bar in s.GetComponentsInChildren<Scrollbar>()){Assert.GreaterOrEqual(bar.size,0);Assert.LessOrEqual(bar.size,1);Assert.AreEqual(Vector2.zero,bar.handleRect.offsetMin);Assert.AreEqual(Vector2.zero,bar.handleRect.offsetMax);}
            Assert.False(s.GetComponentsInChildren<Button>().Any(b=>b.name=="Tree Highlight"||b.name=="Tree Take control"));
            float energy=s.Player.Vitals.Energy;double time=s.Generations.Clock;yield return Refresh();Assert.AreEqual(energy,s.Player.Vitals.Energy);Assert.AreEqual(time,s.Generations.Clock);
            s.Restart(false);yield return Refresh();Assert.AreEqual(13,s.Generations.Archive.Count);hud.ShowTree(s.Player.Life.Id);yield return Refresh();Assert.AreEqual(1,hud.TreeLayout.Records.Count);Fit();
            int seed=s.seed;s.Restart(true);yield return Refresh();Assert.AreNotEqual(seed,s.seed);Assert.AreEqual(72,s.World.Foods.Count);Assert.AreEqual(13,s.Population);
        }
        [UnityTest] public IEnumerator HoverAndDeviceFocusKeepIdentityAndControlOnlyOnExplicitAction()
        {
            s.SetPaused(false);var p=s.Player;var mate=s.Creatures[1];p.Motor.Teleport(new Vector3(0,Ecosystem.Height(0,0)+.2f,0));mate.Motor.Teleport(new Vector3(2,Ecosystem.Height(2,0)+.2f,0));p.Vitals.Eat(1000);mate.Vitals.Eat(1000);
            Assert.True(s.Generations.TryMate(p,mate,out var reason),reason);yield return new WaitForSeconds(2.3f);var child=s.Creatures.Last();hud.ShowTree(p.Life.Id);yield return Refresh();
            var card=s.GetComponentsInChildren<FamilyTreeCard>().First(c=>c.Id==child.Life.Id);var current=hud.TreeFocus;
            card.OnPointerEnter(new PointerEventData(EventSystem.current));Assert.True(card.Highlighted);Assert.AreEqual(current,hud.TreeFocus);Assert.AreSame(p,s.Player);
            Color hover=card.GetComponent<Image>().color;card.OnPointerExit(new PointerEventData(EventSystem.current));EventSystem.current.SetSelectedGameObject(card.gameObject);Assert.AreEqual(hover,card.GetComponent<Image>().color);Assert.AreEqual(child.Life.Id,hud.TreeFocus);Assert.AreSame(p,s.Player);Fit();
            B("Tree Highlight").onClick.Invoke();Assert.AreSame(p,s.Player);Assert.AreSame(child,s.Locator.Target);
            hud.ShowTree(p.Life.Id);yield return Refresh();hud.CenterTree(p.Life.Id,true);
            var original=InputSystem.settings;var settings=Object.Instantiate(original);settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            InputSystem.settings=settings;var keyboard=InputSystem.AddDevice<Keyboard>();var pad=InputSystem.AddDevice<Gamepad>();
            try{InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.DownArrow));yield return null;yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return Refresh();Assert.AreEqual(child.Life.Id,hud.TreeFocus);
                InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.DpadUp));yield return null;yield return null;InputSystem.QueueStateEvent(pad,new GamepadState());yield return Refresh();Assert.AreEqual(p.Life.Id,hud.TreeFocus);
            }finally{InputSystem.RemoveDevice(keyboard);InputSystem.RemoveDevice(pad);InputSystem.settings=original;Object.Destroy(settings);}
            hud.CenterTree(child.Life.Id,true);B("Tree Take control").onClick.Invoke();Assert.AreSame(child,s.Player);yield return Refresh();hud.ShowTree(p.Life.Id);yield return Refresh();Assert.False(s.GetComponentsInChildren<Button>().Any(b=>b.name=="Tree Take control"));Assert.False(s.Care.IsChild(child,p));
        }
        [UnityTest] public IEnumerator MenuAndDirectHelpKeepEveryFunctionAccessible()
        {
            Assert.False(s.GetComponentsInChildren<Button>().Any(b=>b.name=="Living descendants"||b.name=="Hide tips"||b.name=="Quit"));B("Tree events").onClick.Invoke();yield return Refresh();Assert.AreEqual(StageHud.JournalView.Events,hud.View);Fit();
            B("Getting started").onClick.Invoke();yield return Refresh();B("Hide tips").onClick.Invoke();Assert.False(hud.TipsVisible);Fit();B("Run menu").onClick.Invoke();yield return Refresh();Assert.NotNull(B("Restart same seed"));Assert.NotNull(B("Reseed new run"));Assert.NotNull(B("Quit"));Fit();
            hud.ShowDetails(s.Player.Life.Id);yield return Refresh();hud.ToggleSection(0);yield return Refresh();foreach(var target in s.GetComponentsInChildren<JournalHelpTarget>().Where(t=>t.name.StartsWith("Gene ")||t.name.StartsWith("Category "))){target.OnPointerEnter(new PointerEventData(EventSystem.current){delta=Vector2.one});Assert.AreEqual(target.Explanation,s.GetComponentsInChildren<TMP_Text>().First(t=>t.name=="Profile explanation text").text);Fit();EventSystem.current.SetSelectedGameObject(target.gameObject);Fit();}
        }
        [UnityTest] public IEnumerator EditorArrowFallbackDoesNotDoubleNavigateOrRunDuringPlay()
        {
            var keys=s.GetComponent<JournalEditorKeys>();Assert.NotNull(keys);var first=B("Chronicle");EventSystem.current.SetSelectedGameObject(first.gameObject);yield return Refresh();
            keys.Queue(MoveDirection.Down);yield return Refresh();var second=EventSystem.current.currentSelectedGameObject;Assert.AreNotSame(first.gameObject,second);
            EventSystem.current.SetSelectedGameObject(first.gameObject);yield return Refresh();keys.Queue(MoveDirection.Down);
            ExecuteEvents.Execute(first.gameObject,new AxisEventData(EventSystem.current){moveDir=MoveDirection.Down},ExecuteEvents.moveHandler);
            var expected=EventSystem.current.currentSelectedGameObject;yield return Refresh();Assert.AreSame(expected,EventSystem.current.currentSelectedGameObject);
            s.SetPaused(false);keys.Queue(MoveDirection.Down);yield return Refresh();Assert.False(s.Paused);
        }
    }
}
