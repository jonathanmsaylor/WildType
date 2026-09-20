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
        public void Configure(Ecosystem owner, float delay)
        {
            world = owner; regenerationSeconds = Genome.Safe(delay, 24, 14, 38);
            Available = true; fruit.gameObject.SetActive(true); marker.enabled = false;
        }
        public float Consume()
        {
            if (!Available) return 0;
            Available = false; timer = regenerationSeconds; fruit.gameObject.SetActive(false); marker.enabled = false;
            return nutrition;
        }
        void Update()
        {
            if (!world || world.Session.Paused) return;
            if (!Available)
            {
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    Available = true; fruit.gameObject.SetActive(true);
                    world.Session.Fx.Burst(transform.position + Vector3.up, new Color(.85f, .95f, .4f), 9, .7f);
                }
            }
        }
        public void Highlight(bool value) { if (marker) marker.enabled = value && Available; }
    }
}
