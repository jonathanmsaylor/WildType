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
    public sealed class ForagingDecisionPlayTests
    {
        StageSession s;StageHud hud;
        [UnitySetUp]public IEnumerator Setup(){yield return SceneManager.LoadSceneAsync("CreatureStage_Prototype");s=Object.FindAnyObjectByType<StageSession>();s.autoPauseOnFocusLoss=false;hud=s.GetComponent<StageHud>();yield return Refresh();foreach(var a in s.Creatures){var b=a.GetComponent<HerbivoreBrain>();if(b)b.enabled=false;a.DesiredDirection=Vector3.zero;}}
        [UnityTearDown]public IEnumerator Cleanup(){Time.timeScale=1;yield return null;}
        IEnumerator Refresh(){yield return new WaitForSecondsRealtime(.25f);Canvas.ForceUpdateCanvases();}
        void Fit(){foreach(var t in s.GetComponentsInChildren<TMP_Text>()){t.ForceMeshUpdate();Assert.False(t.isTextOverflowing,t.name+": "+t.text);}}
        [UnityTest]public IEnumerator ScoutRespectsSightOcclusionDepletionPauseAndDoesNotChangeEconomy(){
            var a=s.Player;var food=s.World.Foods[0];a.Vitals.SpendEnergy(50);
            Assert.True(ForageCues.WithinSight(a,food.transform.position));Assert.False(ForageCues.WithinSight(a,a.transform.position+Vector3.right*(a.Stats.Vision+1)));
            var block=GameObject.CreatePrimitive(PrimitiveType.Cube);block.layer=9;block.transform.position=(a.transform.position+food.transform.position)*.5f+Vector3.up;block.transform.localScale=new Vector3(3,4,1);Physics.SyncTransforms();Assert.False(ForageCues.WithinSight(a,food.transform.position));Object.Destroy(block);yield return null;
            float energy=a.Vitals.Energy;string genome=JsonUtility.ToJson(a.Genome);float timer=food.Remaining;string read=ForageCues.Readout(a,food,false);
            StringAssert.Contains("Ripe",read);StringAssert.Contains("estimate",read);Assert.AreEqual(energy,a.Vitals.Energy);Assert.AreEqual(timer,food.Remaining);Assert.AreEqual(genome,JsonUtility.ToJson(a.Genome));
            food.Consume();read=ForageCues.Readout(a,food,false);StringAssert.Contains("Empty",read);StringAssert.DoesNotContain(food.Remaining.ToString(),read);
            s.SetPaused(true);yield return Refresh();Assert.AreEqual(0,s.GetComponentInChildren<ForageCues>().VisibleCount);s.Restart(false);yield return Refresh();Assert.AreEqual(72,s.World.Foods.Count);Assert.LessOrEqual(s.Population,24);Assert.LessOrEqual(s.Creatures.Count,32);Assert.LessOrEqual(s.Generations.Archive.Count,512);
        }
        [UnityTest]public IEnumerator SameSetupSteadyTravelEnergyMatchesPreviewAndSprintHasARealCost(){
            s.SetPaused(true);var a=s.Player;float length=12;Vector3 end=a.transform.position+Vector3.forward*(length+a.Interaction.Reach);
            float estimate=ForageCues.WalkEstimate(a,end);var stats=a.Stats;float seconds=length/stats.WalkSpeed;
            var one=new GameObject("Walk accounting").AddComponent<CreatureVitals>();var two=new GameObject("Sprint accounting").AddComponent<CreatureVitals>();one.Configure(stats);two.Configure(stats);
            one.Tick(seconds,stats.WalkSpeed,false);two.Tick(length/stats.SprintSpeed,stats.SprintSpeed,true);
            Assert.AreEqual(stats.MaxEnergy-one.Energy,estimate,.001f);Assert.Greater(stats.MaxEnergy-two.Energy,estimate);Assert.Less(two.Stamina,one.Stamina);
            Debug.Log($"FORAGING_MATCHED steady 12m: walk {stats.MaxEnergy-one.Energy:F6} E / {seconds:F6}s; sprint {stats.MaxEnergy-two.Energy:F6} E / {length/stats.SprintSpeed:F6}s; preview {estimate:F6}. Presentation only; acceleration, hunger changes and detours excluded.");Object.Destroy(one.gameObject);Object.Destroy(two.gameObject);yield return null;
        }
        [UnityTest]public IEnumerator ApproachCueUsesVisibleMotionNotHiddenIntent(){
            s.SetPaused(true);var food=s.World.Foods[0];var grazer=s.Creatures[1];
            var at=food.transform.position+Vector3.left*4;at.y=Ecosystem.Height(at.x,at.z)+.2f;grazer.Motor.Teleport(at);
            Assert.False(ForageCues.Approaching(grazer,food),"An idle nearby grazer is not a contest claim");
            for(int i=0;i<12;i++)grazer.Motor.Tick(Vector3.right,false,grazer.Vitals,.02f);
            Assert.True(ForageCues.Approaching(grazer,food));
            s.SetPaused(false);Assert.Greater(food.Consume(),0);s.SetPaused(true);Assert.False(ForageCues.Approaching(grazer,food),"Empty food cannot be a current meal contest");
            grazer.Vitals.Damage(100);Assert.False(ForageCues.Approaching(grazer,food));yield return null;
        }
        [UnityTest]public IEnumerator SinglePageRetainsExactDataSectionsBothParentsAndReturnState(){
            s.Names.ShowPrompts=false;var p=s.Player;var mate=s.Creatures[1];p.Motor.Teleport(new Vector3(0,Ecosystem.Height(0,0)+.2f,0));mate.Motor.Teleport(new Vector3(2,Ecosystem.Height(2,0)+.2f,0));p.Vitals.Eat(1000);mate.Vitals.Eat(1000);Assert.True(s.Generations.TryMate(p,mate,out var why),why);yield return new WaitForSecondsRealtime(2.6f);var child=s.Creatures.Last();var r=s.Generations.Archive.Get(child.Life.Id);hud.ShowTree(child.Life.Id);yield return Refresh();var pos=hud.TreeScroll.content.anchoredPosition;
            hud.ShowProfile(child.Life.Id);yield return Refresh();for(int i=0;i<3;i++)Assert.False(hud.SectionOpen(i));Assert.False(s.GetComponentsInChildren<Button>(true).Any(b=>b.name=="More details"));
            Assert.AreEqual(4,s.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("Profile card")));
            foreach(var resolution in new[]{new Vector2(1920,1080),new Vector2(1366,768)}){float scale=resolution.y/1080;Assert.LessOrEqual(1560*scale,resolution.x);Fit();}
            for(int i=0;i<3;i++){hud.ToggleSection(i);yield return Refresh();Fit();}
            var texts=s.GetComponentsInChildren<TMP_Text>();string comparisons=texts.First(t=>t.name=="Parent Comparisons & Mutations record").text;StringAssert.Contains(s.Names.PersonalName(p.Life.Id),comparisons);StringAssert.Contains(s.Names.PersonalName(mate.Life.Id),comparisons);StringAssert.Contains(s.Generations.Archive.Changes(child.Life.Id),comparisons);
            for(int i=0;i<GenomeGeneCatalog.Count;i++){var gene=GenomeGeneCatalog.At(i);StringAssert.Contains(JournalReadout.Exact(GenomeGeneCatalog.Get(child.Genome,gene)),s.GetComponentsInChildren<Button>().First(b=>b.name=="Gene "+gene).GetComponentInChildren<TMP_Text>().text);}
            var last=s.GetComponentsInChildren<Button>().Last(b=>b.name.StartsWith("Gene "));EventSystem.current.SetSelectedGameObject(last.gameObject);yield return Refresh();Assert.Greater(hud.ProfileScroll.content.anchoredPosition.y,0);Fit();
            var first=s.GetComponentsInChildren<JournalHelpTarget>().First(t=>t.name.StartsWith("Gene "));var explanation=s.GetComponentsInChildren<TMP_Text>().First(t=>t.name=="Profile explanation text");
            first.OnPointerEnter(new PointerEventData(EventSystem.current));Assert.AreEqual(last.GetComponent<JournalHelpTarget>().Explanation,explanation.text,"Stationary pointer must not steal scrolled focus help");
            first.OnPointerMove(new PointerEventData(EventSystem.current){delta=Vector2.one});Assert.AreEqual(first.Explanation,explanation.text,"Deliberate mouse movement can take over");
            hud.BackToTree();yield return Refresh();Assert.AreEqual(pos,hud.TreeScroll.content.anchoredPosition);Assert.AreEqual(child.Life.Id,hud.TreeFocus);
            Object.Destroy(mate.gameObject);yield return null;string missing=InheritedChoices.Comparisons(s,r,child,0);StringAssert.Contains("unavailable",missing);StringAssert.Contains("Eddy",missing);
        }
    }
}
