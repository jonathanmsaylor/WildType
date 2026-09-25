using System.Collections.Generic;
using NUnit.Framework;
namespace WildType.Tests
{
    public sealed class ChronicleTests
    {
        static CreatureId Id(int n) => CreatureId.From("seed:" + n.ToString("D3"));
        static LineageArchive Family()
        {
            var a = new LineageArchive();
            a.Add(CreatureLineageRecord.Founder(Id(1), 0)); a.Add(CreatureLineageRecord.Founder(Id(2), 0));
            a.Add(CreatureLineageRecord.Child(Id(3), a.Get(Id(1)), a.Get(Id(2)), 10, null));
            a.Add(CreatureLineageRecord.Child(Id(4), a.Get(Id(3)), a.Get(Id(2)), 30, null));
            a.Add(CreatureLineageRecord.Child(Id(5), a.Get(Id(1)), a.Get(Id(2)), 40, null)); return a;
        }
        [Test] public void FamilyIncludesDeadAncestorsAndDescendantsButNotSiblingBranches()
        {
            var a = Family(); a.RecordDeath(Id(3), CreatureDeathCause.OldAge, 60);
            var rows = new List<CreatureLineageRecord>(); a.CollectFamily(Id(3), rows);
            CollectionAssert.AreEqual(new[] { Id(1), Id(2), Id(3), Id(4) }, rows.ConvertAll(r => r.CreatureId));
            Assert.AreEqual("Parent", LineageChronicle.Relationship(a, Id(4), a.Get(Id(3))));
            Assert.AreEqual("Ancestor", LineageChronicle.Relationship(a, Id(4), a.Get(Id(1))));
            Assert.AreEqual("Descendant", LineageChronicle.Relationship(a, Id(1), a.Get(Id(4))));
            a.CollectFamily(default, rows); Assert.IsEmpty(rows);
        }
        [Test] public void ConfirmedDeathSurvivesRecentEventEvictionAndCannotBeRewritten()
        {
            var a = Family(); var history = new TurnoverHistory();
            Assert.True(a.RecordDeath(Id(3), CreatureDeathCause.Starvation, 55));
            history.RecordDeath(a.Get(Id(3)), CreatureDeathCause.Starvation);
            for(int i=0;i<8;i++) history.RecordBirth(a.Get(Id(4)));
            Assert.AreEqual(TurnoverKind.Birth, history.Recent[0].Kind);
            Assert.False(a.RecordDeath(Id(3), CreatureDeathCause.OldAge, 100));
            Assert.True(a.TryDeath(Id(3), out var death)); Assert.AreEqual(55, death.Time);
            Assert.AreEqual(CreatureDeathCause.Starvation, death.Cause); Assert.AreEqual(45, a.Get(Id(3)).AgeAt(death.Time));
        }
        [Test] public void RemovalIsNotEvidenceOfDeathAndUnknownIsNotGuessed()
        {
            var a = Family(); a.MarkDead(Id(3)); Assert.False(a.TryDeath(Id(3), out _));
            Assert.True(a.RecordDeath(Id(4), (CreatureDeathCause)999, 50));
            Assert.True(a.TryDeath(Id(4), out var death)); Assert.AreEqual("cause unknown", LineageChronicle.Cause(death.Cause));
            Assert.False(a.RecordDeath(Id(999), CreatureDeathCause.OldAge, 60));
        }
        [TestCase(double.NaN)] [TestCase(double.PositiveInfinity)] [TestCase(-1d)]
        public void InvalidDeathTimesAreRejected(double time)
        { var a=Family(); Assert.False(a.RecordDeath(Id(3),CreatureDeathCause.OldAge,time)); Assert.True(a.Get(Id(3)).Alive); }
        [Test] public void FullArchiveAndChronicleRemainBoundedAndRetainParentLinks()
        {
            var a=Family();
            for(int n=6;n<=LineageArchive.Capacity;n++) Assert.True(a.Add(CreatureLineageRecord.Child(Id(n),a.Get(Id(n-1)),a.Get(Id(2)),n*10,null)));
            Assert.False(a.Add(CreatureLineageRecord.Child(Id(513),a.Get(Id(512)),a.Get(Id(2)),6000,null)));
            var rows=new List<CreatureLineageRecord>(); a.CollectFamily(Id(1),rows); Assert.AreEqual(511,rows.Count);
            for(int n=1;n<=512;n++) Assert.True(a.RecordDeath(Id(n),CreatureDeathCause.Unknown,6000));
            a.CollectFamily(Id(512),rows); Assert.LessOrEqual(rows.Count,512); Assert.True(a.TryDeath(Id(1),out _));
            Assert.AreEqual(Id(511),a.Get(Id(512)).FirstParentId); Assert.AreEqual(512,a.Count);
        }
        [Test] public void ChronicleRefreshesOffspringCountsWithoutChangingIdentity()
        {
            var a=Family(); var rows=new List<CreatureLineageRecord>(); a.CollectFamily(Id(1),rows);
            Assert.AreEqual(2,rows[0].OffspringCount); int revision=a.Revision;
            a.Add(CreatureLineageRecord.Child(Id(6),a.Get(Id(3)),a.Get(Id(4)),70,null));
            a.CollectFamily(Id(3),rows); Assert.Greater(a.Revision,revision);
            Assert.AreEqual(2,rows.Find(r=>r.CreatureId==Id(3)).OffspringCount);
        }
    }
}
