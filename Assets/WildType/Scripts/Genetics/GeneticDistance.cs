using UnityEngine;

namespace WildType
{
    public static class GeneticDistance
    {
        public static float Between(Genome first, Genome second)
        {
            var a = first == null ? new Genome() : first.Copy();
            var b = second == null ? new Genome() : second.Copy();
            a.Validate(); b.Validate();
            float total = 0;
            for (int i = 0; i < GenomeGeneCatalog.Count; i++)
            {
                GenomeGene gene = GenomeGeneCatalog.At(i);
                total += Mathf.Abs(GenomeGeneCatalog.Normalize(gene, GenomeGeneCatalog.Get(a, gene)) -
                                   GenomeGeneCatalog.Normalize(gene, GenomeGeneCatalog.Get(b, gene)));
            }
            return Mathf.Clamp01(total / GenomeGeneCatalog.Count);
        }

        public static bool Compatible(Genome first, Genome second, EvolutionSettings settings)
        {
            float threshold = (settings ?? new EvolutionSettings()).ValidatedCopy().compatibilityThreshold;
            return Between(first, second) <= threshold;
        }
    }
}
