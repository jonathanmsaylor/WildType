using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace WildType.Tests
{
    public sealed class LineageContinuityPlayTests
    {
        StageSession session;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return SceneManager.LoadSceneAsync("CreatureStage_Prototype");session=Object.FindAnyObjectByType<StageSession>();
            session.autoPauseOnFocusLoss=false;session.GetComponent<PlayerInputBridge>().enabled=false;session.SetPaused(false);yield return null;Freeze();
        }
        [UnityTearDown] public IEnumerator Cleanup(){if(session)session.SetPaused(false);Time.timeScale=1;yield return null;}
        void Freeze(){foreach(var a in session.Creatures){if(!a)continue;var b=a.GetComponent<HerbivoreBrain>();if(b)b.enabled=false;a.DesiredDirection=Vector3.zero;a.WantsSprint=false;}}
        static void Place(CreatureAgent a,float x,float z)=>a.Motor.Teleport(new Vector3(x,Ecosystem.Height(x,z)+.15f,z));
        Button Button(string name)=>session.GetComponentsInChildren<Button>().First(b=>b.name==name);
        string Text()=>string.Join("\n",session.GetComponentsInChildren<TMP_Text>().Select(t=>t.text));
        IEnumerator Birth(CreatureAgent first,CreatureAgent second)
        {
            session.SetPaused(false);foreach(var a in session.Creatures)if(a&&!a.Vitals.Dead)a.Vitals.Eat(1000);
            Place(first,0,0);Place(second,2,0);int count=session.Generations.Births;
            Assert.True(session.Generations.TryMate(first,second,out string reason),reason);
            yield return new WaitForSecondsRealtime(2.5f);Freeze();Assert.AreEqual(count+1,session.Generations.Births);
            if(session.Names.HasPrompt)Assert.True(session.Names.Submit("Dave"));
            var child=session.Creatures.Last();Place(child,-8-count*3,0);
        }
        IEnumerator WaitReady(CreatureAgent first,CreatureAgent second)
        {
            foreach(var a in session.Creatures)if(a&&!a.Vitals.Dead)a.Vitals.Eat(1000);
            float seconds=Mathf.Max(first.Life.Cooldown,second.Life.Cooldown,first.Stats.MaturityAge-first.Life.Age,second.Stats.MaturityAge-second.Life.Age)+1;
            Time.timeScale=20;yield return new WaitForSeconds(seconds);Time.timeScale=1;Freeze();
        }
        IEnumerator Open(bool history=false)
        {
            session.SetPaused(true);yield return new WaitForSecondsRealtime(.2f);
            if(history){Button("Chronicle").onClick.Invoke();yield return new WaitForSecondsRealtime(.2f);}
        }
        void Audit(string phase,params CreatureId[] ids)
        {
            var archive=session.Generations.Archive;var rows=new List<CreatureLineageRecord>();session.CollectFamilyHistory(rows);
            foreach(var id in ids){var record=archive.Get(id);var actor=session.Creatures.FirstOrDefault(a=>a&&a.Life.Id==id);bool dead=archive.TryDeath(id,out var death);
                Debug.Log($"CONTINUITY {phase} id={id} parents={record.FirstParentId}+{record.SecondParentId} controlled={session.Player.Life.Id} registered={(bool)actor} alive={record.Alive} death={(dead?death.Cause.ToString():"none")} age={(dead?record.AgeAt(death.Time):actor?actor.Life.Age:-1):F2} livingList={session.Generations.IsLivingDescendant(actor,session.Player.Life.Id)} history={rows.Any(r=>r.CreatureId==id)} relation={LineageChronicle.Relationship(archive,session.Player.Life.Id,record)}");}
        }
        [UnityTest] public IEnumerator FiveChildrenAcrossPagesTransferAndNewBirthRetainEveryIdentity()
        {
            var parent=session.Player;var mate=session.Creatures[1];var children=new List<CreatureAgent>();
            for(int n=0;n<5;n++){if(n>0)yield return WaitReady(parent,mate);yield return Birth(parent,mate);children.Add(session.Creatures.Last());}
            Assert.AreEqual(5,session.Generations.LivingChildren(parent.Life.Id));Assert.AreEqual(5,session.Generations.LivingDescendants(parent.Life.Id).Count);
            yield return Open();StringAssert.Contains("Living descendants of "+session.Names.Label(parent.Life.Id),Text());StringAssert.Contains("page 1/2",Text());
            Button("Next page").onClick.Invoke();yield return new WaitForSecondsRealtime(.2f);StringAssert.Contains("Dave 5",Text());
            Audit("before transfer",children.Select(a=>a.Life.Id).ToArray());
            Assert.True(session.TakeControl(children[0]));Freeze();Assert.AreEqual(2,session.ControlledHistory.Count);
            Assert.AreEqual(0,session.Generations.LivingDescendants(children[0].Life.Id).Count);
            yield return WaitReady(children[0],session.Creatures[4]);yield return Birth(children[0],session.Creatures[4]);var grandchild=session.Creatures.Last();
            Assert.AreEqual("Dave 1",session.Names.PersonalName(grandchild.Life.Id));Assert.AreEqual(1,session.Generations.LivingDescendants(children[0].Life.Id).Count);
            yield return Open(true);Button("Next page").onClick.Invoke();yield return new WaitForSecondsRealtime(.2f);
            StringAssert.Contains("Sibling · Living",Text());StringAssert.Contains("highlight only",Text());
            var siblingCard=session.GetComponentsInChildren<TMP_Text>().First(t=>t.name=="Relative identity"&&t.text.Contains("Dave 2")).transform.parent.GetComponentsInChildren<Button>().First(b=>b.name=="Take control");
            Assert.False(siblingCard.interactable);siblingCard.onClick.Invoke();Assert.AreSame(children[0],session.Player);
            var locates=session.GetComponentsInChildren<Button>().Where(b=>b.name=="Locate").ToArray();Assert.Greater(locates.Length,0);locates[0].onClick.Invoke();
            Assert.AreSame(children[1],session.Locator.Target);Assert.AreSame(children[0],session.Player);Assert.False(session.Paused);
            Assert.False(session.Care.IsChild(children[0],children[1]));Assert.False(session.TakeControl(children[1]));
            Assert.True(session.Locator.Select(children[1]));Assert.False(session.Locator.Select(session.Creatures[3]));
            Assert.True(session.Care.IsChild(children[0],grandchild));Assert.False(session.Care.IsChild(parent,grandchild));
            Audit("after transfer",children.Select(a=>a.Life.Id).Concat(new[]{grandchild.Life.Id}).ToArray());
            var deadId=grandchild.Life.Id;grandchild.Vitals.Damage(100,CreatureDeathCause.Starvation);yield return new WaitForSeconds(4.3f);
            Audit("confirmed fixture death",deadId);Assert.False(grandchild);Assert.True(session.Generations.Archive.TryDeath(deadId,out var death));
            Assert.AreEqual(CreatureDeathCause.Starvation,death.Cause);Assert.AreEqual(0,session.Generations.LivingDescendants(children[0].Life.Id).Count);
            var family=new List<CreatureLineageRecord>();session.CollectFamilyHistory(family);Assert.True(family.Any(r=>r.CreatureId==deadId));
            string card=LineageChronicle.Card(session,session.Generations.Archive.Get(deadId),null,false);StringAssert.Contains("Deceased · starvation",card);StringAssert.Contains("Lived",card);
            Assert.LessOrEqual(session.Population,24);Assert.LessOrEqual(session.Creatures.Count,32);Assert.AreEqual(72,session.World.Foods.Count);Assert.LessOrEqual(session.Generations.Archive.Count,512);
        }
        [UnityTest] public IEnumerator FounderNamesAreStableCosmeticAndResetWithoutLeakingHistory()
        {
            Assert.AreEqual(13,session.Names.Count);var ids=session.Creatures.Select(a=>a.Life.Id).ToArray();
            var names=ids.Select(id=>session.Names.PersonalName(id)).ToArray();var genomes=session.Creatures.Select(a=>JsonUtility.ToJson(a.Genome)).ToArray();
            var random=Random.state;
            for(int n=0;n<ids.Length;n++){
                Assert.AreEqual(FamilyNames.DefaultName(ids[n]),names[n]);Assert.IsNotEmpty(names[n]);Assert.AreEqual(0,session.Names.BirthOrder(ids[n]));
                Assert.AreEqual(0,session.Generations.Archive.Get(ids[n]).Generation);session.Names.RecordFounder(ids[n]);
                StringAssert.Contains(GenerationLoop.ShortId(ids[n]),session.Names.Label(ids[n]));
            }
            Assert.AreEqual(random,Random.state);Assert.AreEqual(13,session.Names.Count);
            var founder=session.Creatures[2];founder.Vitals.Damage(100);yield return new WaitForSeconds(4.3f);
            Assert.False(founder);Assert.AreEqual(names[2],session.Names.PersonalName(ids[2]));
            yield return Birth(session.Player,session.Creatures[1]);var child=session.Creatures.Last();Assert.True(session.TakeControl(child));
            session.SetPaused(true);session.Restart(false);yield return null;yield return new WaitForSecondsRealtime(.3f);Freeze();
            Assert.AreEqual(13,session.Names.Count);Assert.AreEqual(1,session.ControlledHistory.Count);Assert.False(session.Locator.Target);
            CollectionAssert.AreEqual(names,session.Creatures.Select(a=>session.Names.PersonalName(a.Life.Id)).ToArray());
            CollectionAssert.AreEqual(genomes,session.Creatures.Select(a=>JsonUtility.ToJson(a.Genome)).ToArray());
            Assert.False(session.Generations.Archive.TryDeath(ids[2],out _));
            session.SetPaused(true);session.Restart(true);yield return null;yield return new WaitForSecondsRealtime(.3f);Freeze();
            Assert.AreEqual(13,session.Names.Count);Assert.AreEqual(1,session.ControlledHistory.Count);Assert.AreEqual("",session.Names.PersonalName(ids[0]));
            foreach(var a in session.Creatures)Assert.AreEqual(FamilyNames.DefaultName(a.Life.Id),session.Names.PersonalName(a.Life.Id));
        }
    }
}
