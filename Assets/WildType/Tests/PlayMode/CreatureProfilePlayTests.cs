using System.Collections;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace WildType.Tests
{
    public sealed class CreatureProfilePlayTests
    {
        StageSession s;StageHud hud;CreatureAgent child;
        [UnitySetUp]public IEnumerator Setup(){yield return SceneManager.LoadSceneAsync("CreatureStage_Prototype");s=Object.FindAnyObjectByType<StageSession>();s.autoPauseOnFocusLoss=false;hud=s.GetComponent<StageHud>();yield return Refresh();Freeze();}
        [UnityTearDown]public IEnumerator Cleanup(){Time.timeScale=1;yield return null;}
        IEnumerator Refresh(){yield return new WaitForSecondsRealtime(.25f);Canvas.ForceUpdateCanvases();}
        void Freeze(){foreach(var a in s.Creatures){if(!a)continue;var brain=a.GetComponent<HerbivoreBrain>();if(brain)brain.enabled=false;a.DesiredDirection=Vector3.zero;}}
        Button B(string name)=>s.GetComponentsInChildren<Button>().First(b=>b.name==name);
        string Text(string name)=>s.GetComponentsInChildren<TMP_Text>().First(t=>t.name==name).text;
        void Fit(){foreach(var t in s.GetComponentsInChildren<TMP_Text>()){t.ForceMeshUpdate();Assert.False(t.isTextOverflowing,t.name+": "+t.text);}}
        IEnumerator Birth(string name="Alexander the Great"){s.SetPaused(false);s.Names.ShowPrompts=true;var p=s.Player;var mate=s.Creatures[1];p.Motor.Teleport(new Vector3(0,Ecosystem.Height(0,0)+.2f,0));mate.Motor.Teleport(new Vector3(2,Ecosystem.Height(2,0)+.2f,0));p.Vitals.Eat(1000);mate.Vitals.Eat(1000);Assert.True(s.Generations.TryMate(p,mate,out var reason),reason);yield return new WaitForSecondsRealtime(2.5f);Freeze();Assert.True(s.Names.Submit(name));child=s.Creatures.Last();}
        [UnityTest]public IEnumerator TreeSelectionOpensTruthfulProfileAndReturnsWithoutMovingTree()
        {
            yield return Birth();hud.ShowTree(s.Player.Life.Id);yield return Refresh();
            var position=hud.TreeScroll.content.anchoredPosition;var focus=child.Life.Id;var p=s.Player;
            s.GetComponentsInChildren<FamilyTreeCard>().First(c=>c.Id==child.Life.Id).GetComponent<Button>().onClick.Invoke();yield return Refresh();
            Assert.AreEqual(StageHud.JournalView.Profile,hud.View);Assert.AreEqual(child.Life.Id,hud.ProfileId);Assert.AreSame(p,s.Player);Assert.False(child.Life.Adult);Fit();
            string movement=CreatureProfileText.Movement(s,s.Generations.Archive.Get(child.Life.Id),child,true);StringAssert.Contains(JournalReadout.Exact(child.Stats.WalkSpeed),movement);StringAssert.Contains("Eddy",movement);StringAssert.Contains(s.Names.PersonalName(s.Creatures[1].Life.Id),movement);
            foreach(var b in s.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Profile card"))){var help=b.GetComponent<JournalHelpTarget>();help.OnPointerEnter(new PointerEventData(EventSystem.current));Assert.AreEqual(help.Explanation,Text("Profile explanation text"));Fit();EventSystem.current.SetSelectedGameObject(b.gameObject);Assert.AreEqual(help.Explanation,Text("Profile explanation text"));}
            B("Profile Details").onClick.Invoke();yield return Refresh();Assert.True(s.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("Exact Genes · Page 1")));Fit();
            hud.ShowProfile(child.Life.Id);yield return Refresh();StringAssert.DoesNotContain("idle drain",Text("Profile explanation text"));B("Back to Family Tree").onClick.Invoke();yield return Refresh();Assert.AreEqual(position,hud.TreeScroll.content.anchoredPosition);Assert.AreEqual(focus,hud.TreeFocus);
            hud.ShowProfile(child.Life.Id);yield return Refresh();B("Profile Highlight").onClick.Invoke();Assert.AreSame(child,s.Locator.Target);Assert.AreSame(p,s.Player);
            hud.ShowProfile(child.Life.Id);yield return Refresh();B("Profile Take Control").onClick.Invoke();Assert.AreSame(child,s.Player);Assert.AreEqual("Alexander the Great",s.Names.PersonalName(child.Life.Id));Assert.AreEqual(1,s.Names.BirthOrder(child.Life.Id));
        }
        [UnityTest]public IEnumerator MissingAndDeadRecordsNeverInventStatsAndResetsClearProfile()
        {
            yield return Birth();var id=child.Life.Id;hud.ShowProfile(id);yield return Refresh();Object.Destroy(child.gameObject);yield return Refresh();
            Assert.False(s.Generations.Archive.TryDeath(id,out _));hud.ShowProfile(id);yield return Refresh();Fit();Assert.True(s.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("Unavailable · no recorded death")));
            Assert.False(s.GetComponentsInChildren<Button>().Any(b=>b.name=="Profile Take Control"||b.name=="Profile Highlight"));
            s.Generations.Archive.RecordDeath(id,CreatureDeathCause.Unknown,s.Generations.Clock);yield return Refresh();Fit();Assert.True(s.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("Deceased · cause unknown")));
            s.Restart(false);yield return Refresh();Assert.AreEqual(13,s.Generations.Archive.Count);hud.ShowTree(s.Player.Life.Id);yield return Refresh();Assert.AreEqual(StageHud.JournalView.Chronicle,hud.View);
            s.Restart(true);yield return Refresh();Assert.AreEqual("",hud.BiomeReadout.Text);Assert.AreEqual(72,s.World.Foods.Count);Assert.LessOrEqual(s.Creatures.Count,32);Assert.LessOrEqual(s.Population,24);Assert.LessOrEqual(s.Names.Count,512);
        }
        [UnityTest]public IEnumerator LongDuplicateNamesStayDistinctAndAdultComparisonsSurviveGrowth()
        {
            const string name="Alexandra the Extraordinary Fern";Assert.AreEqual(FamilyNames.NameLimit,name.Length);yield return Birth(name);var first=child;var parent=s.Player;
            string before=CreatureProfileText.Movement(s,s.Generations.Archive.Get(first.Life.Id),first,false);
            Time.timeScale=25;while(parent.Life.Cooldown>0||s.Creatures[1].Life.Cooldown>0){foreach(var a in s.Creatures)if(a)a.Vitals.Eat(1000);yield return new WaitForSeconds(2);}Time.timeScale=1;Freeze();
            Assert.True(first.Life.Adult);Assert.AreEqual(before,CreatureProfileText.Movement(s,s.Generations.Archive.Get(first.Life.Id),first,false));
            yield return Birth(name);Assert.AreNotEqual(first.Life.Id,child.Life.Id);Assert.AreEqual(name,s.Names.PersonalName(first.Life.Id));Assert.AreEqual(name,s.Names.PersonalName(child.Life.Id));
            Assert.AreEqual(1,s.Names.BirthOrder(first.Life.Id));Assert.AreEqual(2,s.Names.BirthOrder(child.Life.Id));StringAssert.Contains(GenerationLoop.ShortId(child.Life.Id),s.Names.NearbyName(child.Life.Id));
            hud.ShowTree(first.Life.Id);yield return Refresh();var card=s.GetComponentsInChildren<FamilyTreeCard>().First(c=>c.Id==first.Life.Id);StringAssert.Contains("…",card.GetComponentInChildren<TMP_Text>().text);card.OnPointerEnter(new PointerEventData(EventSystem.current));Fit();
            hud.ShowProfile(first.Life.Id);yield return Refresh();Fit();StringAssert.Contains(name,B("Profile card 0").GetComponentInChildren<TMP_Text>().text);
            B("Profile Take Control").onClick.Invoke();yield return Refresh();Assert.AreSame(first,s.Player);Fit();Assert.AreEqual("",hud.BiomeReadout.Text);
        }
    }
}
