using NUnit.Framework;
using UnityEngine;
namespace WildType.Tests
{
    public sealed class GenerationRulesTests
    {
        [Test] public void WoundedAdultsAndInvalidAgeCannotReproduce()
        {
            var c = new ReproductionCandidate { creatureId = CreatureId.From("a"), alive = true, age = 30, maturityAge = 20,
                health = 59, energy = 100, maximumEnergy = 100, reproductionEnergyFraction = .7f };
            Assert.AreEqual(ReproductionRejection.LowHealth, ReproductionEligibility.CandidateReason(c));
            c.health = 60; Assert.AreEqual(ReproductionRejection.None, ReproductionEligibility.CandidateReason(c));
            c.age = -1; Assert.AreEqual(ReproductionRejection.InvalidData, ReproductionEligibility.CandidateReason(c));
        }
        [Test] public void ArchiveRetainsDeadParentsAndTransitiveAncestry()
        {
            var archive = new LineageArchive();
            var a = CreatureLineageRecord.Founder(CreatureId.From("a"), 0);
            var b = CreatureLineageRecord.Founder(CreatureId.From("b"), 0);
            Assert.True(archive.Add(a)); Assert.True(archive.Add(b));
            var child = CreatureLineageRecord.Child(CreatureId.From("c"), a, b, 1, null); Assert.True(archive.Add(child));
            var grandchild = CreatureLineageRecord.Child(CreatureId.From("d"), child, b, 2, null); Assert.True(archive.Add(grandchild));
            archive.MarkDead(a.CreatureId); archive.MarkDead(child.CreatureId);
            Assert.True(archive.IsDescendant(grandchild.CreatureId, a.CreatureId));
            Assert.False(archive.IsDescendant(a.CreatureId, grandchild.CreatureId));
            Assert.False(archive.IsDescendant(a.CreatureId, a.CreatureId));
            Assert.False(archive.Get(a.CreatureId).Alive);
            Assert.AreEqual(1, archive.Get(a.CreatureId).OffspringCount);
            Assert.False(archive.Add(child)); Assert.AreEqual(4, archive.Count);
        }
        [Test] public void ArchiveRejectsOrphansAndHasAHardLimit()
        {
            var archive = new LineageArchive();
            var a = CreatureLineageRecord.Founder(CreatureId.From("a"), 0);
            var b = CreatureLineageRecord.Founder(CreatureId.From("b"), 0);
            Assert.False(archive.Add(CreatureLineageRecord.Child(CreatureId.From("orphan"), a, b, 1, null)));
            for (int i = 0; i < LineageArchive.Capacity; i++) Assert.True(archive.Add(CreatureLineageRecord.Founder(CreatureId.From(i.ToString()), 0)));
            Assert.False(archive.Add(a)); Assert.AreEqual(LineageArchive.Capacity, archive.Count);
        }
        [Test] public void SiblingGenomesAndMutationRecordsDoNotAlias()
        {
            var a = new Genome(); var b = new Genome { bodySize = 1.4f };
            var first = GenomeInheritance.CreateChild(a, b, new EvolutionSettings(), new SeededRandomSource(3));
            var second = GenomeInheritance.CreateChild(a, b, new EvolutionSettings(), new SeededRandomSource(4));
            string original = JsonUtility.ToJson(second.Genome); first.Genome.bodySize = .65f;
            Assert.AreEqual(original, JsonUtility.ToJson(second.Genome)); Assert.AreEqual(1, a.bodySize);
            var records = new[] { new MutationRecord(GenomeGene.BodySize, 1, 1.1f, false) };
            var parentA = CreatureLineageRecord.Founder(CreatureId.From("a"), 0);
            var parentB = CreatureLineageRecord.Founder(CreatureId.From("b"), 0);
            var child = CreatureLineageRecord.Child(CreatureId.From("c"), parentA, parentB, 1, records);
            records[0] = default; var copy = child.ImportantMutations; copy[0] = default;
            Assert.AreEqual(.1f, child.ImportantMutations[0].Difference, .0001f);
        }
        [Test] public void ManyMutatedGenerationsStayWithinEveryGeneBound()
        {
            var a = new Genome(); var b = new Genome(); var random = new SeededRandomSource(9281);
            int positive = 0, negative = 0, unmutated = 0;
            for (int n = 0; n < 1000; n++)
            {
                var result = GenomeInheritance.CreateChild(a, b, new EvolutionSettings { importantMutationThreshold = 0 }, random);
                foreach (var mutation in result.ImportantMutations) { if (mutation.Difference > 0) positive++; if (mutation.Difference < 0) negative++; }
                if (result.SmallMutationCount + result.MajorMutationCount == 0) unmutated++;
                for (int i = 0; i < GenomeGeneCatalog.Count; i++)
                { var gene = GenomeGeneCatalog.At(i); Assert.That(GenomeGeneCatalog.Get(result.Genome, gene), Is.InRange(GenomeGeneCatalog.Minimum(gene), GenomeGeneCatalog.Maximum(gene))); }
                a = b; b = result.Genome;
            }
            Assert.Greater(positive, 0); Assert.Greater(negative, 0); Assert.Greater(unmutated, 0);
        }
    }
}
