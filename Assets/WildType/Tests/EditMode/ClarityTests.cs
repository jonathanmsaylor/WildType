using NUnit.Framework;
namespace WildType.Tests
{
    public sealed class ClarityTests
    {
        [TestCase(0,100,100,false,"STARVING")]
        [TestCase(25,100,100,false,"Hungry")]
        [TestCase(80,100,50,false,"does not heal")]
        [TestCase(80,100,100,true,"recover stamina")]
        [TestCase(0,100,0,true,"Life ended")]
        public void StatusIsTruthful(float energy,float max,float health,bool locked,string expected)
        {StringAssert.Contains(expected,GameplayText.Status(energy,max,health,locked));}
        [Test] public void HealthyCreatureHasNoUrgentWarning(){Assert.AreEqual("",GameplayText.Status(90,100,100,false));}
        [Test] public void DetailTextIncludesEveryGeneWithoutChangingIt()
        {
            var g=new Genome();string before=UnityEngine.JsonUtility.ToJson(g);string text=GameplayText.Genes(g);
            for(int i=0;i<GenomeGeneCatalog.Count;i++)StringAssert.Contains(GenomeGeneCatalog.Name(GenomeGeneCatalog.At(i)),text);
            Assert.AreEqual(before,UnityEngine.JsonUtility.ToJson(g));StringAssert.Contains("All other genes equal",GameplayText.Tradeoffs);
            StringAssert.Contains("No body type wins everywhere",GameplayText.Tradeoffs);
        }
        [TestCase(false,"E")][TestCase(true,"South button")]
        public void GuideExplainsPauseCostsAndDistinctActions(bool gamepad,string eat)
        {var text=GameplayText.Guide(gamepad);StringAssert.Contains("Simulation paused",text);StringAssert.Contains(eat,text);StringAssert.Contains("not health",text);StringAssert.Contains("Take control switches",text);StringAssert.Contains("does not heal or guarantee",text);}
        [Test] public void MutationHistoryDoesNotCallInheritanceAnImprovement()
        {var records=new[]{new MutationRecord(GenomeGene.BodySize,1,1.1f,false),new MutationRecord(GenomeGene.LegLength,1,.8f,true)};var text=GameplayText.Mutations(records);StringAssert.Contains("1.100",text);StringAssert.Contains("0.800",text);StringAssert.Contains("not improvements",text);}
    }
}
