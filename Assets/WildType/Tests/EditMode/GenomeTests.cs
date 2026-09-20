using NUnit.Framework;
using UnityEngine;
using System.Reflection;
namespace WildType.Tests
{
    public sealed class GenomeTests
    {
        [TestCase(float.NaN)] [TestCase(float.PositiveInfinity)] [TestCase(float.NegativeInfinity)] [TestCase(-99999f)] [TestCase(99999f)]
        public void InvalidGenesAlwaysProduceFinitePositiveStats(float invalid)
        {
            var genome = new Genome();
            foreach (var field in typeof(Genome).GetFields()) if (field.FieldType == typeof(float)) field.SetValue(genome, invalid);
            genome.camouflage = new Color(invalid, invalid, invalid, invalid); genome.Validate();
            foreach (var field in typeof(Genome).GetFields())
                if (field.FieldType == typeof(float)) Assert.That((float)field.GetValue(genome), Is.GreaterThan(0).And.LessThan(1000), field.Name);
            var stats = new Phenotype(genome);
            foreach (var field in typeof(Phenotype).GetFields(BindingFlags.Public | BindingFlags.Instance))
                Assert.That((float)field.GetValue(stats), Is.GreaterThan(0).And.LessThan(1000), field.Name);
            Assert.That(stats.SprintSpeed, Is.GreaterThan(stats.WalkSpeed));
            Assert.That(genome.camouflage.a, Is.EqualTo(1));
        }
        [Test] public void PresetCopiesAreIndependent()
        {
            var preset = ScriptableObject.CreateInstance<GenomePreset>();
            var a = preset.RuntimeCopy(); var b = preset.RuntimeCopy(); a.bodySize = 1.6f;
            Assert.That(b.bodySize, Is.EqualTo(1)); Assert.That(preset.genome.bodySize, Is.EqualTo(1)); Object.DestroyImmediate(preset);
        }
        [Test] public void CalculationDoesNotModifyGenome()
        {
            var genome = new Genome { bodySize = -100 }; _ = new Phenotype(genome); Assert.That(genome.bodySize, Is.EqualTo(-100));
        }
        [Test] public void SizeTradesEnergyCapacityForConsumption()
        {
            var a = new Phenotype(new Genome()); var b = new Phenotype(new Genome { bodySize = 1.7f });
            Assert.That(b.MaxEnergy, Is.GreaterThan(a.MaxEnergy)); Assert.That(b.PassiveDrain, Is.GreaterThan(a.PassiveDrain)); Assert.That(b.Height, Is.GreaterThan(a.Height));
        }
        [Test] public void SpeedTradesPerformanceForConsumption()
        {
            var a = new Phenotype(new Genome()); var b = new Phenotype(new Genome { movementSpeed = 9 });
            Assert.That(b.SprintSpeed, Is.GreaterThan(a.SprintSpeed)); Assert.That(b.MoveCost, Is.GreaterThan(a.MoveCost)); Assert.That(b.StaminaCost, Is.GreaterThan(a.StaminaCost));
        }
        [Test] public void LegsTradeSpeedForAgility()
        {
            var a = new Phenotype(new Genome()); var b = new Phenotype(new Genome { legLength = 1.6f });
            Assert.That(b.WalkSpeed, Is.GreaterThan(a.WalkSpeed)); Assert.That(b.TurnRate, Is.LessThan(a.TurnRate));
        }
        [Test] public void VisionHasMetabolicCost()
        {
            var a = new Phenotype(new Genome()); var b = new Phenotype(new Genome { vision = 45 });
            Assert.That(b.Vision, Is.GreaterThan(a.Vision)); Assert.That(b.PassiveDrain, Is.GreaterThan(a.PassiveDrain));
        }
        [Test] public void MetabolismTradesProcessingForPassiveCost()
        {
            var a = new Phenotype(new Genome()); var b = new Phenotype(new Genome { metabolism = 1.8f });
            Assert.That(b.StaminaRecovery, Is.GreaterThan(a.StaminaRecovery)); Assert.That(b.NutritionFactor, Is.GreaterThan(a.NutritionFactor)); Assert.That(b.PassiveDrain, Is.GreaterThan(a.PassiveDrain));
        }
        [Test] public void EfficiencyLimitsBurstPerformance()
        {
            var a = new Phenotype(new Genome()); var b = new Phenotype(new Genome { energyEfficiency = 1.5f });
            Assert.That(b.SprintSpeed, Is.LessThan(a.SprintSpeed)); Assert.That(b.MoveCost, Is.LessThan(a.MoveCost));
        }
        [Test] public void SeededVariationIsDeterministicAndBounded()
        {
            var a = new System.Random(123); var b = new System.Random(123);
            for (int i = 0; i < 500; i++)
            {
                var first = Genome.Varied(new Genome(), a); var second = Genome.Varied(new Genome(), b);
                Assert.That(JsonUtility.ToJson(first), Is.EqualTo(JsonUtility.ToJson(second)));
                Assert.That(first.bodySize, Is.InRange(.65f, 1.7f));
            }
        }
        [TestCase(30)] [TestCase(60)] [TestCase(120)]
        public void ResourcesAreFrameRateIndependent(int rate)
        {
            var obj = new GameObject(); var vitals = obj.AddComponent<CreatureVitals>(); var stats = new Phenotype(new Genome());
            vitals.Configure(stats);
            for (int i = 0; i < rate * 2; i++) vitals.Tick(1f / rate, stats.SprintSpeed, true);
            Assert.That(vitals.Energy, Is.EqualTo(stats.MaxEnergy - 2 * (stats.PassiveDrain + stats.MoveCost + stats.SprintCost)).Within(.002f));
            Assert.That(vitals.Stamina, Is.EqualTo(stats.MaxStamina - stats.StaminaCost * 2).Within(.002f));
            Object.DestroyImmediate(obj);
        }
        [Test] public void StaminaLockPreventsThresholdStutter()
        {
            var obj = new GameObject(); var v = obj.AddComponent<CreatureVitals>(); var stats = new Phenotype(new Genome()); v.Configure(stats);
            v.Tick(6, stats.SprintSpeed, true); Assert.That(v.CanSprint, Is.False);
            v.Tick(.2f, 0, false); Assert.That(v.Stamina, Is.GreaterThan(0)); Assert.That(v.CanSprint, Is.False);
            v.Tick(2, 0, false); Assert.That(v.CanSprint, Is.True); Object.DestroyImmediate(obj);
        }
        [Test] public void StarvationDeathFiresExactlyOnce()
        {
            var obj = new GameObject(); var v = obj.AddComponent<CreatureVitals>(); var stats = new Phenotype(new Genome()); v.Configure(stats);
            int deaths = 0; v.Died += () => deaths++;
            for (int i = 0; i < 60000; i++) v.Tick(.02f, 0, false);
            Assert.That(v.Dead, Is.True); Assert.That(deaths, Is.EqualTo(1)); Assert.That(v.Eat(50), Is.Zero); Object.DestroyImmediate(obj);
        }
        [Test] public void InvalidResourceInputsDoNotCorruptState()
        {
            var obj = new GameObject(); var v = obj.AddComponent<CreatureVitals>(); var s = new Phenotype(new Genome()); v.Configure(s);
            v.Tick(float.NaN, 0, false); v.Tick(float.PositiveInfinity, 0, false); v.Eat(float.NaN); v.Damage(float.PositiveInfinity);
            Assert.That(v.Energy, Is.EqualTo(s.MaxEnergy)); Assert.That(v.Health, Is.EqualTo(100)); Object.DestroyImmediate(obj);
        }
    }
}
