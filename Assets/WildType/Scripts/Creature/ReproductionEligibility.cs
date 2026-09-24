using System;
using UnityEngine;

namespace WildType
{
    public enum ReproductionRejection
    {
        None,
        Dead,
        Juvenile,
        LowHealth,
        InsufficientEnergy,
        Cooldown,
        SameCreature,
        Incompatible,
        Reserved,
        PopulationFull,
        InvalidData
    }

    [Serializable]
    public struct ReproductionCandidate
    {
        public CreatureId creatureId;
        public bool alive;
        public float age;
        public float maturityAge;
        public float health;
        public float energy;
        public float maximumEnergy;
        public float reproductionEnergyFraction;
        public float cooldownRemaining;
        public bool reserved;
    }

    public readonly struct ReproductionEligibilityResult
    {
        public bool Eligible => Rejection == ReproductionRejection.None;
        public ReproductionRejection Rejection { get; }
        public ReproductionEligibilityResult(ReproductionRejection rejection) { Rejection = rejection; }
    }

    public static class ReproductionEligibility
    {
        public static ReproductionEligibilityResult Evaluate(ReproductionCandidate first, ReproductionCandidate second,
            float geneticDistance, float compatibilityThreshold, int population, int populationCap)
        {
            if (!first.creatureId.IsValid || !second.creatureId.IsValid || !Finite(geneticDistance) ||
                !Finite(compatibilityThreshold) || population < 0 || populationCap < 1)
                return Reject(ReproductionRejection.InvalidData);
            if (first.creatureId == second.creatureId) return Reject(ReproductionRejection.SameCreature);
            if (population >= populationCap) return Reject(ReproductionRejection.PopulationFull);
            ReproductionRejection firstReason = CandidateReason(first);
            if (firstReason != ReproductionRejection.None) return Reject(firstReason);
            ReproductionRejection secondReason = CandidateReason(second);
            if (secondReason != ReproductionRejection.None) return Reject(secondReason);
            if (geneticDistance > Mathf.Clamp01(compatibilityThreshold)) return Reject(ReproductionRejection.Incompatible);
            return Reject(ReproductionRejection.None);
        }

        public const float MinimumHealth = 60;
        public static ReproductionRejection CandidateReason(ReproductionCandidate candidate)
        {
            if (!candidate.creatureId.IsValid || !Finite(candidate.age) || !Finite(candidate.maturityAge) ||
                !Finite(candidate.health) || !Finite(candidate.energy) || !Finite(candidate.maximumEnergy) ||
                !Finite(candidate.reproductionEnergyFraction) || !Finite(candidate.cooldownRemaining) ||
                candidate.maximumEnergy <= 0 || candidate.maturityAge < 0 || candidate.age < 0 ||
                candidate.reproductionEnergyFraction < 0 || candidate.reproductionEnergyFraction > 1)
                return ReproductionRejection.InvalidData;
            if (!candidate.alive) return ReproductionRejection.Dead;
            if (candidate.age < candidate.maturityAge) return ReproductionRejection.Juvenile;
            if (candidate.health < MinimumHealth) return ReproductionRejection.LowHealth;
            if (candidate.energy < candidate.maximumEnergy * Mathf.Clamp01(candidate.reproductionEnergyFraction))
                return ReproductionRejection.InsufficientEnergy;
            if (candidate.cooldownRemaining > 0) return ReproductionRejection.Cooldown;
            if (candidate.reserved) return ReproductionRejection.Reserved;
            return ReproductionRejection.None;
        }

        static ReproductionEligibilityResult Reject(ReproductionRejection rejection) => new ReproductionEligibilityResult(rejection);
        static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
