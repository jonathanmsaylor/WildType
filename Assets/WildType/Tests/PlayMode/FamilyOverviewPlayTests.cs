using System.Collections;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace WildType.Tests
{
    public sealed class FamilyOverviewPlayTests
    {
        StageSession s; StageHud hud;
        [UnitySetUp] public IEnumerator Setup(){yield return SceneManager.LoadSceneAsync("CreatureStage_Prototype");s=Object.FindAnyObjectByType<StageSession>();s.autoPauseOnFocusLoss=false;s.Names.ShowPrompts=false;s.SetPaused(false);hud=s.GetComponent<StageHud>();yield return null;Freeze();}
        [UnityTearDown] public IEnumerator Cleanup(){Time.timeScale=1;yield return null;}
        void Freeze(){foreach(var a in s.Creatures){var brain=a.GetComponent<HerbivoreBrain>();if(brain)brain.enabled=false;a.DesiredDirection=Vector3.zero;}}
        void Place(CreatureAgent a,Vector3 p)=>a.Motor.Teleport(new Vector3(p.x,Ecosystem.Height(p.x,p.z)+.15f,p.z));
        IEnumerator Refresh(){yield return new WaitForSecondsRealtime(.25f);Canvas.ForceUpdateCanvases();}
        Button B(string name)=>s.GetComponentsInChildren<Button>().First(b=>b.name==name);
        CreatureAgent Child(CreatureAgent a,CreatureAgent b,string id,bool adult=false)
        {var archive=s.Generations.Archive;var r=CreatureLineageRecord.Child(CreatureId.From(id),archive.Get(a.Life.Id),archive.Get(b.Life.Id),s.Generations.Clock-(adult?60:0),null);Assert.True(archive.Add(r));
            var spawn=typeof(StageSession).GetMethod("SpawnChild",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            var child=(CreatureAgent)spawn.Invoke(s,new object[]{a.Genome.Copy(),a.transform.position+Vector3.right*2,r});child.GetComponent<HerbivoreBrain>().enabled=false;return child;}
        [UnityTest] public IEnumerator RelatedPairRejectedEverywhereWithoutChargesHeartsOrReservations()
        {
            var p=s.Player;var child=Child(p,s.Creatures[1],"related",true);
            foreach(var a in s.Creatures)if(a!=p&&a!=child)Place(a,new Vector3(90,0,50));Place(p,Vector3.zero);Place(child,Vector3.right*2);
            float pe=p.Vitals.Energy,ce=child.Vitals.Energy;
            Assert.False(s.Generations.TryMate(p,child,out var why));StringAssert.Contains("Too closely related",why);
            Assert.AreEqual(pe,p.Vitals.Energy);Assert.AreEqual(ce,child.Vitals.Energy);Assert.AreEqual(0,s.Generations.PendingBirths);
            Assert.IsNull(s.Generations.FindPartner(p,5,true));Assert.IsNull(s.Generations.FindPartner(child,5,true));Assert.IsNull(s.Generations.ReadyPlayerPartner());
            Assert.False(s.Generations.TryPlayerMate());StringAssert.Contains("Too closely related",s.Notice);
            child.GetComponent<HerbivoreBrain>().enabled=true;yield return new WaitForSeconds(1);
            Assert.AreNotSame(p,child.GetComponent<HerbivoreBrain>().MateTarget);Assert.AreEqual(0,s.Generations.Births);
            var cues=s.GetComponentInChildren<FamilyWorldCues>();Assert.False(cues.MatePromptVisible);Assert.AreEqual(0,cues.VisibleHearts);
            Freeze();var unrelated=s.Creatures[4];Place(unrelated,Vector3.right*2);Place(child,Vector3.left*20);
            Assert.True(s.Generations.TryMate(p,unrelated,out why),why);float before=p.Vitals.Energy;unrelated.Vitals.Damage(100);yield return new WaitForSeconds(.1f);
            Assert.AreEqual(0,s.Generations.PendingBirths);Assert.AreEqual(0,s.Generations.Births);Assert.Greater(p.Vitals.Energy,before-1);
            var oldId=child.Life.Id;s.SetPaused(true);s.Restart(false);yield return Refresh();Assert.AreEqual(13,s.Generations.Archive.Count);Assert.IsNull(s.Generations.Archive.Get(oldId));
        }
        [UnityTest] public IEnumerator OverviewAndTreeReadLiveNeedsWithoutChangingResourcesOrControl()
        {
            var p=s.Player;var child=Child(p,s.Creatures[1],"hungry child");child.Vitals.SpendEnergy(child.Stats.MaxEnergy*.82f);child.Vitals.Damage(45);
            var second=Child(p,s.Creatures[4],"second");second.Vitals.SpendEnergy(second.Stats.MaxEnergy*.8f);
            hud.Show(StageHud.JournalView.Overview);yield return Refresh();Assert.AreEqual(2,hud.FamilyNeedCount);
            float energy=child.Vitals.Energy;double clock=s.Generations.Clock;
            B("Family need 0").onClick.Invoke();yield return Refresh();Assert.AreEqual(StageHud.JournalView.Chronicle,hud.View);Assert.AreSame(p,s.Player);
            hud.CenterTree(child.Life.Id,true);yield return Refresh();var card=s.GetComponentsInChildren<FamilyTreeCard>().First(c=>c.Id==child.Life.Id);
            Assert.AreEqual(3,card.GetComponentsInChildren<TMP_Text>().Count(t=>t.name.EndsWith("live value")));
            StringAssert.Contains("LOW",card.GetComponentsInChildren<TMP_Text>().First(t=>t.name=="Energy live value").text);
            StringAssert.Contains(JournalReadout.Whole(child.Vitals.Energy)+"/"+JournalReadout.Whole(child.Stats.MaxEnergy),card.GetComponentsInChildren<TMP_Text>().First(t=>t.name=="Energy live value").text);
            Assert.AreEqual(child.Vitals.Energy/child.Stats.MaxEnergy,card.GetComponentsInChildren<Image>().First(i=>i.name=="Energy live fill").rectTransform.localScale.x,.00001f);
            child.Vitals.Tick(.5f,child.Stats.WalkSpeed,true);yield return Refresh();
            StringAssert.Contains(JournalReadout.Whole(child.Vitals.Stamina),card.GetComponentsInChildren<TMP_Text>().First(t=>t.name=="Stamina live value").text);
            energy=child.Vitals.Energy;
            Assert.AreEqual(energy,child.Vitals.Energy);Assert.AreEqual(clock,s.Generations.Clock);
            foreach(var t in s.GetComponentsInChildren<TMP_Text>()){t.ForceMeshUpdate();Assert.False(t.isTextOverflowing,t.name+": "+t.text);}
            Assert.False(s.GetComponentsInChildren<Button>(true).Any(b=>b.name.Contains("Scroll profile")||b.name=="Explore tree"));
            child.Vitals.Eat(1000);yield return Refresh();Assert.False(card.GetComponentsInChildren<TMP_Text>().First(t=>t.name=="Energy live value").text.Contains("LOW"));
            child.Vitals.Damage(100);yield return Refresh();hud.CenterTree(child.Life.Id,false);Assert.False(card.GetComponentsInChildren<TMP_Text>().Any(t=>t.name.EndsWith("live value")));
            var id=second.Life.Id;Object.Destroy(second.gameObject);yield return Refresh();hud.CenterTree(id,false);yield return Refresh();
            card=s.GetComponentsInChildren<FamilyTreeCard>().First(c=>c.Id==id);Assert.False(card.GetComponentsInChildren<TMP_Text>().Any(t=>t.name.EndsWith("live value")));Assert.False(s.Generations.Archive.TryDeath(id,out _));
            hud.Show(StageHud.JournalView.Overview);yield return Refresh();Assert.AreEqual(0,hud.FamilyNeedCount);
            var third=Child(p,s.Creatures[7],"third");Assert.True(s.TakeControl(third));yield return Refresh();hud.Show(StageHud.JournalView.Overview);yield return Refresh();Assert.AreEqual(0,hud.FamilyNeedCount);Assert.AreSame(third,s.Player);
            int seed=s.seed;s.Restart(true);yield return Refresh();Assert.AreNotEqual(seed,s.seed);Assert.AreEqual(13,s.Population);Assert.AreEqual(72,s.World.Foods.Count);Assert.AreEqual(StageHud.JournalView.Overview,hud.View);
        }
        [UnityTest] public IEnumerator CourtshipRevalidatesRecordedKinshipBeforeBirthAndCharging()
        {
            var p=s.Player;var partner=s.Creatures[1];var child=Child(p,s.Creatures[4],"revalidation",true);
            Place(p,Vector3.zero);Place(partner,Vector3.right*2);Place(child,Vector3.left*20);
            Assert.True(s.Generations.TryMate(p,partner,out var why),why);
            // Adversarial identity change during a pending action, not a natural birth fixture.
            partner.AttachLife(s.Generations.Archive.Get(child.Life.Id));
            float a=p.Vitals.Energy,b=partner.Vitals.Energy;
            yield return new WaitForSeconds(.1f);
            Assert.AreEqual(0,s.Generations.PendingBirths);Assert.AreEqual(0,s.Generations.Births);
            Assert.Greater(p.Vitals.Energy,a-1);Assert.Greater(partner.Vitals.Energy,b-1);
            StringAssert.Contains("Too closely related",s.Notice);Assert.AreEqual(0,p.Life.Cooldown);
        }
        [UnityTest] public IEnumerator DirectTreeNavigationAndFoodPromptOnlyOfferRealActions()
        {
            var child=Child(s.Player,s.Creatures[1],"navigation");hud.ShowTree(s.Player.Life.Id);yield return Refresh();
            var menu=B("Chronicle");EventSystem.current.SetSelectedGameObject(menu.gameObject);ExecuteEvents.Execute(menu.gameObject,new AxisEventData(EventSystem.current){moveDir=MoveDirection.Right},ExecuteEvents.moveHandler);
            Assert.NotNull(EventSystem.current.currentSelectedGameObject.GetComponent<FamilyTreeCard>());
            hud.NavigateTree(s.Player.Life.Id,0,1);Assert.AreEqual(child.Life.Id,hud.TreeFocus);hud.NavigateTree(child.Life.Id,-1,0);Assert.AreEqual(menu.gameObject,EventSystem.current.currentSelectedGameObject);
            s.SetPaused(false);s.Player.Vitals.SpendEnergy(40);var food=s.World.Foods[0];var cues=s.GetComponentInChildren<FamilyWorldCues>();
            Place(s.Player,food.transform.position-Vector3.forward*(s.Player.Interaction.Reach*.8f));s.orbit.Configure(s.Player,s);yield return Refresh();
            Assert.AreEqual("",s.Player.Interaction.EatReason(food));
            Assert.True(cues.FoodPromptVisible);Assert.Greater(cues.FoodPromptAlpha,0);
            Place(s.Player,food.transform.position-Vector3.forward*(s.Player.Interaction.Reach+1));yield return null;
            Assert.False(cues.FoodPromptVisible);Assert.False(s.GetComponentsInChildren<TMP_Text>().Any(t=>t.name=="Food interaction prompt"&&t.text.Contains("Eat")));
            yield return Refresh();Assert.AreEqual(0,cues.FoodPromptAlpha);
            Place(s.Player,food.transform.position-Vector3.forward*(s.Player.Interaction.Reach*.95f));yield return Refresh();Assert.False(cues.FoodPromptVisible,"Outer band does not repeatedly re-offer the key");
            Place(s.Player,food.transform.position-Vector3.forward*(s.Player.Interaction.Reach*.8f));yield return Refresh();Assert.True(cues.FoodPromptVisible);
            Place(s.Player,food.transform.position-Vector3.forward*(s.Player.Interaction.Reach*.95f));yield return Refresh();Assert.True(cues.FoodPromptVisible,"Retain the prompt inside genuine eating range");
            var at=s.Player.transform.position;Place(child,at+new Vector3(-1.5f,0,1));Place(s.Creatures[1],at+new Vector3(1.5f,0,1));
            s.Player.Vitals.Eat(1000);s.Player.Vitals.SpendEnergy(12);s.Creatures[1].Vitals.Eat(1000);yield return Refresh();
            Assert.True(cues.MatePromptVisible);
            var mate=s.GetComponentsInChildren<TMP_Text>().First(t=>t.name=="Mating prompt");
            foreach(var label in s.GetComponentsInChildren<TMP_Text>().Where(t=>t.name=="Child relationship cue")){
                var delta=label.rectTransform.anchoredPosition-mate.rectTransform.anchoredPosition;
                Assert.True(Mathf.Abs(delta.x)>=480||Mathf.Abs(delta.y)>=84,"Child and mating prompts overlap");
            }
            s.SetPaused(true);yield return Refresh();Assert.False(cues.FoodPromptVisible);Assert.LessOrEqual(s.Creatures.Count,32);Assert.LessOrEqual(s.Generations.Archive.Count,512);
        }
        [UnityTest] public IEnumerator GamepadAndKeyboardCanScrollLongRecordsWithoutExtraButtons()
        {
            var original=InputSystem.settings;var settings=Object.Instantiate(original);settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            InputSystem.settings=settings;var pad=InputSystem.AddDevice<Gamepad>();var keyboard=InputSystem.AddDevice<Keyboard>();
            try {
                hud.ShowProfile(s.Player.Life.Id);yield return Refresh();hud.ToggleSection(1);yield return Refresh();
                float before=hud.ProfileScroll.content.anchoredPosition.y;
                InputSystem.QueueStateEvent(pad,new GamepadState{rightStick=Vector2.down});yield return Refresh();
                InputSystem.QueueStateEvent(pad,new GamepadState());Assert.Greater(hud.ProfileScroll.content.anchoredPosition.y,before+50);
                before=hud.ProfileScroll.content.anchoredPosition.y;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.PageDown));yield return Refresh();
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());Assert.Greater(hud.ProfileScroll.content.anchoredPosition.y,before+50);
                float energy=s.Player.Vitals.Energy;yield return Refresh();Assert.AreEqual(energy,s.Player.Vitals.Energy);
                foreach(var resolution in new[]{new Vector2(1920,1080),new Vector2(1366,768)}){
                    float scale=resolution.y/1080;Assert.LessOrEqual(1560*scale,resolution.x);Assert.LessOrEqual(900*scale,resolution.y);
                    foreach(var t in s.GetComponentsInChildren<TMP_Text>()){t.ForceMeshUpdate();Assert.False(t.isTextOverflowing,t.name+": "+t.text);}
                }
            } finally {InputSystem.RemoveDevice(pad);InputSystem.RemoveDevice(keyboard);InputSystem.settings=original;Object.Destroy(settings);}
        }
    }
}
