using UnityEngine;
namespace WildType
{
    public sealed class FeedbackPool : MonoBehaviour
    {
        ParticleSystem particles;
        public void Configure(Material material)
        {
            particles = gameObject.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main; main.loop = true; main.playOnAwake = false; main.maxParticles = 512;
            main.simulationSpace = ParticleSystemSimulationSpace.World; main.startLifetime = .7f; main.startSize = .12f; main.gravityModifier = .45f;
            var emission = particles.emission; emission.enabled = false;
            var shape = particles.shape; shape.enabled = false;
            var renderer = particles.GetComponent<ParticleSystemRenderer>(); renderer.sharedMaterial = material;
            particles.Play();
        }
        public void Burst(Vector3 at, Color color, int count, float speed)
        {
            if (!particles) return;
            for (int i = 0; i < count; i++)
            {
                var emit = new ParticleSystem.EmitParams { position = at, velocity = Random.insideUnitSphere * speed + Vector3.up * speed, startColor = color, startSize = .08f + Random.value * .08f };
                particles.Emit(emit, 1);
            }
        }
        public int ParticleCount => particles ? particles.particleCount : 0;
        public void Clear() { if (particles) particles.Clear(); }
    }
}
