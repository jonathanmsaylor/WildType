using UnityEngine;
namespace WildType
{
    public enum Habitat { Meadow, Dry, Woodland }
    // Local clearance exposes existing stride/turn tradeoffs; no new genes or resource multipliers.
    public static class EcologyRules
    {
        // One local pressure rule: repeated brightfruit harvests exhaust a plant's fast recovery.
        // Untouched ripe plants recover their reserve; other habitats remain alternatives.
        public const float HarvestDelayStep = 45, MaximumGrazingDelay = 120, RipeRestRecovery = .5f;
        public static float GrazingDelay(Habitat region, float value) => region == Habitat.Meadow
            ? Genome.Safe(value, 0, 0, MaximumGrazingDelay) : 0;
        public static Habitat Region(Vector3 point) => point.x > 35 ? Habitat.Dry : point.x < -35 ? Habitat.Woodland : Habitat.Meadow;
        public static string Name(Habitat region) => region == Habitat.Dry ? "Amber flats" : region == Habitat.Woodland ? "Fernwood" : "The meadow";
        public static string FoodName(Habitat region) => region == Habitat.Dry ? "Sunpod" : region == Habitat.Woodland ? "Fernberry" : "Brightfruit";
        public static float Nutrition(Habitat region) => region == Habitat.Dry ? 72 : region == Habitat.Woodland ? 22 : 42;
        public static float Regrowth(Habitat region, float variation)
        {
            variation = Genome.Safe(variation, .5f, 0, 1);
            return region == Habitat.Dry ? Mathf.Lerp(85, 115, variation) : region == Habitat.Woodland ? Mathf.Lerp(18, 26, variation) : Mathf.Lerp(25, 33, variation);
        }
        public static Color FruitColor(Habitat region) => region == Habitat.Dry ? new Color(1, .77f, .12f) : region == Habitat.Woodland ? new Color(.65f, .48f, 1) : new Color(.97f, .33f, .13f);
        public static float TravelLimit(Phenotype stats, Vector3 point, bool sprint)
        {
            float open = sprint ? stats.SprintSpeed : stats.WalkSpeed;
            float x = Genome.Safe(point.x, 0, -200, 200);
            float brush = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-35, -43, x));
            // Tight understory requires a turn within .45 m: v = angular speed * radius.
            // Cap, never multiply, the original speed. Sharp-turning genomes retain more stride.
            return Mathf.Lerp(open, Mathf.Min(open, stats.TurnRate * .45f), brush);
        }
        public static string Observation(Habitat region) => region == Habitat.Dry
            ? "Amber flats: rich sunpods, long waits. Keep a reserve."
            : region == Habitat.Woodland ? "Fernwood: small bites, quick regrowth. Sharp turns help."
            : "Meadow: repeated grazing slows brightfruit recovery. Try fresh patches.";
    }
}
