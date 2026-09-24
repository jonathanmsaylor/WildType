using System;
using System.Collections.Generic;
using UnityEngine;

namespace WildType
{
    [Serializable]
    public struct MutationRecord
    {
        [SerializeField] GenomeGene gene;
        [SerializeField] float inheritedValue;
        [SerializeField] float childValue;
        [SerializeField] bool major;

        public GenomeGene Gene => gene;
        public string TraitName => GenomeGeneCatalog.Name(gene);
        public float InheritedValue => inheritedValue;
        public float ChildValue => childValue;
        public float Difference => childValue - inheritedValue;
        public bool Major => major;

        public MutationRecord(GenomeGene gene, float inheritedValue, float childValue, bool major)
        {
            this.gene = gene;
            this.inheritedValue = inheritedValue;
            this.childValue = childValue;
            this.major = major;
        }
    }

    public sealed class InheritanceResult
    {
        readonly MutationRecord[] importantMutations;
        public Genome Genome { get; }
        public IReadOnlyList<MutationRecord> ImportantMutations => importantMutations;
        public int SmallMutationCount { get; }
        public int MajorMutationCount { get; }

        public InheritanceResult(Genome genome, List<MutationRecord> records, int smallCount, int majorCount)
        {
            Genome = genome ?? throw new ArgumentNullException(nameof(genome));
            importantMutations = records == null ? Array.Empty<MutationRecord>() : records.ToArray();
            SmallMutationCount = smallCount;
            MajorMutationCount = majorCount;
        }
    }

    public static class GenomeInheritance
    {
        public static InheritanceResult CreateChild(Genome parentA, Genome parentB, EvolutionSettings settings, IRandomSource random)
        {
            if (parentA == null) throw new ArgumentNullException(nameof(parentA));
            if (parentB == null) throw new ArgumentNullException(nameof(parentB));
            if (random == null) throw new ArgumentNullException(nameof(random));

            var a = parentA.Copy(); a.Validate();
            var b = parentB.Copy(); b.Validate();
            var safeSettings = (settings ?? new EvolutionSettings()).ValidatedCopy();
            var child = new Genome();
            var records = new List<MutationRecord>(safeSettings.maximumImportantMutations);
            int smallCount = 0, majorCount = 0;

            for (int i = 0; i < GenomeGeneCatalog.Count; i++)
            {
                GenomeGene gene = GenomeGeneCatalog.At(i);
                float fromA = GenomeGeneCatalog.Get(a, gene);
                float fromB = GenomeGeneCatalog.Get(b, gene);
                // Keep both parents represented; mutation, not scripted averaging, restores extremes.
                float inherited = Mathf.Lerp(fromA, fromB, .15f + .7f * (float)random.Next01());
                float value = inherited;
                bool major = random.Next01() < safeSettings.majorMutationChance;
                bool small = !major && random.Next01() < safeSettings.smallMutationChance;
                if (major)
                {
                    float magnitude = Mathf.Lerp(safeSettings.majorMutationMinimum, safeSettings.majorMutationMaximum, (float)random.Next01());
                    value += Signed(random) * magnitude * GenomeGeneCatalog.Span(gene);
                    majorCount++;
                }
                else if (small)
                {
                    float magnitude = Mathf.Lerp(.2f, 1f, (float)random.Next01()) * safeSettings.smallMutationMagnitude;
                    value += Signed(random) * magnitude * GenomeGeneCatalog.Span(gene);
                    smallCount++;
                }

                value = GenomeGeneCatalog.Clamp(gene, value);
                GenomeGeneCatalog.Set(child, gene, value);
                float normalizedDifference = Mathf.Abs(value - inherited) / GenomeGeneCatalog.Span(gene);
                if ((major || normalizedDifference >= safeSettings.importantMutationThreshold) && records.Count < safeSettings.maximumImportantMutations)
                    records.Add(new MutationRecord(gene, inherited, value, major));
            }

            child.Validate();
            return new InheritanceResult(child, records, smallCount, majorCount);
        }

        static float Signed(IRandomSource random) => random.Next01() < .5 ? -1f : 1f;
    }
}
