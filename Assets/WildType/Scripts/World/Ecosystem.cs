using System;
using System.Collections.Generic;
using UnityEngine;
namespace WildType
{
    public sealed class Ecosystem : MonoBehaviour
    {
        public const float PlayRadius = 121;
        public const int GroundMask = 1 << 8, ObstacleMask = 1 << 9, WorldMask = GroundMask | ObstacleMask;
        public const int FoodCap = 72;
        readonly List<FoodPlant> foods = new List<FoodPlant>(FoodCap);
        public IReadOnlyList<FoodPlant> Foods => foods;
        public StageSession Session { get; private set; }
        Habitat observedRegion, candidateRegion;
        float observationTimer, stableRegion, emptyTime, noticeCooldown;
        CreatureAgent observedPlayer;
        public int AvailableFood { get { int n = 0; foreach (var p in foods) if (p && p.Available) n++; return n; } }
        public FoodPlant NearestFood(Vector3 point, float range)
        {
            FoodPlant best = null; float distance = range * range;
            foreach (var food in foods)
            {
                if (!food || !food.Available) continue;
                float d = (food.transform.position - point).sqrMagnitude;
                if (d < distance) { distance = d; best = food; }
            }
            return best;
        }
        // Only visible candidates, valued by useful nutrition minus estimated travel cost.
        // No remote availability or unseen regrowth timer is exposed to the brain.
        public FoodPlant Forage(CreatureAgent actor, FoodPlant avoid = null)
        {
            FoodPlant best = null; float score = float.NegativeInfinity;
            foreach (var food in foods)
            {
                if (!food || !food.Available || food == avoid) continue;
                float distance = Vector3.Distance(food.transform.position, actor.transform.position);
                if (distance > actor.Stats.Vision || Physics.Linecast(actor.transform.position + Vector3.up, food.transform.position + Vector3.up, ObstacleMask)) continue;
                float useful = Mathf.Min(actor.Stats.MaxEnergy - actor.Vitals.Energy, food.nutrition * actor.Stats.NutritionFactor);
                float seconds = Mathf.Max(0, distance - actor.Interaction.Reach) / Mathf.Max(.1f, EcologyRules.TravelLimit(actor.Stats, actor.transform.position, false));
                float value = (useful - seconds * (actor.Stats.PassiveDrain + actor.Stats.MoveCost)) / (2 + seconds);
                if (value > score) { score = value; best = food; }
            }
            return score > 0 ? best : null;
        }
        public void Populate(StageSession session, int seed)
        {
            Session = session; foods.Clear(); observedPlayer = null; observationTimer = stableRegion = emptyTime = noticeCooldown = 0;
            var random = new System.Random(seed);
            for (int i = 0; i < FoodCap; i++)
            {
                Habitat region = i < 32 ? Habitat.Meadow : i < 60 ? Habitat.Woodland : Habitat.Dry;
                Vector3 position = default; bool found = false;
                for (int attempt = 0; attempt < 512; attempt++)
                {
                    if (i < 8 && attempt == 0)
                    { float angle = i * Mathf.PI / 4; position = new Vector3(Mathf.Sin(angle) * (i == 0 ? 4 : 10), 0, Mathf.Cos(angle) * (i == 0 ? 4 : 10)); }
                    else position = SiteCandidate(random, region);
                    position.y = Height(position.x, position.z) + .06f;
                    if (Physics.CheckSphere(position + Vector3.up, 2, ObstacleMask)) continue;
                    float spacing = region == Habitat.Dry ? 12 : region == Habitat.Woodland ? 4 : 7;
                    bool occupied = false;
                    foreach (var other in foods) if ((other.transform.position - position).sqrMagnitude < spacing * spacing) { occupied = true; break; }
                    if (!occupied) { found = true; break; }
                }
                if (!found) throw new InvalidOperationException("No safe bounded food site for slot " + i);
                var plant = Instantiate(session.foodPrefab, position, Quaternion.Euler(0, (float)random.NextDouble() * 360, 0), session.RuntimeRoot);
                plant.name = EcologyRules.FoodName(region) + " " + i;
                plant.ConfigureRegion(this, region, (float)random.NextDouble()); foods.Add(plant);
            }
        }
        public static Vector3 SiteCandidate(System.Random random, Habitat region)
        {
            // Regional quotas guarantee readable scarcity instead of relying on one fortunate seed.
            float x, z;
            do
            {
                x = region == Habitat.Dry ? Mathf.Lerp(39, 105, (float)random.NextDouble()) : region == Habitat.Woodland
                    ? Mathf.Lerp(-105, -39, (float)random.NextDouble()) : Mathf.Lerp(-31, 31, (float)random.NextDouble());
                z = Mathf.Lerp(-102, 102, (float)random.NextDouble());
            } while (x * x + z * z > 109 * 109);
            return new Vector3(x, Height(x, z), z);
        }
        public static Vector3 RandomGround(System.Random random, float inner, float outer)
        {
            float angle = (float)random.NextDouble() * Mathf.PI * 2;
            float r = Mathf.Sqrt(Mathf.Lerp(inner * inner, outer * outer, (float)random.NextDouble()));
            float x = Mathf.Sin(angle) * r, z = Mathf.Cos(angle) * r;
            return new Vector3(x, Height(x, z), z);
        }
        public static float Height(float x, float z)
        {
            float radius = new Vector2(x, z).magnitude;
            float rolling = Mathf.Sin(x * .044f) * 2.5f + Mathf.Cos(z * .052f) * 1.8f + Mathf.Sin((x + z) * .075f) * .7f;
            float rim = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(111, 128, radius));
            return rolling + rim * (10 + 3 * Mathf.Sin(Mathf.Atan2(z, x) * 9));
        }
        public string Zone(Vector3 p) => EcologyRules.Name(EcologyRules.Region(p));
        void Update()
        {
            if (!Session || !Session.Ready || Session.Paused || !Session.Player || Session.Player.Vitals.Dead) return;
            observationTimer += Time.deltaTime; if (observationTimer < 1) return;
            float dt = observationTimer; observationTimer = 0; noticeCooldown -= dt;
            var player = Session.Player; var here = EcologyRules.Region(player.transform.position);
            if (observedPlayer != player)
            { observedPlayer = player; observedRegion = candidateRegion = here; stableRegion = emptyTime = 0; }
            if (candidateRegion != here) { candidateRegion = here; stableRegion = 0; }
            else stableRegion += dt;
            bool empty = !NearestFood(player.transform.position, player.Stats.Vision);
            emptyTime = empty && player.Vitals.Energy < player.Stats.MaxEnergy * .75f ? emptyTime + dt : 0;
            if (noticeCooldown > 0 || Session.CurrentNotice.Length > 0) return;
            if (here != observedRegion && stableRegion >= 2)
            { observedRegion = here; Session.ShowNotice(EcologyRules.Observation(here), 6); noticeCooldown = 12; }
            else if (emptyTime > 7)
            { Session.ShowNotice("Little ripe forage nearby. Try beyond this patch.", 5); noticeCooldown = 30; emptyTime = 0; }
        }
    }
}
