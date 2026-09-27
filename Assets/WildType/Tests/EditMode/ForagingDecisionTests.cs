using NUnit.Framework;
using UnityEngine;
namespace WildType.Tests
{
    public sealed class ForagingDecisionTests
    {
        [Test]public void AdultTradeoffComparesActualPhenotypesNotAnOverallScore(){
            var p=new Genome();var g=p.Copy();g.legLength=1.5f;var a=new Phenotype(g);var b=new Phenotype(p);
            string text=InheritedChoices.Pair(a,b,"Eddy",0);StringAssert.Contains("Faster",text);StringAssert.Contains("Eddy",text);StringAssert.Contains("wider turns",text);StringAssert.DoesNotContain("better",text);
            Assert.Greater(a.WalkSpeed,b.WalkSpeed);Assert.Less(a.TurnRate,b.TurnRate);
        }
        [Test]public void VisionTradeoffOnlyClaimsMoreUpkeepWhenActuallyHigher(){
            var p=new Genome();var g=p.Copy();g.vision=40;var a=new Phenotype(g);var b=new Phenotype(p);
            StringAssert.Contains("Sees farther",InheritedChoices.Pair(a,b,"Sela",1));StringAssert.Contains("more upkeep",InheritedChoices.Pair(a,b,"Sela",1));
            g.energyEfficiency=1.5f;a=new Phenotype(g);Assert.Less(a.PassiveDrain,b.PassiveDrain);StringAssert.Contains("less upkeep",InheritedChoices.Pair(a,b,"Sela",1));
        }
        [Test]public void LargerReserveDoesNotClaimLongerSurvival(){var g=new Genome{bodySize=1.6f};string text=InheritedChoices.Pair(new Phenotype(g),new Phenotype(new Genome()),"Sela",2);StringAssert.Contains("Larger reserve",text);StringAssert.Contains("more upkeep",text);StringAssert.DoesNotContain("lasts longer",text);}
        [Test]public void SimilarThresholdDoesNotRoundSimulationOrChangeGenome(){var g=new Genome{legLength=1.0012345f};string before=JsonUtility.ToJson(g);Assert.False(InheritedChoices.Different(100.001f,100));Assert.True(InheritedChoices.Different(105,100));InheritedChoices.Pair(new Phenotype(g),new Phenotype(new Genome()),"Eddy",0);Assert.AreEqual(before,JsonUtility.ToJson(g));StringAssert.Contains("1.001",JournalReadout.Exact(g.legLength));}
        [Test]public void EveryGeneStillHasPracticalHelpIncludingScouting(){for(int i=0;i<GenomeGeneCatalog.Count;i++){var gene=GenomeGeneCatalog.At(i);StringAssert.Contains(JournalReadout.Exact(GenomeGeneCatalog.Get(new Genome(),gene)),GeneGuide.Help(new Genome(),gene));}StringAssert.Contains("scouting",GeneGuide.Explain(GenomeGene.Vision));}
        [Test]public void CompactCalloutPrefersARealOpposingCostRatherThanAnOverallRanking(){
            var parent=new Phenotype(new Genome());var large=new Phenotype(new Genome{bodySize=1.5f});
            StringAssert.Contains("Larger reserve",InheritedChoices.Opposing(large,parent,"Eddy"));StringAssert.Contains("more upkeep",InheritedChoices.Opposing(large,parent,"Eddy"));
            var sight=new Phenotype(new Genome{vision=40});StringAssert.Contains("Sees farther",InheritedChoices.Opposing(sight,parent,"Eddy"));
            Assert.AreEqual("",InheritedChoices.Opposing(parent,parent,"Eddy"));
        }
    }
}
