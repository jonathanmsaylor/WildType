using NUnit.Framework;
namespace WildType.Tests
{
    public sealed class FamilyCareRulesTests
    {
        [TestCase(100, 100, 0, 100, 18, 14.4f)]
        [TestCase(30, 100, 20, 100, 5, 4)]
        [TestCase(100, 100, 98, 100, 2.5f, 2)]
        [TestCase(500, 500, 0, 40, 18, 14.4f)]
        public void TransferConservesUsableEnergyAndReserves(float parent, float max, float child, float childMax, float cost, float gain)
        {
            Assert.True(FamilyCareRules.Transfer(parent, max, child, childMax, out float actualCost, out float actualGain));
            Assert.AreEqual(cost, actualCost, .0001f); Assert.AreEqual(gain, actualGain, .0001f);
            Assert.LessOrEqual(actualGain, actualCost); Assert.GreaterOrEqual(parent - actualCost, max * .25f);
            Assert.LessOrEqual(child + actualGain, childMax);
        }
        [TestCase(25, 100, 0, 100)]
        [TestCase(0, 100, 0, 100)]
        [TestCase(100, 100, 100, 100)]
        [TestCase(float.NaN, 100, 0, 100)]
        [TestCase(100, float.PositiveInfinity, 0, 100)]
        [TestCase(100, 100, -1, 100)]
        public void InvalidEmptyAndFullTransfersSpendNothing(float parent, float max, float child, float childMax)
        {
            Assert.False(FamilyCareRules.Transfer(parent, max, child, childMax, out float cost, out float gain));
            Assert.AreEqual(0, cost); Assert.AreEqual(0, gain);
        }
        [Test] public void DirectFamilyIsNotAllDescendantsOrSharedLineage()
        {
            var a = CreatureLineageRecord.Founder(CreatureId.From("a"), 0);
            var b = CreatureLineageRecord.Founder(CreatureId.From("b"), 0);
            var c = CreatureLineageRecord.Child(CreatureId.From("c"), a, b, 1, null);
            var d = CreatureLineageRecord.Child(CreatureId.From("d"), c, b, 2, null);
            Assert.True(FamilyCareRules.DirectChild(a.CreatureId, c)); Assert.True(FamilyCareRules.DirectChild(b.CreatureId, c));
            Assert.False(FamilyCareRules.DirectChild(a.CreatureId, d)); Assert.False(FamilyCareRules.DirectChild(c.CreatureId, c));
            Assert.False(FamilyCareRules.DirectChild(a.CreatureId, c.WithAlive(false)));
            Assert.False(FamilyCareRules.DirectChild(default, c)); Assert.False(FamilyCareRules.DirectChild(a.CreatureId, null));
        }
    }
}
