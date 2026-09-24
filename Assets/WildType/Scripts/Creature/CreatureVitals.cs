using System;
using UnityEngine;
namespace WildType
{
    public enum CreatureDeathCause { Unknown, Starvation, OldAge }
    public sealed class CreatureVitals : MonoBehaviour
    {
        public float Energy { get; private set; }
        public float Stamina { get; private set; }
        public float Health { get; private set; }
        public bool Dead => Health <= 0;
        public bool SprintLocked { get; private set; }
        public Phenotype Stats { get; private set; }
        public CreatureDeathCause DeathCause { get; private set; }
        public event Action Died;
        public void Configure(Phenotype stats)
        {
            Stats = stats; Energy = stats.MaxEnergy; Stamina = stats.MaxStamina; Health = 100; SprintLocked = false;
            DeathCause = CreatureDeathCause.Unknown;
        }
        public bool CanSprint => !Dead && !SprintLocked && Stamina > .01f && Energy > Stats.MaxEnergy * .08f;
        public void Tick(float dt, float speed, bool sprinting)
        {
            if (Dead || dt <= 0 || float.IsNaN(dt) || float.IsInfinity(dt)) return;
            speed = Genome.Safe(speed, 0, 0, 30);
            float cost = Stats.PassiveDrain + Stats.MoveCost * Mathf.Clamp01(speed / Stats.WalkSpeed);
            if (sprinting && speed > .2f)
            { cost += Stats.SprintCost; Stamina -= Stats.StaminaCost * dt; }
            else Stamina += Stats.StaminaRecovery * dt;
            Energy = Mathf.Clamp(Energy - cost * dt, 0, Stats.MaxEnergy);
            Stamina = Mathf.Clamp(Stamina, 0, Stats.MaxStamina);
            if (Stamina <= .01f) SprintLocked = true;
            else if (Stamina >= Stats.MaxStamina * .25f) SprintLocked = false;
            if (Energy <= .001f) Damage(4f * dt, CreatureDeathCause.Starvation);
        }
        public float Eat(float nutrition)
        {
            if (Dead || float.IsNaN(nutrition) || float.IsInfinity(nutrition)) return 0;
            float before = Energy;
            Energy = Mathf.Clamp(Energy + Mathf.Max(0, nutrition) * Stats.NutritionFactor, 0, Stats.MaxEnergy);
            return Energy - before;
        }
        public bool SpendEnergy(float amount)
        {
            if (Dead || float.IsNaN(amount) || float.IsInfinity(amount) || amount < 0 || Energy < amount) return false;
            Energy -= amount; return true;
        }
        public void Damage(float amount, CreatureDeathCause cause = CreatureDeathCause.Unknown)
        {
            if (Dead || float.IsNaN(amount) || float.IsInfinity(amount)) return;
            Health = Mathf.Clamp(Health - Mathf.Max(0, amount), 0, 100);
            if (Dead)
            {
                // Capture the fatal source before listeners run; never infer it from current reserves/age.
                DeathCause = cause == CreatureDeathCause.Starvation || cause == CreatureDeathCause.OldAge ? cause : CreatureDeathCause.Unknown;
                Died?.Invoke();
            }
        }
    }
}
