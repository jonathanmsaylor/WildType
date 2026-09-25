using NUnit.Framework;
using UnityEngine;
namespace WildType.Tests
{
    public sealed class EcologyTests
    {
        [Test] public void RegionBoundariesMatchSavedTerrain()
        {
            Assert.AreEqual(Habitat.Woodland, EcologyRules.Region(new Vector3(-35.01f, 0, 0)));
            Assert.AreEqual(Habitat.Meadow, EcologyRules.Region(new Vector3(-35, 0, 0)));
            Assert.AreEqual(Habitat.Meadow, EcologyRules.Region(new Vector3(35, 0, 0)));
            Assert.AreEqual(Habitat.Dry, EcologyRules.Region(new Vector3(35.01f, 0, 0)));
        }
        [Test] public void SeededCandidatesStayInTheirRegionAndDoNotUseUnityRandom()
        {
            var state = Random.state;
            for (int seed = 0; seed < 50; seed++)
                foreach (Habitat region in System.Enum.GetValues(typeof(Habitat)))
                {
                    var a = new System.Random(seed); var b = new System.Random(seed);
                    for (int i = 0; i < 72; i++)
                    {
                        var p = Ecosystem.SiteCandidate(a, region); Assert.AreEqual(p, Ecosystem.SiteCandidate(b, region));
                        Assert.AreEqual(region, EcologyRules.Region(p)); Assert.True(CreatureMotor.Finite(p));
                        Assert.LessOrEqual(new Vector2(p.x, p.z).magnitude, 109.001f);
                    }
                }
            Assert.AreEqual(state, Random.state);
        }
        [Test] public void FoodConditionsAreDistinctFiniteAndBounded()
        {
            foreach (float v in new[] { -10f, 0, .5f, 1, 100, float.NaN, float.PositiveInfinity })
            {
                float wood = EcologyRules.Regrowth(Habitat.Woodland, v), dry = EcologyRules.Regrowth(Habitat.Dry, v);
                Assert.That(wood, Is.InRange(18, 26)); Assert.That(dry, Is.InRange(85, 115));
                Assert.Greater(dry, wood * 3);
            }
            Assert.Less(EcologyRules.Nutrition(Habitat.Woodland), EcologyRules.Nutrition(Habitat.Meadow));
            Assert.Greater(EcologyRules.Nutrition(Habitat.Dry), EcologyRules.Nutrition(Habitat.Meadow));
            Assert.AreNotEqual(EcologyRules.FruitColor(Habitat.Dry), EcologyRules.FruitColor(Habitat.Woodland));
        }
        [Test] public void SameLegGenomesReverseTravelAdvantageAcrossHabitats()
        {
            var compact = new Phenotype(new Genome { legLength = .6f }); var strider = new Phenotype(new Genome { legLength = 1.6f });
            foreach (bool sprint in new[] { false, true })
            {
                Assert.Greater(EcologyRules.TravelLimit(strider, Vector3.zero, sprint), EcologyRules.TravelLimit(compact, Vector3.zero, sprint));
                Assert.Less(EcologyRules.TravelLimit(strider, Vector3.left * 60, sprint), EcologyRules.TravelLimit(compact, Vector3.left * 60, sprint));
            }
            Debug.Log($"ECOLOGY_MATCHED_LEGS compact open={EcologyRules.TravelLimit(compact, Vector3.zero, false):F3} wood={EcologyRules.TravelLimit(compact, Vector3.left*60, false):F3}; long open={EcologyRules.TravelLimit(strider, Vector3.zero, false):F3} wood={EcologyRules.TravelLimit(strider, Vector3.left*60, false):F3}");
        }
        [Test] public void CanopyTransitionIsContinuousAndNeverAddsSpeed()
        {
            var stats = new Phenotype(new Genome()); float previous = EcologyRules.TravelLimit(stats, Vector3.left * 50, true);
            for (float x = -49.99f; x <= 50; x += .01f)
            {
                float value = EcologyRules.TravelLimit(stats, new Vector3(x, 0, 0), true);
                Assert.That(value, Is.InRange(0, stats.SprintSpeed)); Assert.Less(Mathf.Abs(value - previous), .02f); previous = value;
            }
            Assert.AreEqual(stats.SprintSpeed, EcologyRules.TravelLimit(stats, new Vector3(float.NaN, 0, 0), true));
        }
        [Test] public void LargeReservesBufferScarcityButSmallBitesRewardLowerMaintenance()
        {
            var small = new Phenotype(new Genome { bodySize = .65f }); var large = new Phenotype(new Genome { bodySize = 1.7f });
            Assert.Greater(large.MaxEnergy / large.PassiveDrain, small.MaxEnergy / small.PassiveDrain, "Larger bodies can wait longer on a full reserve");
            float bite = EcologyRules.Nutrition(Habitat.Woodland), delay = EcologyRules.Regrowth(Habitat.Woodland, .5f);
            Assert.Greater(bite - small.PassiveDrain * delay, bite - large.PassiveDrain * delay, "Small bodies keep more of each modest woodland bite");
            Assert.Greater(bite / small.MaxEnergy, bite / large.MaxEnergy, "One small meal replenishes a larger fraction of a small body");
        }
        [Test] public void RegrowthIsFrameIndependentAndInvalidTicksCannotCorruptIt()
        {
            var first = new GameObject().AddComponent<FoodPlant>(); var second = new GameObject().AddComponent<FoodPlant>();
            try
            {
                first.Configure(null, 85); second.Configure(null, 85); Assert.Greater(first.Consume(), 0); second.Consume();
                first.Tick(12); for (int i = 0; i < 120; i++) second.Tick(.1f);
                Assert.AreEqual(first.Remaining, second.Remaining, .002f);
                first.Tick(float.NaN); first.Tick(float.PositiveInfinity); first.Tick(-1); Assert.AreEqual(73, first.Remaining);
                Assert.AreEqual(0, first.Consume()); first.Tick(73); Assert.True(first.Available); Assert.AreEqual(0, first.Remaining);
            }
            finally { Object.DestroyImmediate(first.gameObject); Object.DestroyImmediate(second.gameObject); }
        }
        [Test] public void MutatedDescendantsRemainFiniteInEveryHabitatWithoutFavoredGenes()
        {
            var a = new Genome { legLength = .6f }; var b = new Genome { legLength = 1.6f }; var random = new SeededRandomSource(420);
            for (int i = 0; i < 200; i++)
            {
                var child = GenomeInheritance.CreateChild(a, b, new EvolutionSettings(), random).Genome;
                foreach (float x in new[] { -100f, -39, 0, 100 })
                {
                    var p = new Phenotype(child); float v = EcologyRules.TravelLimit(p, new Vector3(x, 0, 0), true);
                    Assert.That(v, Is.InRange(.01f, p.SprintSpeed));
                }
                a = b; b = child;
            }
        }
    }
}
