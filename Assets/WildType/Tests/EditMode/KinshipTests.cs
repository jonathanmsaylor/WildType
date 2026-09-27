using NUnit.Framework;
namespace WildType.Tests
{
    public sealed class KinshipTests
    {
        LineageArchive archive;
        CreatureId Id(string name)=>CreatureId.From(name);
        void Founder(string name)=>Assert.True(archive.Add(CreatureLineageRecord.Founder(Id(name),0)));
        void Child(string name,string a,string b)=>Assert.True(archive.Add(CreatureLineageRecord.Child(Id(name),archive.Get(Id(a)),archive.Get(Id(b)),1,null)));
        [SetUp] public void Setup()
        {
            archive=new LineageArchive();foreach(var n in new[]{"a","b","c","d","e","f","g","h","i","j"})Founder(n);
            Child("ab1","a","b");Child("ab2","b","a");Child("ac","c","a");
            Child("x","d","ab1");Child("y","ab2","e");Child("z","ac","f");
            Child("xx","g","x");Child("yy","y","h");Child("xxx","i","xx");
        }
        [TestCase("a","ab1")][TestCase("a","xxx")]
        [TestCase("ab1","ab2")][TestCase("ab1","ac")]
        [TestCase("ab2","x")][TestCase("ac","x")]
        [TestCase("x","y")][TestCase("x","z")]
        public void ProhibitedRelationshipsAreSymmetric(string a,string b)
        {Assert.True(archive.TooCloselyRelated(Id(a),Id(b)));Assert.True(archive.TooCloselyRelated(Id(b),Id(a)));}
        [TestCase("a","b")][TestCase("a","d")][TestCase("xx","yy")]
        [TestCase("xx","y")][TestCase("ab2","xx")][TestCase("x","h")]
        public void DistantAndUnrelatedPairsRemainPossible(string a,string b)
        {Assert.False(archive.TooCloselyRelated(Id(a),Id(b)));Assert.False(archive.TooCloselyRelated(Id(b),Id(a)));}
        [Test] public void DeadAncestorsRemainEvidenceAndUnknownIdsDoNotInventKinship()
        {archive.RecordDeath(Id("a"),CreatureDeathCause.OldAge,500);archive.RecordDeath(Id("ab1"),CreatureDeathCause.Unknown,500);
            Assert.True(archive.TooCloselyRelated(Id("x"),Id("y")));Assert.True(archive.TooCloselyRelated(Id("a"),Id("xxx")));
            Assert.False(archive.TooCloselyRelated(Id("missing"),Id("x")));Assert.False(archive.TooCloselyRelated(default,default));}
        [Test] public void ArchiveBoundAndFreshRunKeepChecksFinite()
        {string parent="xxx";while(archive.Count<LineageArchive.Capacity){string next="deep"+archive.Count;Child(next,parent,"j");parent=next;}
            Assert.True(archive.TooCloselyRelated(Id("a"),Id(parent)));Assert.False(archive.TooCloselyRelated(Id(parent),Id("h")));
            Assert.AreEqual(512,archive.Count);Assert.False(archive.Add(CreatureLineageRecord.Founder(Id("overflow"),0)));
            archive=new LineageArchive();Founder("a");Founder("b");Assert.False(archive.TooCloselyRelated(Id("a"),Id("b")));}
    }
}
