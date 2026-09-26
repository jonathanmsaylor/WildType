using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace WildType.Tests
{
    public sealed class ScrollableTreeTests
    {
        CreatureId Id(int i)=>CreatureId.From("tree-"+i.ToString("D4"));
        [Test] public void FullArchiveHasOneNodePerIdentityAndExactlyBothParentLinks()
        {
            var a=new LineageArchive();a.Add(CreatureLineageRecord.Founder(Id(0),0));a.Add(CreatureLineageRecord.Founder(Id(1),0));
            for(int i=2;i<512;i++)Assert.True(a.Add(CreatureLineageRecord.Child(Id(i),a.Get(Id(i<8?0:2+(i-8)/4)),a.Get(Id(1)),i,null)));
            a.RecordDeath(Id(2),CreatureDeathCause.OldAge,600);
            var family=new List<CreatureLineageRecord>();a.CollectFamily(Id(10),family,new[]{Id(0)});var layout=new FamilyTreeLayout();layout.Build(a,family);
            Assert.AreEqual(512,layout.Records.Count);Assert.AreEqual(512,layout.Positions.Count);Assert.AreEqual(1020,layout.Links.Count);
            Assert.AreEqual(512,layout.Records.Select(r=>r.CreatureId).Distinct().Count());
            foreach(var r in layout.Records){Assert.True(float.IsFinite(layout.Positions[r.CreatureId].x));if(r.Generation==0)continue;
                Assert.Contains((r.FirstParentId,r.CreatureId),layout.Links);Assert.Contains((r.SecondParentId,r.CreatureId),layout.Links);
                Assert.Less(layout.Positions[r.FirstParentId].y,layout.Positions[r.CreatureId].y);}
            Assert.True(a.TryDeath(Id(2),out _));Assert.False(a.TryDeath(Id(3),out _));Assert.AreEqual(512,a.Count);
        }
        [Test] public void ParentClosureIncludesCoParentWithoutInventingExtraBranches()
        {
            var a=new LineageArchive();for(int i=0;i<4;i++)a.Add(CreatureLineageRecord.Founder(Id(i),0));
            a.Add(CreatureLineageRecord.Child(Id(4),a.Get(Id(0)),a.Get(Id(1)),0,null));
            a.Add(CreatureLineageRecord.Child(Id(5),a.Get(Id(2)),a.Get(Id(3)),0,null));
            a.Add(CreatureLineageRecord.Child(Id(6),a.Get(Id(4)),a.Get(Id(5)),0,null));
            var family=new List<CreatureLineageRecord>();a.CollectFamily(Id(0),family);var l=new FamilyTreeLayout();l.Build(a,family);
            Assert.AreEqual(7,l.Records.Count);Assert.AreEqual(6,l.Links.Count);Assert.AreEqual(Id(1),l.Move(Id(0),1,0));Assert.AreEqual(Id(4),l.Move(Id(0),0,1));
            var before=l.Positions.ToArray();l.Build(a,family);CollectionAssert.AreEqual(before,l.Positions.ToArray());
        }
        [Test] public void DenseGenerationDoesNotShrinkOrOverlapCards()
        {
            var a=new LineageArchive();a.Add(CreatureLineageRecord.Founder(Id(0),0));a.Add(CreatureLineageRecord.Founder(Id(1),0));
            for(int i=2;i<512;i++)a.Add(CreatureLineageRecord.Child(Id(i),a.Get(Id(0)),a.Get(Id(1)),i,null));
            var family=new List<CreatureLineageRecord>();a.CollectFamily(Id(0),family);var l=new FamilyTreeLayout();l.Build(a,family);
            for(int i=3;i<512;i++)Assert.False(l.Bounds(Id(i)).Overlaps(l.Bounds(Id(i-1))));
            Assert.Greater(l.Size.x,100000);Assert.AreEqual(326,FamilyTreeLayout.CardWidth);Assert.AreEqual(Id(511),l.Move(Id(510),1,0));
        }
    }
}
