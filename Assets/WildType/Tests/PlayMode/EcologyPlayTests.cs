using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace WildType.Tests
{
    public sealed class EcologyPlayTests
    {
        StageSession session;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return SceneManager.LoadSceneAsync("CreatureStage_Prototype");
            session = Object.FindAnyObjectByType<StageSession>(); session.autoPauseOnFocusLoss = false; session.SetPaused(false);
            session.GetComponent<PlayerInputBridge>().enabled = false; yield return null; Freeze();
        }
        [UnityTearDown] public IEnumerator Cleanup() { Time.timeScale = 1; if (session) session.SetPaused(false); yield return null; }
        void Freeze() { foreach (var a in session.Creatures) { var brain = a.GetComponent<HerbivoreBrain>(); if (brain) brain.enabled = false; a.DesiredDirection = Vector3.zero; } }
        void Place(CreatureAgent a, Vector3 p) { p.y = Ecosystem.Height(p.x, p.z) + .2f; a.Motor.Teleport(p); }
        [UnityTest] public IEnumerator RegionalSlotsRegrowAndResetWithoutAllocatingMoreFood()
        {
            int[] counts = new int[3]; var positions = new Vector3[72]; var delays = new float[72];
            for (int i = 0; i < 72; i++)
            {
                var f = session.World.Foods[i]; positions[i] = f.transform.position; delays[i] = f.regenerationSeconds;
                counts[(int)f.Region]++; Assert.AreEqual(f.Region, EcologyRules.Region(positions[i]));
                Assert.False(Physics.CheckSphere(positions[i] + Vector3.up, 2, Ecosystem.ObstacleMask));
                Assert.AreEqual(EcologyRules.Nutrition(f.Region), f.Consume()); Assert.AreEqual(0, f.Consume());
                f.Tick(f.regenerationSeconds - .1f); Assert.False(f.Available); f.Tick(.2f); Assert.True(f.Available);
            }
            CollectionAssert.AreEqual(new[] { 32, 12, 28 }, counts);
            var food = session.World.Foods[60]; food.Consume(); float left = food.Remaining;
            session.SetPaused(true); food.Tick(100); Assert.AreEqual(0, food.Consume());
            yield return new WaitForSecondsRealtime(.2f); Assert.AreEqual(left, food.Remaining);
            session.Restart(false); yield return null; yield return new WaitForSeconds(.1f); Freeze();
            for (int i = 0; i < 72; i++) { Assert.AreEqual(positions[i], session.World.Foods[i].transform.position); Assert.AreEqual(delays[i], session.World.Foods[i].regenerationSeconds); }
            session.SetPaused(true); session.Restart(true); yield return null; yield return new WaitForSeconds(.1f); Freeze();
            Assert.AreNotEqual(positions[20], session.World.Foods[20].transform.position);
            Assert.AreEqual(72, Object.FindObjectsByType<FoodPlant>().Length); Assert.AreEqual(0, session.Generations.Births); Assert.AreEqual(0, session.Generations.Deaths);
        }
        [UnityTest] public IEnumerator ActualMotorAndSharedForagingRespectLocalRules()
        {
            // Temporary matched-genome actors isolate leg length; no production preset changes.
            var low = Object.Instantiate(session.creaturePrefab, session.RuntimeRoot); var high = Object.Instantiate(session.creaturePrefab, session.RuntimeRoot);
            low.Configure(new Genome { legLength = .6f }, session, false); high.Configure(new Genome { legLength = 1.6f }, session, false);
            try
            {
                foreach (float x in new[] { 0f, -60 })
                {
                    // Find a clear local corridor, preserving real ground and normal motor/vitals.
                    Vector3 p = default; bool clear = false;
                    for (int z = -80; z < 80; z += 4)
                    { p = new Vector3(x, Ecosystem.Height(x, z) + 1, z); if (!Physics.CheckBox(p + Vector3.forward * 3, new Vector3(2, 2, 6), Quaternion.identity, Ecosystem.ObstacleMask)) { clear = true; break; } }
                    Assert.True(clear); Place(low, p + Vector3.left); Place(high, p + Vector3.right);
                    low.DesiredDirection = high.DesiredDirection = Vector3.forward;
                    yield return new WaitForSeconds(1.3f);
                    float ls = low.Motor.Speed, hs = high.Motor.Speed;
                    Assert.AreEqual(EcologyRules.TravelLimit(low.Stats, low.transform.position, false), ls, .25f);
                    Assert.AreEqual(EcologyRules.TravelLimit(high.Stats, high.transform.position, false), hs, .25f);
                    if (x == 0) Assert.Greater(hs, ls); else Assert.Greater(ls, hs);
                    Debug.Log($"ECOLOGY_ACTUAL_MOTOR x={x} short={ls:F3} long={hs:F3}");
                    low.DesiredDirection = high.DesiredDirection = Vector3.zero;
                }
                var player = session.Player;
                foreach (int index in new[] { 0, 32, 60 })
                {
                    var food = session.World.Foods[index]; player.Vitals.SpendEnergy(player.Vitals.Energy * .8f);
                    Place(player, food.transform.position + Vector3.back * .6f); yield return new WaitForSeconds(.1f);
                    float before = player.Vitals.Energy; Assert.True(player.Interaction.TryEat(food));
                    Assert.AreEqual(Mathf.Min(player.Stats.MaxEnergy, before + food.nutrition * player.Stats.NutritionFactor), player.Vitals.Energy, .01f);
                    Assert.False(food.Available);
                }
            }
            finally { Object.Destroy(low.gameObject); Object.Destroy(high.gameObject); }
        }
        [UnityTest, Timeout(90000)] public IEnumerator HungryAILeavesEmptyPatchAndFindsFoodAcrossBoundary()
        {
            session.Generations.enabled = false;
            foreach (var f in session.World.Foods) { f.Consume(); f.enabled = false; }
            var ai = session.Creatures[1]; var brain = ai.GetComponent<HerbivoreBrain>();
            // Known clear meadow/dry edge corridor, selected from actual scene collision.
            Vector3 start = default; bool clear = false;
            for (int z = -85; z < 85; z += 5)
            { start = new Vector3(32, Ecosystem.Height(32, z) + 1, z); if (!Physics.CheckBox(start + Vector3.right * 10, new Vector3(14, 3, 5), Quaternion.identity, Ecosystem.ObstacleMask)) { clear = true; break; } }
            Assert.True(clear); Place(ai, start); ai.Vitals.SpendEnergy(ai.Vitals.Energy * .55f);
            // No ripe food in vision; hidden just beyond vision. A successful search is not a teleport.
            var target = session.World.Foods[60]; target.transform.position = new Vector3(32 + ai.Stats.Vision + 3, 0, start.z);
            var p = target.transform.position; p.y = Ecosystem.Height(p.x, p.z) + .06f; target.transform.position = p; target.ConfigureRegion(session.World, Habitat.Dry, .5f);
            Assert.IsNull(session.World.Forage(ai)); brain.Configure(ai, 19); brain.enabled = true;
            float begin = ai.Vitals.Energy; int meals = brain.Meals; Time.timeScale = 6;
            for (int i = 0; i < 50 && brain.Meals == meals; i++) yield return new WaitForSeconds(1);
            Time.timeScale = 1;
            Debug.Log($"ECOLOGY_RELOCATION meals={brain.Meals} searches={brain.Relocations} start={start} end={ai.transform.position} destination={brain.SearchDestination} target={brain.Target} E={ai.Vitals.Energy:F2} alive={!ai.Vitals.Dead}");
            Assert.Greater(brain.Meals, meals, "AI must find and eat the initially unseen food"); Assert.Greater(ai.transform.position.x, 35);
            Assert.Greater(ai.Vitals.Energy, begin); Assert.False(target.Available); Assert.True(CreatureMotor.Finite(ai.transform.position));
            Object.Destroy(target.gameObject); yield return null; yield return new WaitForSeconds(.6f);
            Assert.True(!brain.Target || brain.Target.Available);
            // It may discover the first plant during its initial exploratory walk, before a long search.
            // Now nothing remains anywhere: verify a hungry brain commits to a longer search as well.
            ai.Vitals.SpendEnergy(ai.Vitals.Energy * .55f); int searches = brain.Relocations;
            Time.timeScale = 6; yield return new WaitForSeconds(12); Time.timeScale = 1;
            Assert.Greater(brain.Relocations, searches, "Empty local food must cause an extended search");
        }
        [UnityTest, Timeout(240000)] public IEnumerator BoundedUnassistedGenerationsReportEcologyOutcomes()
        {
            // Convert the inputless player into ordinary AI. On its death, release only the observer pause;
            // do not replace, resurrect, feed, teleport, alter genomes or change birth eligibility.
            session.Player.SetPlayer(false);
            foreach (var a in session.Creatures) a.GetComponent<HerbivoreBrain>().enabled = true;
            // Keep only the observer's dead object for camera/reference safety; it cannot eat or mate.
            var observer = session.Player; observer.Vitals.Died += () => observer.SetPlayer(true);
            var visits = new int[3]; var ids = new HashSet<string>();
            int peak = 0, peakObjects = 0, highestGeneration = 0, crossings = 0, birthsAtFirstDeath = -1;
            int starved = 0, aged = 0, unknown = 0;
            var previous = new Dictionary<string, Habitat>();
            double begin = session.Generations.Clock; Time.timeScale = 16;
            for (int tick = 0; tick < 120; tick++)
            {
                yield return new WaitForSeconds(10);
                if (session.Paused) session.SetPaused(false); Time.timeScale = 16;
                foreach (var a in session.Creatures)
                {
                    if (!a || a.Vitals.Dead) continue;
                    string id = a.Life.Id.Value; var region = EcologyRules.Region(a.transform.position); visits[(int)region]++;
                    if (previous.TryGetValue(id, out var old) && old != region) crossings++; previous[id] = region;
                    if (ids.Add(id))
                    {
                        a.Vitals.Died += () => { if (a.Vitals.DeathCause == CreatureDeathCause.Starvation) starved++; else if (a.Vitals.DeathCause == CreatureDeathCause.OldAge) aged++; else unknown++; };
                        Debug.Log($"ECOLOGY_COHORT id={id} gen={session.Generations.Archive.Get(a.Life.Id).Generation} size={a.Genome.bodySize:F3} legs={a.Genome.legLength:F3} region={region}");
                    }
                    highestGeneration = Mathf.Max(highestGeneration, session.Generations.Archive.Get(a.Life.Id).Generation);
                    Assert.True(CreatureMotor.Finite(a.transform.position)); Assert.False(float.IsNaN(a.Vitals.Energy));
                }
                peak = Mathf.Max(peak, session.Population); peakObjects = Mathf.Max(peakObjects, session.Creatures.Count);
                if (session.Generations.Deaths > 0 && birthsAtFirstDeath < 0) birthsAtFirstDeath = session.Generations.Births;
                Assert.LessOrEqual(session.Population + session.Generations.PendingBirths, 24);
                Assert.LessOrEqual(session.Creatures.Count + session.Generations.PendingBirths, 32);
                Assert.LessOrEqual(session.Generations.Archive.Count, 512); Assert.AreEqual(72, session.World.Foods.Count);
                Assert.LessOrEqual(session.Fx.ParticleCount, 512);
                if (session.Population == 0) break;
            }
            Time.timeScale = 1;
            float size = 0, legs = 0; var regionalSize = new float[3]; var regionalLegs = new float[3]; var regionalCount = new int[3];
            foreach (var a in session.Creatures) if (a && !a.Vitals.Dead)
            {
                size += a.Genome.bodySize; legs += a.Genome.legLength;
                int r = (int)EcologyRules.Region(a.transform.position); regionalCount[r]++; regionalSize[r] += a.Genome.bodySize; regionalLegs[r] += a.Genome.legLength;
            }
            for (int r = 0; r < 3; r++) Debug.Log($"ECOLOGY_FINAL_REGION {(Habitat)r} living={regionalCount[r]} meanSize={regionalSize[r]/Mathf.Max(1,regionalCount[r]):F3} meanLegs={regionalLegs[r]/Mathf.Max(1,regionalCount[r]):F3}");
            Debug.Log($"ECOLOGY_SOAK elapsed={session.Generations.Clock-begin:F1} births={session.Generations.Births} deaths={session.Generations.Deaths} living={session.Population} peak={peak} objects={peakObjects} records={session.Generations.Archive.Count} maxGen={highestGeneration} regionSamples(meadow,dry,wood)={string.Join(",", visits)} crossings={crossings} finalMeanSize={size/Mathf.Max(1,session.Population):F3} finalMeanLegs={legs/Mathf.Max(1,session.Population):F3}");
            Debug.Log($"ECOLOGY_OBSERVED_CAUSES starvation={starved} oldAge={aged} unknown={unknown}");
            Assert.Greater(session.Generations.Births, 0); Assert.Greater(session.Generations.Deaths, 0);
            Assert.Greater(session.Generations.Births, birthsAtFirstDeath, "Births continue after genuine deaths free population slots");
            Assert.GreaterOrEqual(highestGeneration, 2); Assert.Greater(crossings, 0);
        }
    }
}
