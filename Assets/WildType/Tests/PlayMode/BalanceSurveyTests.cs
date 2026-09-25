using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
namespace WildType.Tests
{
    // Repeatable observation, not a fitness assertion. No meals, kills or chosen mates are injected.
    public sealed class BalanceSurveyTests
    {
        StageSession session;
        float oldCapture;
        sealed class Cohort
        {
            public float size, legs;
            public int generation, samples, hungry, damaged, starved, aged;
        }
        [UnityTest, Timeout(600000)] public IEnumerator Seed917430() => Survey(917430);
        [UnityTest, Timeout(600000)] public IEnumerator Seed925349() => Survey(925349);
        [UnityTest, Timeout(600000)] public IEnumerator Seed933268() => Survey(933268);
        [UnityTearDown] public IEnumerator Cleanup()
        {
            Time.captureDeltaTime = oldCapture; Time.timeScale = 1;
            if (session) session.SetPaused(false); yield return null;
        }
        IEnumerator Survey(int seed)
        {
            oldCapture = Time.captureDeltaTime;
            yield return SceneManager.LoadSceneAsync("CreatureStage_Prototype");
            session = Object.FindAnyObjectByType<StageSession>(); session.autoPauseOnFocusLoss = false;
            yield return null; session.seed = seed; session.Restart(false);
            yield return null; yield return null;
            session.GetComponent<PlayerInputBridge>().enabled = false;
            session.Player.SetPlayer(false);
            var observer = session.Player;
            observer.Vitals.Died += () => observer.SetPlayer(true); // dead camera anchor only, not a replacement
            Time.timeScale = 1; Time.captureDeltaTime = .1f; // identical 10 Hz decisions/render, normal 50 Hz physics
            var cohorts = new Dictionary<string, Cohort>(); var previous = new Dictionary<string, Habitat>();
            int starved = 0, aged = 0, unknown = 0, crossings = 0, peakObjects = 0, minPop = 24, maxPop = 0, maxGen = 0;
            int birthsAtFirstDeath = -1, previousBirths = 0;
            double firstDeath = -1, maxVacancyWait = 0, waitingSince = -1;
            int[] foodMin = {32,12,28}, foodMax = new int[3], visits = new int[3];
            double[] foodSum = new double[3], windowFood = new double[3];
            double drainSum = 0, passiveSum = 0; int samples = 0, windowSamples = 0;
            double begin = session.Generations.Clock;
            for (int tick = 0; tick <= 1200; tick++)
            {
                foreach (var a in session.Creatures)
                {
                    if (!a || a.Vitals.Dead) continue;
                    string id = a.Life.Id.Value;
                    if (!cohorts.TryGetValue(id, out var c))
                    {
                        c = new Cohort { size = a.Genome.bodySize, legs = a.Genome.legLength, generation = session.Generations.Archive.Get(a.Life.Id).Generation };
                        cohorts.Add(id, c); var record = c;
                        a.Vitals.Died += () => {
                            if (a.Vitals.DeathCause == CreatureDeathCause.Starvation) { starved++; record.starved++; }
                            else if (a.Vitals.DeathCause == CreatureDeathCause.OldAge) { aged++; record.aged++; } else unknown++;
                        };
                    }
                    c.samples++; if (a.Vitals.Energy < a.Stats.MaxEnergy * .15f) c.hungry++;
                    if (a.Vitals.Health < 99.99f) c.damaged++;
                    int r = (int)EcologyRules.Region(a.transform.position); visits[r]++;
                    if (previous.TryGetValue(id, out var last) && (int)last != r) crossings++;
                    previous[id] = (Habitat)r; maxGen = Mathf.Max(maxGen, c.generation);
                    passiveSum += a.Stats.PassiveDrain;
                    drainSum += a.Stats.PassiveDrain + a.Stats.MoveCost * Mathf.Clamp01(a.Motor.Speed / a.Stats.WalkSpeed)
                        + (a.Motor.Sprinting && a.Motor.Speed > .2f ? a.Stats.SprintCost : 0);
                    Assert.True(CreatureMotor.Finite(a.transform.position)); Assert.True(float.IsFinite(a.Vitals.Energy));
                }
                int[] ripe = new int[3];
                foreach (var f in session.World.Foods) { Assert.True(float.IsFinite(f.Remaining)); if (f.Available) ripe[(int)f.Region]++; }
                for (int r = 0; r < 3; r++) { foodMin[r] = Mathf.Min(foodMin[r],ripe[r]); foodMax[r] = Mathf.Max(foodMax[r],ripe[r]); foodSum[r] += ripe[r]; windowFood[r] += ripe[r]; }
                int pop = session.Population;
                if (tick > 120) minPop = Mathf.Min(minPop, pop); maxPop = Mathf.Max(maxPop,pop); peakObjects = Mathf.Max(peakObjects,session.Creatures.Count);
                if (session.Generations.Deaths > 0 && firstDeath < 0) { firstDeath = session.Generations.Clock; birthsAtFirstDeath = session.Generations.Births; }
                if (firstDeath >= 0 && pop < 24 && waitingSince < 0) waitingSince = session.Generations.Clock;
                if (firstDeath >= 0 && session.Generations.Births > previousBirths)
                {
                    if (waitingSince >= 0) maxVacancyWait = Math.Max(maxVacancyWait,session.Generations.Clock-waitingSince);
                    waitingSince = -1;
                }
                previousBirths = session.Generations.Births; samples++; windowSamples++;
                Assert.LessOrEqual(pop + session.Generations.PendingBirths,24); Assert.LessOrEqual(session.Creatures.Count + session.Generations.PendingBirths,32);
                Assert.AreEqual(72,session.World.Foods.Count); Assert.LessOrEqual(session.Generations.Archive.Count,512); Assert.LessOrEqual(session.Fx.ParticleCount,512);
                if (tick > 0 && tick % 100 == 0)
                {
                    Debug.Log($"BALANCE_WINDOW seed={seed} t={tick} pop={pop} births={session.Generations.Births} starved={starved} aged={aged} ripeMean(M,D,W)={windowFood[0]/windowSamples:F2},{windowFood[1]/windowSamples:F2},{windowFood[2]/windowSamples:F2}");
                    Array.Clear(windowFood,0,3); windowSamples=0;
                }
                if (tick == 1200 || pop == 0) break;
                yield return new WaitForSeconds(1);
            }
            for (int group = 0; group < 4; group++)
            {
                int n = 0, exposure = 0, hungry = 0, damaged = 0, starvation = 0, old = 0;
                foreach (var c in cohorts.Values)
                {
                    bool member = group == 0 ? c.size < 1 : group == 1 ? c.size >= 1 : group == 2 ? c.legs < 1.1f : c.legs >= 1.1f;
                    if (!member) continue; n++; exposure+=c.samples; hungry+=c.hungry; damaged+=c.damaged; starvation+=c.starved; old+=c.aged;
                }
                Debug.Log($"BALANCE_TRAIT seed={seed} group={group}(size<1,size>=1,legs<1.1,legs>=1.1) count={n} actorSamples={exposure} lowEnergy={hungry} damaged={damaged} starvation={starvation} oldAge={old}");
            }
            Debug.Log($"BALANCE_RESULT seed={seed} elapsed={session.Generations.Clock-begin:F1} births={session.Generations.Births} deaths={session.Generations.Deaths} starvation={starved} oldAge={aged} unknown={unknown} living={session.Population} post120PopRange={minPop}-{maxPop} objects={peakObjects} records={session.Generations.Archive.Count} maxGen={maxGen} birthsAfterFirstDeath={session.Generations.Births-birthsAtFirstDeath} longestObservedVacancyToBirth={maxVacancyWait:F1} crossings={crossings} regionSamples={string.Join(",",visits)} meanDrain={drainSum/samples:F2} meanIdleDrain={passiveSum/samples:F2}");
            for (int r = 0; r < 3; r++) Debug.Log($"BALANCE_FOOD seed={seed} region={(Habitat)r} ripeMinMeanMax={foodMin[r]}/{foodSum[r]/samples:F2}/{foodMax[r]}");
            Assert.AreEqual(session.Generations.Deaths,starved+aged+unknown,"Observe every natural death");
            Assert.Greater(session.Population,1,"Extinction is not acceptable for these smoke-test seeds");
            Assert.Greater(session.Generations.Births,birthsAtFirstDeath,"Replacement births resume without respawns");
        }
    }
}
