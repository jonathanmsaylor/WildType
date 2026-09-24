using NUnit.Framework;
using UnityEngine;
namespace WildType.Tests
{
    public sealed class VisibleInheritanceTests
    {
        [Test] public void AppearanceIsDeterministicAndDoesNotConsumeRandomOrModifyGenome()
        {
            var genome = new Genome { bodySize = 1.37f, legLength = 1.28f, camouflage = new Color(.7f, .3f, .4f) };
            string before = JsonUtility.ToJson(genome); var random = Random.state;
            var a = new CreatureAppearance(genome); var b = new CreatureAppearance(genome.Copy());
            Assert.AreEqual(a, b); Assert.AreEqual(before, JsonUtility.ToJson(genome)); Assert.AreEqual(random, Random.state);
            Assert.AreEqual(genome.camouflage, a.Coat);
        }
        [Test] public void InvalidAppearanceInputsAreSanitizedWithoutChangingSource()
        {
            var genome = new Genome { bodySize = float.NaN, legLength = float.PositiveInfinity, camouflage = new Color(-10, float.NaN, 90) };
            var shape = new CreatureAppearance(genome);
            Assert.True(CreatureMotor.Finite(shape.BodyScale)); Assert.AreEqual(1, shape.Size); Assert.AreEqual(1, shape.LegLength);
            Assert.AreEqual(.08f, shape.Coat.r); Assert.AreEqual(.9f, shape.Coat.b); Assert.True(float.IsNaN(genome.bodySize));
            Assert.True(CreatureMotor.Finite(new CreatureAppearance(null).BodyScale));
        }
        [Test] public void OrdinaryOffspringCombineBothParentsAppearanceWithoutMutation()
        {
            var a = new Genome { bodySize = .7f, legLength = .65f, camouflage = new Color(.1f, .2f, .3f) };
            var b = new Genome { bodySize = 1.65f, legLength = 1.55f, camouflage = new Color(.8f, .7f, .6f) };
            var settings = new EvolutionSettings { smallMutationChance = 0, majorMutationChance = 0 };
            var sa = new CreatureAppearance(a); var sb = new CreatureAppearance(b);
            for (int seed = 0; seed < 100; seed++)
            {
                var result = GenomeInheritance.CreateChild(a, b, settings, new SeededRandomSource(seed));
                var c = new CreatureAppearance(result.Genome);
                Assert.AreEqual(0, result.SmallMutationCount + result.MajorMutationCount);
                Assert.That(c.BodyWidth, Is.GreaterThan(sa.BodyWidth).And.LessThan(sb.BodyWidth));
                Assert.That(c.LegLength, Is.GreaterThan(sa.LegLength).And.LessThan(sb.LegLength));
                Assert.That(c.BandWidth, Is.GreaterThan(sb.BandWidth).And.LessThan(sa.BandWidth));
                Assert.That(c.Coat.r, Is.GreaterThan(sa.Coat.r).And.LessThan(sb.Coat.r));
                Assert.That(c.Coat.g, Is.GreaterThan(sa.Coat.g).And.LessThan(sb.Coat.g));
                Assert.That(c.Coat.b, Is.GreaterThan(sa.Coat.b).And.LessThan(sb.Coat.b));
            }
        }
        [Test] public void ProbabilisticMutationsRemainBoundedAndCanMoveAppearanceEitherWay()
        {
            var parent = new Genome(); bool shorter = false, longer = false, ordinary = false;
            for (int seed = 0; seed < 500; seed++)
            {
                var result = GenomeInheritance.CreateChild(parent, parent, new EvolutionSettings(), new SeededRandomSource(seed));
                var g = result.Genome; var shape = new CreatureAppearance(g);
                shorter |= g.legLength < parent.legLength; longer |= g.legLength > parent.legLength;
                ordinary |= g.legLength == parent.legLength;
                Assert.That(shape.BandWidth, Is.InRange(.10f, .24f)); Assert.True(CreatureMotor.Finite(shape.BodyScale));
                for (int n = 0; n < GenomeGeneCatalog.Count; n++)
                { var gene = GenomeGeneCatalog.At(n); Assert.That(GenomeGeneCatalog.Get(g, gene), Is.InRange(GenomeGeneCatalog.Minimum(gene), GenomeGeneCatalog.Maximum(gene))); }
            }
            Assert.True(shorter && longer && ordinary, "No scripted improvement or compulsory mutation");
        }
        [Test] public void LongVisibleLegsTradeStrideSpeedForActualSteering()
        {
            var a = new Genome { legLength = .6f }; var b = new Genome { legLength = 1.6f };
            var low = new Phenotype(a); var high = new Phenotype(b);
            Assert.Greater(new CreatureAppearance(b).LegLength, new CreatureAppearance(a).LegLength);
            Assert.Greater(high.SprintSpeed, low.SprintSpeed); Assert.Less(high.TurnRate, low.TurnRate);
            var slowTurn = CreatureMotor.SteerVelocity(Vector3.forward * 3, Vector3.right, 3, high.Acceleration, high.TurnRate, .1f);
            var fastTurn = CreatureMotor.SteerVelocity(Vector3.forward * 3, Vector3.right, 3, low.Acceleration, low.TurnRate, .1f);
            Assert.Greater(Vector3.Angle(slowTurn, Vector3.right), Vector3.Angle(fastTurn, Vector3.right));
            Assert.AreEqual(3, slowTurn.magnitude, .0001f); Assert.AreEqual(low.MaxStamina, high.MaxStamina);
        }
        [Test] public void BroadBodyTradesReservesForFoodCostAndAcceleration()
        {
            var a = new Genome { bodySize = .65f }; var b = new Genome { bodySize = 1.7f };
            var low = new Phenotype(a); var high = new Phenotype(b);
            var slim = new CreatureAppearance(a); var broad = new CreatureAppearance(b);
            Assert.Greater(broad.BodyWidth / broad.Size, slim.BodyWidth / slim.Size);
            Assert.Greater(high.MaxEnergy, low.MaxEnergy); Assert.Greater(high.PassiveDrain, low.PassiveDrain);
            Assert.Greater(high.MoveCost, low.MoveCost); Assert.Less(high.Acceleration, low.Acceleration);
            Assert.AreEqual(low.SprintSpeed, high.SprintSpeed, "No second body-size speed multiplier");
        }
        [Test] public void SteeringReversesOnGroundAndBrakesWithoutCreep()
        {
            var a = CreatureMotor.SteerVelocity(Vector3.forward * 4, Vector3.back, 4, 13, 3, .1f);
            Assert.AreEqual(0, a.y, .00001f); Assert.AreEqual(3 * Mathf.Rad2Deg * .1f, Vector3.Angle(Vector3.forward, a), .001f);
            var b = CreatureMotor.SteerVelocity(Vector3.forward * 4, Vector3.right, 4, 13, 3, .1f);
            var c = Vector3.forward * 4;
            for (int i = 0; i < 10; i++) c = CreatureMotor.SteerVelocity(c, Vector3.right, 4, 13, 3, .01f);
            Assert.Less(Vector3.Distance(b, c), .0001f);
            for (int i = 0; i < 30; i++) a = CreatureMotor.SteerVelocity(a, Vector3.zero, 4, 13, 3, .02f);
            Assert.AreEqual(Vector3.zero, a);
        }
        [Test] public void FamilySummaryExplainsBothParentsAndDoesNotCallMutationImprovement()
        {
            var a = new Genome { legLength = .8f }; var b = new Genome { legLength = 1.4f }; var c = new Genome { legLength = 1.1f };
            string text = InheritanceSummary.Child(c, a, b);
            StringAssert.Contains("Legs 0.80 / 1.40 -> 1.10", text);
            StringAssert.Contains("+stride speed / -steering", text); StringAssert.Contains("+reserves / +food cost", text);
            string mutation = InheritanceSummary.NotableMutation(new[] { new MutationRecord(GenomeGene.LegLength, 1, .95f, false) });
            StringAssert.Contains("1.00 -> 0.95", mutation); StringAssert.Contains("not a guaranteed benefit", mutation);
            Assert.AreEqual("", InheritanceSummary.NotableMutation(null));
        }
    }
}
