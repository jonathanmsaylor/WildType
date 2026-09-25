using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace WildType.Tests
{
    public sealed class ScarcityPlayTests
    {
        [UnityTest] public IEnumerator GrazingPauseRestAndRestartPreserveBoundsAndRecover()
        {
            yield return SceneManager.LoadSceneAsync("CreatureStage_Prototype");
            var s=Object.FindAnyObjectByType<StageSession>(); s.autoPauseOnFocusLoss=false; yield return null;
            s.GetComponent<PlayerInputBridge>().enabled=false;
            foreach(var a in s.Creatures) { var b=a.GetComponent<HerbivoreBrain>(); if(b) b.enabled=false; a.DesiredDirection=Vector3.zero; }
            var p=s.World.Foods[0]; p.enabled=false; var original=p.transform.position;
            for(int i=0;i<4;i++) { p.Consume(); p.Tick(p.Remaining); }
            Assert.AreEqual(120,p.GrazingDelay); Assert.True(s.World.GrazedNearby(p.transform.position,5));
            p.Consume(); float left=p.Remaining; s.SetPaused(true); p.Tick(1000);
            yield return new WaitForSecondsRealtime(.1f); Assert.AreEqual(left,p.Remaining); Assert.AreEqual(120,p.GrazingDelay); Assert.AreEqual(0,p.Consume());
            s.SetPaused(false); p.Tick(left); Assert.True(p.Available);
            s.SetPaused(true); p.Tick(240); Assert.AreEqual(120,p.GrazingDelay);
            s.SetPaused(false); p.Tick(240); Assert.AreEqual(0,p.GrazingDelay); Assert.AreEqual(p.regenerationSeconds,p.NextRegrowthSeconds);
            p.Consume(); s.SetPaused(true); s.Restart(false); yield return null; yield return null;
            Assert.False(p); Assert.AreEqual(original,s.World.Foods[0].transform.position);
            foreach(var f in s.World.Foods) { Assert.AreEqual(0,f.GrazingDelay); Assert.True(f.Available); }
            int seed=s.seed; s.World.Foods[0].Consume(); s.SetPaused(true); s.Restart(true); yield return null; yield return null;
            Assert.AreNotEqual(seed,s.seed); Assert.AreEqual(72,Object.FindObjectsByType<FoodPlant>().Length);
            Assert.AreEqual(13,s.Population); Assert.AreEqual(13,s.Generations.Archive.Count); Assert.AreEqual(0,s.Generations.Deaths); Assert.AreEqual(0,s.Generations.Births);
            foreach(var f in s.World.Foods) { Assert.AreEqual(0,f.GrazingDelay); Assert.True(float.IsFinite(f.NextRegrowthSeconds)); }
            s.SetPaused(false);
        }
    }
}
