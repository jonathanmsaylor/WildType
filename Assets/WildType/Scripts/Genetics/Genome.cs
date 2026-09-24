using System;
using UnityEngine;
namespace WildType
{
    [Serializable]
    public sealed class Genome
    {
        public float bodySize = 1f, legLength = 1f, movementSpeed = 6f, turnAgility = 7f;
        public float vision = 24f, metabolism = 1f, energyEfficiency = 1f, maximumEnergy = 100f, stamina = 100f;
        public float fertility = 1f, reproductionThreshold = .72f, offspringTendency = 1f, lifespan = 600f;
        public Color camouflage = new Color(.2f, .65f, .55f);
        public Genome Copy() => (Genome)MemberwiseClone();
        public static float Safe(float v, float fallback, float min, float max) =>
            float.IsNaN(v) || float.IsInfinity(v) ? fallback : Mathf.Clamp(v, min, max);
        public void Validate()
        {
            bodySize = Safe(bodySize, 1, .65f, 1.7f); legLength = Safe(legLength, 1, .6f, 1.6f);
            movementSpeed = Safe(movementSpeed, 6, 3, 10); turnAgility = Safe(turnAgility, 7, 2, 12);
            vision = Safe(vision, 24, 10, 45); metabolism = Safe(metabolism, 1, .5f, 1.8f);
            energyEfficiency = Safe(energyEfficiency, 1, .6f, 1.5f);
            maximumEnergy = Safe(maximumEnergy, 100, 60, 150); stamina = Safe(stamina, 100, 60, 150);
            fertility = Safe(fertility, 1, .5f, 1.5f);
            reproductionThreshold = Safe(reproductionThreshold, .72f, .5f, .9f);
            offspringTendency = Safe(offspringTendency, 1, .4f, 1.6f);
            lifespan = Safe(lifespan, 600, 240, 900);
            camouflage = new Color(Safe(camouflage.r, .2f, .08f, .9f), Safe(camouflage.g, .65f, .08f, .9f), Safe(camouflage.b, .55f, .08f, .9f), 1);
        }
        public static Genome Varied(Genome source, System.Random random)
        {
            var g = source.Copy();
            g.bodySize *= .9f + (float)random.NextDouble() * .2f;
            g.legLength *= .9f + (float)random.NextDouble() * .2f;
            g.movementSpeed *= .92f + (float)random.NextDouble() * .16f;
            g.fertility *= .94f + (float)random.NextDouble() * .12f;
            g.reproductionThreshold *= .97f + (float)random.NextDouble() * .06f;
            g.offspringTendency *= .94f + (float)random.NextDouble() * .12f;
            g.lifespan *= .95f + (float)random.NextDouble() * .1f;
            g.Validate(); return g;
        }
    }
    public readonly struct Phenotype
    {
        public readonly float Size, Height, WalkSpeed, SprintSpeed, Acceleration, TurnRate, MaxEnergy, MaxStamina;
        public readonly float PassiveDrain, MoveCost, SprintCost, StaminaCost, StaminaRecovery, NutritionFactor, Vision;
        public readonly float FertilityFactor, ReproductionEnergyFraction, ReproductiveMotivation, ReproductionCooldown;
        public readonly float LifespanSeconds, MaturityAge, ReproductionMaintenanceCost;
        public Phenotype(Genome source)
        {
            // Never modify a preset when calculating or sanitizing derived statistics.
            var g = source.Copy(); g.Validate();
            Size = g.bodySize; Height = Size * (.85f + g.legLength);
            float mass = Size * Size * Size;
            float burst = Mathf.Lerp(1.15f, .86f, Mathf.InverseLerp(.6f, 1.5f, g.energyEfficiency));
            SprintSpeed = g.movementSpeed * (.8f + .2f * g.legLength) * burst;
            WalkSpeed = SprintSpeed * .57f; Acceleration = 13f / Mathf.Sqrt(Size);
            TurnRate = g.turnAgility / (.75f + .25f * g.legLength);
            MaxEnergy = g.maximumEnergy * (.65f + .35f * mass); MaxStamina = g.stamina;
            float lifespan01 = Mathf.InverseLerp(240, 900, g.lifespan);
            ReproductionMaintenanceCost = .025f * g.fertility * g.fertility + .012f * g.offspringTendency + .025f * lifespan01;
            PassiveDrain = (.14f + mass * .09f + g.vision * .0025f) * g.metabolism / g.energyEfficiency + ReproductionMaintenanceCost;
            MoveCost = (.14f + g.movementSpeed * .045f) * Mathf.Sqrt(mass) / g.energyEfficiency;
            SprintCost = (.35f + g.movementSpeed * .12f) / g.energyEfficiency;
            StaminaCost = (10f + g.movementSpeed * 1.4f) / g.energyEfficiency;
            StaminaRecovery = 14f * g.metabolism; NutritionFactor = .8f + g.metabolism * .2f; Vision = g.vision;
            FertilityFactor = g.fertility;
            ReproductionEnergyFraction = g.reproductionThreshold;
            ReproductiveMotivation = g.offspringTendency;
            ReproductionCooldown = 52f / g.fertility;
            LifespanSeconds = g.lifespan;
            MaturityAge = Mathf.Lerp(18f, 42f, lifespan01);
        }
    }
}
