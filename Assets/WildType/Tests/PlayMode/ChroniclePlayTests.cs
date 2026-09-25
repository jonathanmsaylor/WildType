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
    public sealed class ChroniclePlayTests
    {
        StageSession session; CreatureAgent parent,child,grandchild;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return SceneManager.LoadSceneAsync("CreatureStage_Prototype"); session=Object.FindAnyObjectByType<StageSession>();
            session.autoPauseOnFocusLoss=false; session.GetComponent<PlayerInputBridge>().enabled=false;
            session.SetPaused(false); yield return null; Freeze(); parent=session.Player;
        }
        [UnityTearDown] public IEnumerator Cleanup(){if(session)session.SetPaused(false);Time.timeScale=1;yield return null;}
        void Freeze(){foreach(var a in session.Creatures){var b=a.GetComponent<HerbivoreBrain>();if(b)b.enabled=false;a.DesiredDirection=Vector3.zero;}}
        static void Place(CreatureAgent a,float x,float z)=>a.Motor.Teleport(new Vector3(x,Ecosystem.Height(x,z)+.15f,z));
        Button Button(string name)=>session.GetComponentsInChildren<Button>().First(b=>b.name==name);
        string Text()=>string.Join("\n",session.GetComponentsInChildren<TMP_Text>().Select(t=>t.text));
        IEnumerator Birth(CreatureAgent a,CreatureAgent b)
        {
            Place(a,0,0);Place(b,2,0);a.Vitals.Eat(1000);b.Vitals.Eat(1000);
            Assert.True(session.Generations.TryMate(a,b,out string reason),reason);
            yield return new WaitForSecondsRealtime(2.5f);Freeze();
        }
        IEnumerator Family()
        {
            yield return Birth(parent,session.Creatures[1]);child=session.Creatures.Last();
            Assert.True(session.Names.Submit(FamilyNames.DefaultName(CreatureId.From(session.seed+":015"))));
            Time.timeScale=10;yield return new WaitForSeconds(child.Stats.MaturityAge+.5f);Time.timeScale=1;Freeze();
            yield return Birth(child,session.Creatures[4]);grandchild=session.Creatures.Last();
            Assert.AreEqual(session.Names.PersonalName(child.Life.Id),session.Names.PersonalName(grandchild.Life.Id));
            Assert.AreNotEqual(session.Names.Label(child.Life.Id),session.Names.Label(grandchild.Life.Id));
        }
        IEnumerator Open(){session.SetPaused(true);yield return new WaitForSecondsRealtime(.2f);Button("Chronicle").onClick.Invoke();yield return new WaitForSecondsRealtime(.2f);}
        void Caps(){Assert.LessOrEqual(session.Population,24);Assert.LessOrEqual(session.Creatures.Count,32);Assert.AreEqual(72,session.World.Foods.Count);Assert.LessOrEqual(session.Generations.Archive.Count,512);}
        [UnityTest] public IEnumerator DeceasedChildAndLivingGrandchildKeepIdentityAndDistinctActions()
        {
            yield return Family(); var id=child.Life.Id; string name=session.Names.Label(id);
            child.Vitals.SpendEnergy(child.Vitals.Energy);child.Vitals.Tick(30,0,false);
            Assert.True(child.Vitals.Dead);yield return new WaitForSeconds(4.3f);Assert.False(child);
            yield return Open();string text=Text();StringAssert.Contains(name,text);StringAssert.Contains("Deceased · starvation",text);
            StringAssert.Contains("Parents #001 + #002 · Children born 1",text);StringAssert.Contains("Gen 2",text);
            var locate=session.GetComponentsInChildren<Button>().Where(b=>b.name=="Locate").ToArray();Assert.AreEqual(1,locate.Length);
            var deadCard=session.GetComponentsInChildren<Button>().First(b=>b.GetComponentInChildren<TMP_Text>().text.Contains("Deceased · starvation"));
            Assert.False(deadCard.interactable);deadCard.onClick.Invoke();Assert.AreSame(parent,session.Player);
            foreach(var label in session.GetComponentsInChildren<TMP_Text>().Where(t=>t.text.Contains("Children born")))
            {label.ForceMeshUpdate();Assert.LessOrEqual(label.preferredHeight,label.rectTransform.rect.height+1);Assert.LessOrEqual(label.preferredWidth,label.rectTransform.rect.width+1);}
            locate[0].onClick.Invoke();Assert.AreSame(grandchild,session.Locator.Target);Assert.AreSame(parent,session.Player);Assert.False(session.Care.IsChild(parent,grandchild));
            Assert.True(session.Generations.Archive.TryDeath(id,out var death));Assert.AreEqual(CreatureDeathCause.Starvation,death.Cause);Caps();
        }
        [UnityTest] public IEnumerator ControlTransferRetainsDeadAncestorsAndResetsPagesAndRunState()
        {
            yield return Family();var childId=child.Life.Id;child.Vitals.Damage(100);yield return new WaitForSeconds(4.3f);
            Assert.True(session.TakeControl(grandchild));yield return Open();
            StringAssert.Contains("Family chronicle · page 1/2",Text());Button("Next page").onClick.Invoke();yield return new WaitForSecondsRealtime(.2f);
            StringAssert.Contains("Parent · Deceased · cause unknown",Text());StringAssert.Contains(session.Names.Label(childId),Text());
            Assert.AreEqual(0,session.GetComponentsInChildren<Button>().Count(b=>b.name=="Locate"));
            session.Restart(false);yield return null;yield return new WaitForSecondsRealtime(.3f);Freeze();yield return Open();
            StringAssert.Contains("page 1/1",Text());StringAssert.DoesNotContain("Deceased",Text());Assert.AreEqual(0,session.Names.Count);
            int seed=session.seed;session.Restart(true);yield return null;yield return new WaitForSecondsRealtime(.3f);Freeze();yield return Open();
            Assert.AreNotEqual(seed,session.seed);Assert.False(session.Generations.Archive.TryDeath(childId,out _));StringAssert.Contains("page 1/1",Text());Caps();
        }
        [UnityTest] public IEnumerator DestroyedLivingCreatureIsUnavailableNotDeceased()
        {
            yield return Birth(parent,session.Creatures[1]);child=session.Creatures.Last();session.Names.Submit("Absent");var id=child.Life.Id;
            int deaths=session.Generations.Deaths;Object.Destroy(child.gameObject);yield return null;yield return null;
            yield return Open();StringAssert.Contains("Absent 1",Text());StringAssert.Contains("Unavailable · no recorded death",Text());
            Assert.False(session.Generations.Archive.TryDeath(id,out _));Assert.AreEqual(deaths,session.Generations.Deaths);
            Assert.AreEqual(0,session.GetComponentsInChildren<Button>().Count(b=>b.name=="Locate"));Caps();
        }
    }
}
