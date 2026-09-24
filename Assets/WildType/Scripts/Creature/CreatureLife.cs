using UnityEngine;
namespace WildType
{
    // Simulation identity and life history stay outside the replaceable VisualRoot.
    public sealed class CreatureLife : MonoBehaviour
    {
        CreatureAgent actor;
        double birthTime, readyAt;
        float deathAge, appliedScale = -1;
        public CreatureId Id { get; private set; }
        public float Age => actor.Vitals.Dead ? deathAge : (float)System.Math.Max(0, actor.Session.Generations.Clock - birthTime);
        public bool Adult => Age >= actor.Stats.MaturityAge;
        public float Growth => Mathf.Lerp(.48f, 1, Mathf.Clamp01(Age / actor.Stats.MaturityAge));
        public float Cooldown => Mathf.Max(0, (float)(readyAt - actor.Session.Generations.Clock));
        public void Configure(CreatureAgent creature, CreatureLineageRecord record)
        { actor = creature; Id = record.CreatureId; birthTime = record.BirthTime; ApplyGrowth(); }
        public void BeginCooldown() { readyAt = actor.Session.Generations.Clock + actor.Stats.ReproductionCooldown; }
        public void FreezeDeathAge() { deathAge = (float)System.Math.Max(0, actor.Session.Generations.Clock - birthTime); }
        public void Tick()
        {
            if (actor.Vitals.Dead) return;
            ApplyGrowth();
            if (Age >= actor.Stats.LifespanSeconds) actor.Vitals.Damage(100, CreatureDeathCause.OldAge);
        }
        void ApplyGrowth()
        {
            float scale = Growth;
            if (Mathf.Abs(scale - appliedScale) < .005f && !(scale == 1 && appliedScale < 1)) return;
            appliedScale = scale;
            actor.Motor.SetGrowth(scale);
            if (actor.Visual) actor.Visual.transform.localScale = Vector3.one * scale;
        }
        public ReproductionCandidate Candidate(bool reserved = false) => new ReproductionCandidate
        {
            creatureId = Id, alive = !actor.Vitals.Dead, age = Age, maturityAge = actor.Stats.MaturityAge,
            health = actor.Vitals.Health, energy = actor.Vitals.Energy, maximumEnergy = actor.Stats.MaxEnergy,
            reproductionEnergyFraction = actor.Stats.ReproductionEnergyFraction, cooldownRemaining = Cooldown, reserved = reserved
        };
    }
}
