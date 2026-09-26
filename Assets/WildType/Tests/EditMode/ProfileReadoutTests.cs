using NUnit.Framework;
using UnityEngine;
namespace WildType.Tests
{
    public sealed class ProfileReadoutTests
    {
        [TestCase("Alexander the Great","Alexander the Great")]
        [TestCase(" Tammy  2 ","Tammy 2")]
        [TestCase("Mary\tJane","Mary Jane")]
        public void NamesPreserveWordsAndTypedDigits(string input,string expected){Assert.AreEqual(expected,FamilyNames.CleanName(input));Assert.AreEqual(expected,FamilyNames.Format(input,512));}
        [Test] public void MovementComparisonsAreNamedAndHonestAboutSmallDifferences()
        {
            Assert.AreEqual("8% faster than Eddy",CreatureProfileText.Compare(4.32f,4,"Eddy"));
            Assert.AreEqual("same as Sela",CreatureProfileText.Compare(4,4,"Sela"));
            StringAssert.Contains("0.2% slower",CreatureProfileText.Compare(3.992f,4,"Eddy"));
            Assert.AreEqual("Slow",CreatureProfileText.SpeedRating(2.9f));Assert.AreEqual("Moderate",CreatureProfileText.SpeedRating(3.4f));Assert.AreEqual("Fast",CreatureProfileText.SpeedRating(5));
        }
        [Test] public void NoticesOnlySampleRealRegionLimitsAndRetainGeneticPrecision()
        {
            var g=new Genome{turnAgility=2};string before=JsonUtility.ToJson(g);var stats=new Phenotype(g);
            Assert.AreEqual("",BiomeEntryReadout.Changes(stats,Vector3.zero,new Vector3(80,0,0)));
            string brush=BiomeEntryReadout.Changes(stats,Vector3.zero,new Vector3(-43,0,0));StringAssert.Contains("Walk limit",brush);StringAssert.Contains("Sprint limit",brush);
            Assert.AreNotEqual(brush,BiomeEntryReadout.Changes(stats,Vector3.zero,new Vector3(-39,0,0)));
            Assert.AreEqual(before,JsonUtility.ToJson(g));
        }
        [Test] public void NoticeDebouncesFreezesExpiresAndClearsAcrossIdentities()
        {
            var m=new BiomeEntryReadout();var run=new object();var id=CreatureId.From("one");var stats=new Phenotype(new Genome{turnAgility=2});
            m.Tick(run,id,stats,Vector3.zero,false,.1f);
            for(int i=0;i<20;i++)m.Tick(run,id,stats,new Vector3(i%2==0?-35.5f:-34.5f,0,0),false,.1f);Assert.AreEqual("",m.Text);
            m.Tick(run,id,stats,new Vector3(-39,0,0),false,.7f);m.Tick(run,id,stats,new Vector3(-40,0,0),false,.5f);StringAssert.Contains("Entering Fernwood",m.Text);
            float alpha=m.Alpha;string text=m.Text;m.Tick(run,id,stats,new Vector3(80,0,0),true,100);Assert.AreEqual(alpha,m.Alpha);Assert.AreEqual(text,m.Text);
            m.Tick(run,id,stats,new Vector3(-40,0,0),false,8);Assert.AreEqual(0,m.Alpha);Assert.AreEqual("",m.Text);
            m.Tick(run,CreatureId.From("two"),stats,Vector3.zero,false,.1f);Assert.AreEqual("",m.Text);
            m.Tick(new object(),id,stats,Vector3.zero,false,.1f);Assert.AreEqual("",m.Text);
        }
    }
}
