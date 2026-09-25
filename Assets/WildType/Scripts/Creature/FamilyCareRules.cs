using UnityEngine;
namespace WildType
{
    // Usable energy, not raw plant nutrition: metabolism must not amplify a gift.
    public static class FamilyCareRules
    {
        public const float Range = 3.5f, MaximumCost = 18, Efficiency = .8f, ReserveFraction = .25f, Cooldown = 8;
        public static bool DirectChild(CreatureId parent, CreatureLineageRecord child) =>
            parent.IsValid && child != null && child.Alive && child.CreatureId != parent &&
            (child.FirstParentId == parent || child.SecondParentId == parent);
        public static bool Transfer(float parentEnergy, float parentMax, float childEnergy, float childMax, out float cost, out float gain)
        {
            cost = gain = 0;
            if (!Finite(parentEnergy) || !Finite(parentMax) || !Finite(childEnergy) || !Finite(childMax) ||
                parentMax <= 0 || childMax <= 0 || parentEnergy < 0 || childEnergy < 0 || parentEnergy > parentMax || childEnergy > childMax) return false;
            float available = Mathf.Max(0, parentEnergy - parentMax * ReserveFraction);
            float candidate = Mathf.Min(MaximumCost, available, (childMax - childEnergy) / Efficiency);
            if (candidate * Efficiency < 1) return false;
            cost = candidate; gain = cost * Efficiency; return true;
        }
        static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
