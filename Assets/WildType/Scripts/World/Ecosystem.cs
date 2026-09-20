using System.Collections.Generic;
using UnityEngine;
namespace WildType
{
    public sealed class Ecosystem : MonoBehaviour
    {
        public const float PlayRadius = 121;
        public const int GroundMask = 1 << 8;
        public const int ObstacleMask = 1 << 9;
        public const int WorldMask = GroundMask | ObstacleMask;
        public const int FoodCap = 72;
        readonly List<FoodPlant> foods = new List<FoodPlant>(FoodCap);
        public IReadOnlyList<FoodPlant> Foods => foods;
        public StageSession Session { get; private set; }
        public int AvailableFood { get { int n = 0; foreach (var p in foods) if (p && p.Available) n++; return n; } }
        // The small bounded registry is the future spatial-index seam; never search the scene.
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
        public void Populate(StageSession session, int seed)
        {
            Session = session; foods.Clear(); var random = new System.Random(seed);
            for (int i = 0; i < FoodCap; i++)
            {
                Vector3 position;
                if (i < 8) { float angle = i * Mathf.PI / 4; position = new Vector3(Mathf.Sin(angle) * (i == 0 ? 4 : 10), 0, Mathf.Cos(angle) * (i == 0 ? 4 : 10)); }
                else position = RandomGround(random, 18, 109);
                // Reject obstructed food sites so fruit remains reachable.
                for (int attempt = 0; attempt < 40 && Physics.CheckSphere(position + Vector3.up, 2, ObstacleMask); attempt++)
                    position = RandomGround(random, 8, 109);
                position.y = Height(position.x, position.z) + .06f;
                var plant = Instantiate(session.foodPrefab, position, Quaternion.Euler(0, (float)random.NextDouble() * 360, 0), session.RuntimeRoot);
                plant.name = "Brightfruit " + i; plant.Configure(this, 18 + (float)random.NextDouble() * 15); foods.Add(plant);
            }
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
        public string Zone(Vector3 p) => p.x > 35 ? "Amber flats" : p.x < -35 ? "Fernwood" : "The meadow";
    }
}
