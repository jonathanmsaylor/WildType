using UnityEngine;
namespace WildType
{
    public sealed class FoodPlant : MonoBehaviour
    {
        public Transform fruit;
        public Renderer marker;
        public float nutrition = 42;
        public float regenerationSeconds = 24;
        float timer;
        Ecosystem world;
        public bool Available { get; private set; } = true;
        public float Remaining => timer;
        public Habitat Region { get; private set; }
        public string DisplayName => EcologyRules.FoodName(Region);
        public void ConfigureRegion(Ecosystem owner, Habitat region, float variation)
        {
            Region = region; Configure(owner, EcologyRules.Regrowth(region, variation));
            nutrition = EcologyRules.Nutrition(region);
            // Reuse authored geometry/materials: no additional food objects or per-frame allocation.
            transform.localScale = region == Habitat.Woodland ? new Vector3(1.25f, .62f, 1.25f) : region == Habitat.Dry ? new Vector3(.85f, 1.45f, .85f) : Vector3.one;
            var block = new MaterialPropertyBlock(); block.SetColor("_BaseColor", EcologyRules.FruitColor(region));
            foreach (var renderer in fruit.GetComponentsInChildren<Renderer>(true)) renderer.SetPropertyBlock(block);
            fruit.localScale = region == Habitat.Dry ? new Vector3(1.5f, 1.1f, 1.5f) : region == Habitat.Woodland ? new Vector3(1.1f, .72f, 1.1f) : Vector3.one;
        }
        public void Configure(Ecosystem owner, float delay)
        {
            world = owner; regenerationSeconds = Genome.Safe(delay, 24, 14, 115);
            nutrition = Genome.Safe(nutrition, 42, 1, 100); timer = 0;
            Available = true; if (fruit) fruit.gameObject.SetActive(true); if (marker) marker.enabled = false;
        }
        public float Consume()
        {
            if (!Available) return 0;
            if (world && (!world.Session || !world.Session.Ready || world.Session.Paused)) return 0;
            Available = false; timer = Genome.Safe(regenerationSeconds, 24, 14, 115);
            if (fruit) fruit.gameObject.SetActive(false); if (marker) marker.enabled = false;
            return Genome.Safe(nutrition, 42, 1, 100);
        }
        void Update()
        {
            if (!world || !world.Session || !world.Session.Ready || world.Session.Paused) return;
            Tick(Time.deltaTime);
        }
        public void Tick(float dt)
        {
            if (float.IsNaN(dt) || float.IsInfinity(dt) || dt <= 0 || (world && (!world.Session || !world.Session.Ready || world.Session.Paused))) return;
            if (!Available)
            {
                timer = Mathf.Max(0, timer - dt);
                if (timer <= 0)
                {
                    Available = true; if (fruit) fruit.gameObject.SetActive(true);
                    if (world && world.Session) world.Session.Fx.Burst(transform.position + Vector3.up, EcologyRules.FruitColor(Region), 9, .7f);
                }
            }
        }
        public void Highlight(bool value) { if (marker) marker.enabled = value && Available; }
    }
}
