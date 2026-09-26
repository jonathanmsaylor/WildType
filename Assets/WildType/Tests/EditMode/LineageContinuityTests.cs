using System.Collections.Generic;
using NUnit.Framework;
namespace WildType.Tests
{
    public sealed class LineageContinuityTests
    {
        static CreatureId Id(int n)=>CreatureId.From("917430:"+n.ToString("D3"));
        [Test] public void EarlierPlayersChildrenRemainHistoryButNeverBecomeDescendants()
        {
            var a=new LineageArchive();a.Add(CreatureLineageRecord.Founder(Id(1),0));a.Add(CreatureLineageRecord.Founder(Id(2),0));
            a.Add(CreatureLineageRecord.Founder(Id(3),0));
            foreach(int n in new[]{17,18})a.Add(CreatureLineageRecord.Child(Id(n),a.Get(Id(1)),a.Get(Id(2)),20,null));
            a.Add(CreatureLineageRecord.Child(Id(19),a.Get(Id(17)),a.Get(Id(3)),60,null));
            a.Add(CreatureLineageRecord.Child(Id(20),a.Get(Id(18)),a.Get(Id(3)),65,null));
            var rows=new List<CreatureLineageRecord>();var history=new[]{Id(1),Id(17)};
            a.CollectFamily(Id(17),rows,history);
            CollectionAssert.AreEqual(new[]{Id(1),Id(2),Id(17),Id(18),Id(19),Id(20)},rows.ConvertAll(r=>r.CreatureId));
            Assert.AreEqual("Sibling",LineageChronicle.Relationship(a,Id(17),a.Get(Id(18))));
            Assert.AreEqual("Child",LineageChronicle.Relationship(a,Id(17),a.Get(Id(19))));
            Assert.AreEqual("Other family branch",LineageChronicle.Relationship(a,Id(17),a.Get(Id(20))));
            Assert.False(a.IsDescendant(Id(18),Id(17)));Assert.False(a.IsDescendant(Id(20),Id(17)));
            foreach(var r in rows)Assert.True(a.InFamilyHistory(r.CreatureId,Id(17),history));
            Assert.False(a.InFamilyHistory(Id(3),Id(17),history));Assert.False(a.InFamilyHistory(Id(999),Id(17),history));
            a.RecordDeath(Id(18),CreatureDeathCause.Starvation,99);a.CollectFamily(Id(17),rows,history);
            Assert.True(rows.Exists(r=>r.CreatureId==Id(18)&&!r.Alive));Assert.True(a.TryDeath(Id(18),out var death));Assert.AreEqual(79,a.Get(Id(18)).AgeAt(death.Time));
        }
        [Test] public void SharedInvalidFounderParentsNeverMakeUnrelatedFoundersSiblings()
        {
            var a=new LineageArchive();a.Add(CreatureLineageRecord.Founder(Id(1),0));a.Add(CreatureLineageRecord.Founder(Id(2),0));
            Assert.AreEqual("Other family branch",LineageChronicle.Relationship(a,Id(1),a.Get(Id(2))));
            Assert.False(a.InFamilyHistory(Id(2),Id(1),new[]{Id(1)}));
        }
        [Test] public void HalfSiblingLabelRequiresARealSharedParent()
        {
            var a=new LineageArchive();for(int n=1;n<=3;n++)a.Add(CreatureLineageRecord.Founder(Id(n),0));
            a.Add(CreatureLineageRecord.Child(Id(4),a.Get(Id(1)),a.Get(Id(2)),10,null));
            a.Add(CreatureLineageRecord.Child(Id(5),a.Get(Id(3)),a.Get(Id(2)),10,null));
            Assert.AreEqual("Sibling",LineageChronicle.Relationship(a,Id(4),a.Get(Id(5))));
        }
        [Test] public void HistoryUnionIsDeduplicatedAndBoundedAtFullArchive()
        {
            var a=new LineageArchive();a.Add(CreatureLineageRecord.Founder(Id(1),0));a.Add(CreatureLineageRecord.Founder(Id(2),0));
            var history=new List<CreatureId>{Id(1)};
            for(int n=3;n<=512;n++){a.Add(CreatureLineageRecord.Child(Id(n),a.Get(Id(n-1)),a.Get(Id(1)),n,null));history.Add(Id(n));}
            var rows=new List<CreatureLineageRecord>();a.CollectFamily(Id(512),rows,history);
            Assert.AreEqual(512,rows.Count);Assert.AreEqual(512,new HashSet<CreatureId>(rows.ConvertAll(r=>r.CreatureId)).Count);
            a.CollectFamily(default,rows,history);Assert.IsEmpty(rows);
        }
    }
}
