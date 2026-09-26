using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
namespace WildType.Tests
{
    public sealed class WelcomingTests
    {
        [TestCase(0,"0")][TestCase(.001f,"less than 1")][TestCase(.99f,"less than 1")]
        [TestCase(1,"1")][TestCase(1.5f,"2")][TestCase(14.4f,"14")][TestCase(18.6f,"19")]
        public void OrdinaryNumbersRoundWithoutInventingZero(float value,string display)=>Assert.AreEqual(display,JournalReadout.Whole(value));
        [Test] public void RoundedCareIsExplicitlyApproximateAndExactRemainsAvailable()
        {
            Assert.AreEqual("You spend 18 energy · child gains about 14",JournalReadout.Care(18,14.4f));
            Assert.AreEqual(14.4f,float.Parse(JournalReadout.Exact(14.4f),System.Globalization.CultureInfo.InvariantCulture));
        }
        [Test] public void CategoriesCoverEveryActualGeneAndExplanationsKeepTradeoffs()
        {
            var genome=new Genome();string before=JsonUtility.ToJson(genome);var counts=new int[4];
            for(int i=0;i<GenomeGeneCatalog.Count;i++){
                var gene=GenomeGeneCatalog.At(i);counts[GeneGuide.Category(gene)]++;
                StringAssert.Contains("Higher",GeneGuide.Explain(gene));StringAssert.Contains("lower",GeneGuide.Explain(gene).ToLowerInvariant());
                StringAssert.Contains(JournalReadout.Exact(GenomeGeneCatalog.Get(genome,gene)),GeneGuide.Help(genome,gene));
            }
            CollectionAssert.AreEqual(new[]{4,3,3,6},counts);Assert.AreEqual(before,JsonUtility.ToJson(genome));
        }
        [Test] public void RequiredAmountNeverUnderstatesEligibilityThreshold()
        {Assert.AreEqual("73",JournalReadout.Required(72.1f));Assert.AreEqual("72",JournalReadout.Required(72));Assert.AreEqual("Unavailable",JournalReadout.Whole(float.NaN));}
        [Test] public void RegionNamesAreProperNames()
        {Assert.AreEqual("The Meadow",EcologyRules.Name(Habitat.Meadow));Assert.AreEqual("Fernwood",EcologyRules.Name(Habitat.Woodland));Assert.AreEqual("Amber Flats",EcologyRules.Name(Habitat.Dry));}
        [Test] public void TreeLinksBothRecordedParentsAndKeepsCurrentDeathState()
        {
            var archive=new LineageArchive();var a=CreatureLineageRecord.Founder(CreatureId.From("a"),0);var b=CreatureLineageRecord.Founder(CreatureId.From("b"),0);
            archive.Add(a);archive.Add(b);var child=CreatureLineageRecord.Child(CreatureId.From("c"),a,b,1,null);archive.Add(child);archive.RecordDeath(child.CreatureId,CreatureDeathCause.OldAge,600);
            var list=new List<CreatureLineageRecord>();archive.CollectChildren(a.CreatureId,list);Assert.AreEqual(1,list.Count);Assert.False(list[0].Alive);
            archive.CollectChildren(b.CreatureId,list);Assert.AreEqual(child.CreatureId,list[0].CreatureId);archive.CollectChildren(default,list);Assert.IsEmpty(list);
        }
    }
}
