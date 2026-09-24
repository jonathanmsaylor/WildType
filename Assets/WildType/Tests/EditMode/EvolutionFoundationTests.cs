using System;
using NUnit.Framework;
using UnityEngine;

namespace WildType.Tests
{
    public sealed class EvolutionFoundationTests
    {
        sealed class SequenceRandom : IRandomSource
        {
            readonly double[] values;
            int index;
            public SequenceRandom(params double[] values) { this.values = values.Length == 0 ? new[] { .5 } : values; }
            public double Next01() { double value = values[index++ % values.Length]; return Math.Max(0, Math.Min(.999999, value)); }
        }

        static EvolutionSettings NoMutation() => new EvolutionSettings { smallMutationChance = 0, majorMutationChance = 0 };

        static Genome Extreme(float normalized)
        {
            var genome = new Genome();
            for (int i = 0; i < GenomeGeneCatalog.Count; i++)
            {
                GenomeGene gene = GenomeGeneCatalog.At(i);
                GenomeGeneCatalog.Set(genome, gene, Mathf.Lerp(GenomeGeneCatalog.Minimum(gene), GenomeGeneCatalog.Maximum(gene), normalized));
            }
            genome.Validate(); return genome;
        }

        [Test]
        public void NewGenesSanitizeNonfiniteAndClamp()
        {
            var genome = new Genome
            {
                fertility = float.NaN,
                reproductionThreshold = float.PositiveInfinity,
                offspringTendency = -100,
                lifespan = 100000
            };
            genome.Validate();
            Assert.That(genome.fertility, Is.EqualTo(1));
            Assert.That(genome.reproductionThreshold, Is.EqualTo(.72f));
            Assert.That(genome.offspringTendency, Is.EqualTo(.4f));
            Assert.That(genome.lifespan, Is.EqualTo(900));
        }

        [Test]
        public void NewGenesSurviveIndependentRuntimeCopy()
        {
            var original = new Genome { fertility = 1.4f, reproductionThreshold = .55f, offspringTendency = 1.5f, lifespan = 840 };
            var copy = original.Copy(); copy.fertility = .5f;
            Assert.That(original.fertility, Is.EqualTo(1.4f));
            Assert.That(copy.reproductionThreshold, Is.EqualTo(.55f));
            Assert.That(copy.offspringTendency, Is.EqualTo(1.5f));
            Assert.That(copy.lifespan, Is.EqualTo(840));
        }

        [Test]
        public void FertilityTradesCooldownForMaintenance()
        {
            var low = new Phenotype(new Genome { fertility = .5f });
            var high = new Phenotype(new Genome { fertility = 1.5f });
            Assert.That(high.FertilityFactor, Is.GreaterThan(low.FertilityFactor));
            Assert.That(high.ReproductionCooldown, Is.LessThan(low.ReproductionCooldown));
            Assert.That(high.PassiveDrain, Is.GreaterThan(low.PassiveDrain));
        }

        [Test]
        public void ReproductionThresholdChangesRequiredReserve()
        {
            var early = new Phenotype(new Genome { reproductionThreshold = .5f });
            var cautious = new Phenotype(new Genome { reproductionThreshold = .9f });
            Assert.That(early.ReproductionEnergyFraction, Is.LessThan(cautious.ReproductionEnergyFraction));
        }

        [Test]
        public void OffspringTendencyTradesMotivationForMaintenance()
        {
            var low = new Phenotype(new Genome { offspringTendency = .4f });
            var high = new Phenotype(new Genome { offspringTendency = 1.6f });
            Assert.That(high.ReproductiveMotivation, Is.GreaterThan(low.ReproductiveMotivation));
            Assert.That(high.PassiveDrain, Is.GreaterThan(low.PassiveDrain));
        }

        [Test]
        public void LifespanTradesLongevityForMaturityAndMaintenance()
        {
            var shortLife = new Phenotype(new Genome { lifespan = 240 });
            var longLife = new Phenotype(new Genome { lifespan = 900 });
            Assert.That(longLife.LifespanSeconds, Is.GreaterThan(shortLife.LifespanSeconds));
            Assert.That(longLife.MaturityAge, Is.GreaterThan(shortLife.MaturityAge));
            Assert.That(longLife.PassiveDrain, Is.GreaterThan(shortLife.PassiveDrain));
        }

        [Test]
        public void InheritanceIsDeterministicAndParentsStayUnchanged()
        {
            var parentA = Extreme(.15f); var parentB = Extreme(.85f);
            string beforeA = JsonUtility.ToJson(parentA), beforeB = JsonUtility.ToJson(parentB);
            var first = GenomeInheritance.CreateChild(parentA, parentB, new EvolutionSettings(), new SeededRandomSource(9182));
            var second = GenomeInheritance.CreateChild(parentA, parentB, new EvolutionSettings(), new SeededRandomSource(9182));
            Assert.That(JsonUtility.ToJson(first.Genome), Is.EqualTo(JsonUtility.ToJson(second.Genome)));
            Assert.That(JsonUtility.ToJson(parentA), Is.EqualTo(beforeA));
            Assert.That(JsonUtility.ToJson(parentB), Is.EqualTo(beforeB));
            Assert.That(first.Genome, Is.Not.SameAs(parentA).And.Not.SameAs(parentB));
        }

        [Test]
        public void EveryChildGeneDrawsBetweenBothParentsWithoutMutation()
        {
            var low = Extreme(0); var high = Extreme(1);
            var child = GenomeInheritance.CreateChild(low, high, NoMutation(), new SequenceRandom(.5)).Genome;
            for (int i = 0; i < GenomeGeneCatalog.Count; i++)
            {
                GenomeGene gene = GenomeGeneCatalog.At(i);
                float value = GenomeGeneCatalog.Get(child, gene);
                Assert.That(value, Is.GreaterThan(GenomeGeneCatalog.Get(low, gene)), gene.ToString());
                Assert.That(value, Is.LessThan(GenomeGeneCatalog.Get(high, gene)), gene.ToString());
            }
        }

        [Test]
        public void DifferentSeedsCanProduceDifferentValidChildren()
        {
            var first = GenomeInheritance.CreateChild(Extreme(.2f), Extreme(.8f), NoMutation(), new SeededRandomSource(1)).Genome;
            var second = GenomeInheritance.CreateChild(Extreme(.2f), Extreme(.8f), NoMutation(), new SeededRandomSource(2)).Genome;
            Assert.That(JsonUtility.ToJson(first), Is.Not.EqualTo(JsonUtility.ToJson(second)));
            for (int i = 0; i < GenomeGeneCatalog.Count; i++)
            {
                GenomeGene gene = GenomeGeneCatalog.At(i);
                Assert.That(GenomeGeneCatalog.Get(first, gene), Is.InRange(GenomeGeneCatalog.Minimum(gene), GenomeGeneCatalog.Maximum(gene)));
            }
        }

        [Test]
        public void ForcedSmallMutationCanMovePositiveOrNegativeWithinBounds()
        {
            var settings = new EvolutionSettings { smallMutationChance = 1, smallMutationMagnitude = .03f, majorMutationChance = 0, importantMutationThreshold = 0 };
            var positive = GenomeInheritance.CreateChild(new Genome(), new Genome(), settings, new SequenceRandom(.5, .5, .5, 1, .75));
            var negative = GenomeInheritance.CreateChild(new Genome(), new Genome(), settings, new SequenceRandom(.5, .5, .5, 1, .25));
            Assert.That(positive.SmallMutationCount, Is.EqualTo(GenomeGeneCatalog.Count));
            Assert.That(negative.SmallMutationCount, Is.EqualTo(GenomeGeneCatalog.Count));
            Assert.That(positive.ImportantMutations[0].Difference, Is.Positive);
            Assert.That(negative.ImportantMutations[0].Difference, Is.Negative);
            Assert.That(Mathf.Abs(positive.ImportantMutations[0].Difference), Is.LessThanOrEqualTo(GenomeGeneCatalog.Span(positive.ImportantMutations[0].Gene) * .03f + .0001f));
        }

        [Test]
        public void MajorMutationsAreClassifiedAndImportantHistoryIsCapped()
        {
            var settings = new EvolutionSettings
            {
                majorMutationChance = 1,
                majorMutationMinimum = .1f,
                majorMutationMaximum = .2f,
                smallMutationChance = 1,
                importantMutationThreshold = 0,
                maximumImportantMutations = 2
            };
            var result = GenomeInheritance.CreateChild(new Genome(), new Genome(), settings, new SequenceRandom(.5, 0, 1, .75));
            Assert.That(result.MajorMutationCount, Is.EqualTo(GenomeGeneCatalog.Count));
            Assert.That(result.SmallMutationCount, Is.Zero);
            Assert.That(result.ImportantMutations.Count, Is.EqualTo(2));
            Assert.That(result.ImportantMutations[0].Major, Is.True);
        }

        [Test]
        public void GeneticDistanceIsNormalizedSymmetricAndSelfIsZero()
        {
            var low = Extreme(0); var high = Extreme(1);
            Assert.That(GeneticDistance.Between(low, low), Is.EqualTo(0).Within(.000001f));
            Assert.That(GeneticDistance.Between(low, high), Is.EqualTo(GeneticDistance.Between(high, low)).Within(.000001f));
            Assert.That(GeneticDistance.Between(low, high), Is.InRange(0, 1));
            Assert.That(GeneticDistance.Between(low, high), Is.GreaterThan(GeneticDistance.Between(low, Extreme(.1f))));
        }

        [Test]
        public void GeneticDistanceSanitizesInvalidValues()
        {
            var invalid = new Genome { bodySize = float.NaN, fertility = float.PositiveInfinity, camouflage = new Color(float.NaN, 1, 1) };
            float distance = GeneticDistance.Between(invalid, new Genome());
            Assert.That(float.IsNaN(distance) || float.IsInfinity(distance), Is.False);
            Assert.That(distance, Is.InRange(0, 1));
        }

        [Test]
        public void FounderAndChildLineageRulesAreStableWithoutGameObjects()
        {
            var first = CreatureLineageRecord.Founder(CreatureId.From("founder-b"), 10);
            var second = CreatureLineageRecord.Founder(CreatureId.From("founder-a"), 12).WithOffspringCount(4);
            var child = CreatureLineageRecord.Child(CreatureId.From("child"), first, second, 50, null);
            Assert.That(first.Generation, Is.Zero);
            Assert.That(first.FirstParentId.IsValid, Is.False);
            Assert.That(first.FounderId, Is.EqualTo(first.CreatureId));
            Assert.That(child.Generation, Is.EqualTo(1));
            Assert.That(child.FirstParentId, Is.EqualTo(first.CreatureId));
            Assert.That(child.SecondParentId, Is.EqualTo(second.CreatureId));
            Assert.That(child.FounderId.Value, Is.EqualTo("founder-a"));
            Assert.That(child.AgeAt(65), Is.EqualTo(15));
            Assert.That(second.OffspringCount, Is.EqualTo(4));
        }

        [Test]
        public void ChildGenerationUsesOlderParentGenerationAndMutationListIsBounded()
        {
            var rootA = CreatureLineageRecord.Founder(CreatureId.From("a"), 0);
            var rootB = CreatureLineageRecord.Founder(CreatureId.From("b"), 0);
            var older = CreatureLineageRecord.Child(CreatureId.From("older"), rootA, rootB, 1, null);
            var records = new MutationRecord[20];
            var child = CreatureLineageRecord.Child(CreatureId.From("next"), older, rootB, 2, records);
            Assert.That(child.Generation, Is.EqualTo(2));
            Assert.That(child.ImportantMutations.Length, Is.EqualTo(CreatureLineageRecord.MutationRecordCap));
        }

        [Test]
        public void EligiblePairPassesWithoutChangingInput()
        {
            var first = Eligible("a"); var second = Eligible("b");
            var result = ReproductionEligibility.Evaluate(first, second, .1f, .3f, 13, 40);
            Assert.That(result.Eligible, Is.True);
            Assert.That(first.energy, Is.EqualTo(90));
            Assert.That(second.energy, Is.EqualTo(90));
        }

        [TestCase(ReproductionRejection.Dead)]
        [TestCase(ReproductionRejection.Juvenile)]
        [TestCase(ReproductionRejection.LowHealth)]
        [TestCase(ReproductionRejection.InsufficientEnergy)]
        [TestCase(ReproductionRejection.Cooldown)]
        [TestCase(ReproductionRejection.Reserved)]
        public void CandidateEligibilityRejectsInvalidStates(ReproductionRejection reason)
        {
            var first = Eligible("a"); var second = Eligible("b");
            switch (reason)
            {
                case ReproductionRejection.Dead: first.alive = false; break;
                case ReproductionRejection.Juvenile: first.age = 5; break;
                case ReproductionRejection.LowHealth: first.health = 0; break;
                case ReproductionRejection.InsufficientEnergy: first.energy = 10; break;
                case ReproductionRejection.Cooldown: first.cooldownRemaining = 2; break;
                case ReproductionRejection.Reserved: first.reserved = true; break;
            }
            Assert.That(ReproductionEligibility.Evaluate(first, second, .1f, .3f, 13, 40).Rejection, Is.EqualTo(reason));
        }

        [Test]
        public void PairEligibilityRejectsSelfIncompatibilityAndPopulationCap()
        {
            var first = Eligible("same"); var second = Eligible("same");
            Assert.That(ReproductionEligibility.Evaluate(first, second, .1f, .3f, 13, 40).Rejection, Is.EqualTo(ReproductionRejection.SameCreature));
            second.creatureId = CreatureId.From("other");
            Assert.That(ReproductionEligibility.Evaluate(first, second, .8f, .3f, 13, 40).Rejection, Is.EqualTo(ReproductionRejection.Incompatible));
            Assert.That(ReproductionEligibility.Evaluate(first, second, .1f, .3f, 40, 40).Rejection, Is.EqualTo(ReproductionRejection.PopulationFull));
        }

        [Test]
        public void EligibilityRejectsNonfiniteDataWithoutMutation()
        {
            var first = Eligible("a"); var second = Eligible("b"); first.maximumEnergy = float.NaN;
            var result = ReproductionEligibility.Evaluate(first, second, .1f, .3f, 13, 40);
            Assert.That(result.Rejection, Is.EqualTo(ReproductionRejection.InvalidData));
            Assert.That(float.IsNaN(first.maximumEnergy), Is.True);
        }

        static ReproductionCandidate Eligible(string id)
        {
            return new ReproductionCandidate
            {
                creatureId = CreatureId.From(id), alive = true, age = 60, maturityAge = 20,
                health = 100, energy = 90, maximumEnergy = 100, reproductionEnergyFraction = .7f,
                cooldownRemaining = 0, reserved = false
            };
        }
    }
}
