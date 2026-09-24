using System;
using UnityEngine;

namespace WildType
{
    [Serializable]
    public sealed class EvolutionSettings
    {
        [Range(0, 1)] public float smallMutationChance = .15f;
        [Range(0, .25f)] public float smallMutationMagnitude = .03f;
        [Range(0, 1)] public float majorMutationChance = .008f;
        [Range(0, 1)] public float majorMutationMinimum = .1f;
        [Range(0, 1)] public float majorMutationMaximum = .24f;
        [Range(0, 1)] public float importantMutationThreshold = .08f;
        [Range(1, 16)] public int maximumImportantMutations = 6;
        [Range(0, 1)] public float compatibilityThreshold = .34f;

        public EvolutionSettings ValidatedCopy()
        {
            var copy = (EvolutionSettings)MemberwiseClone();
            copy.smallMutationChance = Genome.Safe(copy.smallMutationChance, .15f, 0, 1);
            copy.smallMutationMagnitude = Genome.Safe(copy.smallMutationMagnitude, .03f, 0, .25f);
            copy.majorMutationChance = Genome.Safe(copy.majorMutationChance, .008f, 0, 1);
            copy.majorMutationMinimum = Genome.Safe(copy.majorMutationMinimum, .1f, 0, 1);
            copy.majorMutationMaximum = Genome.Safe(copy.majorMutationMaximum, .24f, copy.majorMutationMinimum, 1);
            copy.importantMutationThreshold = Genome.Safe(copy.importantMutationThreshold, .08f, 0, 1);
            copy.maximumImportantMutations = Mathf.Clamp(copy.maximumImportantMutations, 1, 16);
            copy.compatibilityThreshold = Genome.Safe(copy.compatibilityThreshold, .34f, 0, 1);
            return copy;
        }
    }
}
