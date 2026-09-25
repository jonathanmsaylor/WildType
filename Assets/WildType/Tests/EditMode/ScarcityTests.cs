using NUnit.Framework;
using UnityEngine;
namespace WildType.Tests
{
    public sealed class ScarcityTests
    {
        static FoodPlant Plant(Habitat region)
        {
            var p = new GameObject("isolated food").AddComponent<FoodPlant>();
            p.fruit = new GameObject("fruit").transform; p.fruit.SetParent(p.transform);
            p.ConfigureRegion(null,region,.5f); return p;
        }
        [Test] public void RepeatedHarvestIsLocalBoundedAndDoesNotChangeNutrition()
        {
            foreach (Habitat region in System.Enum.GetValues(typeof(Habitat)))
            {
                var p = Plant(region);
                try
                {
                    for (int i=0;i<100;i++)
                    {
                        float expected = p.regenerationSeconds + (region == Habitat.Meadow ? Mathf.Min(120,i*45) : 0);
                        Assert.AreEqual(expected,p.NextRegrowthSeconds);
                        Assert.AreEqual(EcologyRules.Nutrition(region),p.Consume()); Assert.AreEqual(expected,p.Remaining);
                        float pressure = p.GrazingDelay; Assert.AreEqual(0,p.Consume()); Assert.AreEqual(pressure,p.GrazingDelay);
                        p.Tick(expected); Assert.True(p.Available); Assert.That(p.GrazingDelay,Is.InRange(0,120));
                    }
                }
                finally { Object.DestroyImmediate(p.gameObject); }
            }
        }
        [Test] public void RipeRestRecoversFastRegrowthButEmptyTimeDoesNot()
        {
            var p = Plant(Habitat.Meadow);
            try
            {
                for(int i=0;i<4;i++) { p.Consume(); p.Tick(p.Remaining); }
                Assert.AreEqual(120,p.GrazingDelay); p.Consume(); p.Tick(100); Assert.AreEqual(120,p.GrazingDelay);
                p.Tick(p.Remaining); p.Tick(120); Assert.AreEqual(60,p.GrazingDelay);
                p.Tick(120); Assert.AreEqual(0,p.GrazingDelay); Assert.AreEqual(29,p.NextRegrowthSeconds);
                Assert.AreEqual(42,p.Consume()); Assert.AreEqual(29,p.Remaining);
                p.ConfigureRegion(null,Habitat.Meadow,.5f); Assert.AreEqual(0,p.GrazingDelay); Assert.True(p.Available);
            }
            finally { Object.DestroyImmediate(p.gameObject); }
        }
        [Test] public void CrossingRecoveryTicksAreFiniteAndFrameIndependent()
        {
            var a=Plant(Habitat.Meadow); var b=Plant(Habitat.Meadow);
            try
            {
                a.Consume(); b.Consume(); a.Tick(69); for(int i=0;i<690;i++) b.Tick(.1f);
                Assert.True(a.Available && b.Available); Assert.AreEqual(25,a.GrazingDelay,.001f); Assert.AreEqual(a.GrazingDelay,b.GrazingDelay,.002f);
                foreach(float dt in new[]{float.NaN,float.PositiveInfinity,float.NegativeInfinity,-1,0}) a.Tick(dt);
                Assert.AreEqual(25,a.GrazingDelay); a.Tick(float.MaxValue); Assert.AreEqual(0,a.GrazingDelay);
                foreach(float value in new[]{float.NaN,float.PositiveInfinity,float.NegativeInfinity,-9,999})
                    Assert.That(EcologyRules.GrazingDelay(Habitat.Meadow,value),Is.InRange(0,120));
            }
            finally { Object.DestroyImmediate(a.gameObject); Object.DestroyImmediate(b.gameObject); }
        }
        [Test] public void MatchedSizeGenomesPayDifferentCostsForTheSameHarvestSchedule()
        {
            var small=new GameObject().AddComponent<CreatureVitals>(); var large=new GameObject().AddComponent<CreatureVitals>();
            var p=Plant(Habitat.Meadow);
            try
            {
                small.Configure(new Phenotype(new Genome {bodySize=.65f})); large.Configure(new Phenotype(new Genome {bodySize=1.7f}));
                Assert.Greater(large.Stats.MaxEnergy/large.Stats.PassiveDrain,small.Stats.MaxEnergy/small.Stats.PassiveDrain,"A full large reserve buffers a longer complete gap");
                // One shared plant schedule, two independent consumers: isolates size, not an evolving population.
                float elapsed=0;
                for(int meal=0;meal<4;meal++)
                {
                    p.Consume(); float delay=p.Remaining; elapsed+=delay;
                    small.Tick(delay,0,false); large.Tick(delay,0,false); p.Tick(delay);
                    Assert.True(p.Available); small.Eat(p.nutrition); large.Eat(p.nutrition);
                }
                Assert.False(small.Dead || large.Dead); Assert.AreEqual(small.Stats.MaxEnergy,small.Energy,.01f);
                Assert.Less(large.Energy/large.Stats.MaxEnergy,.6f);
                Debug.Log($"SCARCITY_MATCHED_SIZE elapsed={elapsed} smallE={small.Energy:F2}/{small.Stats.MaxEnergy:F2} largeE={large.Energy:F2}/{large.Stats.MaxEnergy:F2}; resting full-reserve gap small={small.Stats.MaxEnergy/small.Stats.PassiveDrain:F1}s large={large.Stats.MaxEnergy/large.Stats.PassiveDrain:F1}s");
            }
            finally { Object.DestroyImmediate(small.gameObject); Object.DestroyImmediate(large.gameObject); Object.DestroyImmediate(p.gameObject); }
        }
        [Test] public void MatchedLegsExposeTravelTimeAndRealEnergyCostsWithoutAnotherModifier()
        {
            var shortLeg=new Phenotype(new Genome {legLength=.6f}); var longLeg=new Phenotype(new Genome {legLength=1.6f});
            float Cost(Phenotype p,Vector3 region) { float v=EcologyRules.TravelLimit(p,region,false); return 100/v*(p.PassiveDrain+p.MoveCost*Mathf.Clamp01(v/p.WalkSpeed)); }
            Assert.Less(Cost(longLeg,Vector3.zero),Cost(shortLeg,Vector3.zero));
            Assert.Greater(100/EcologyRules.TravelLimit(longLeg,Vector3.left*60,false),100/EcologyRules.TravelLimit(shortLeg,Vector3.left*60,false));
            Assert.Greater(Cost(longLeg,Vector3.left*60),Cost(longLeg,Vector3.zero));
            // Existing speed-scaled drain also decreases in brush: slower arrival is NOT proof of higher cost per metre than the short-legged peer.
            Debug.Log($"SCARCITY_MATCHED_TRAVEL 100m steady walk energy short/long open={Cost(shortLeg,Vector3.zero):F2}/{Cost(longLeg,Vector3.zero):F2} wood={Cost(shortLeg,Vector3.left*60):F2}/{Cost(longLeg,Vector3.left*60):F2}; seconds open={100/shortLeg.WalkSpeed:F2}/{100/longLeg.WalkSpeed:F2} wood={100/EcologyRules.TravelLimit(shortLeg,Vector3.left*60,false):F2}/{100/EcologyRules.TravelLimit(longLeg,Vector3.left*60,false):F2}");
        }
    }
}
