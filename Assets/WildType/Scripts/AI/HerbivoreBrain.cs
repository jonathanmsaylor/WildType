using UnityEngine;
namespace WildType
{
    public sealed class HerbivoreBrain : MonoBehaviour
    {
        CreatureAgent actor;
        System.Random random;
        FoodPlant target;
        CreatureAgent mate;
        public CreatureAgent MateTarget => mate;
        Vector3 wander;
        float decision, stuckTime;
        public FoodPlant Target => target;
        public void Configure(CreatureAgent creature, int seed)
        { actor = creature; random = new System.Random(seed); ChooseWander(); decision = (float)random.NextDouble() * .4f; }
        void ChooseWander()
        {
            Vector2 direction = new Vector2((float)random.NextDouble() - .5f, (float)random.NextDouble() - .5f).normalized;
            wander = transform.position + new Vector3(direction.x, 0, direction.y) * (8 + (float)random.NextDouble() * 16);
            Vector2 flat = Vector2.ClampMagnitude(new Vector2(wander.x, wander.z), 106); wander = new Vector3(flat.x, Ecosystem.Height(flat.x, flat.y), flat.y);
        }
        void Update()
        {
            if (!actor || actor.IsPlayer || actor.Session.Paused || actor.Vitals.Dead) return;
            var generations = actor.Session.Generations;
            if (generations.enabled && generations.Reserved(actor))
            { actor.DesiredDirection = Vector3.zero; actor.WantsSprint = false; actor.Intent = "Courting"; return; }
            if (mate && (mate.Vitals.Dead || generations.IndividualReason(mate).Length > 0 ||
                actor.Session.Population + generations.PendingBirths >= GenerationLoop.PopulationCap)) mate = null;
            decision -= Time.deltaTime;
            if (target && !target.Available) target = null;
            if (decision <= 0)
            {
                decision = .35f + (float)random.NextDouble() * .15f;
                if (actor.Vitals.Energy < actor.Stats.MaxEnergy * Mathf.Max(.78f, actor.Stats.ReproductionEnergyFraction + .04f))
                    target = actor.Session.World.NearestFood(transform.position, actor.Stats.Vision);
                else target = null;
                if (target || !generations.enabled || generations.IndividualReason(actor).Length > 0) mate = null;
                if (!target && !mate && generations.enabled && generations.IndividualReason(actor).Length == 0 &&
                    random.NextDouble() < .35f * actor.Stats.ReproductiveMotivation)
                    mate = generations.FindPartner(actor, actor.Stats.Vision, true);
                if (!target && (Vector3.Distance(transform.position, wander) < 2 || stuckTime > 2)) { ChooseWander(); stuckTime = 0; }
            }
            Vector3 destination = target ? target.transform.position : mate ? mate.transform.position : wander;
            Vector3 direction = destination - transform.position; direction.y = 0;
            if (target && direction.magnitude < actor.Interaction.Reach * .88f)
            { actor.DesiredDirection = Vector3.zero; actor.WantsSprint = false; actor.Interaction.TryEat(target); target = null; return; }
            if (mate && direction.magnitude < GenerationLoop.MateRange * .8f)
            {
                actor.DesiredDirection = Vector3.zero; actor.WantsSprint = false;
                actor.Intent = mate.IsPlayer ? "Waiting for your mating choice" : "Seeking partner";
                if (!mate.IsPlayer) generations.TryMate(actor, mate, out _);
                return;
            }
            direction.Normalize();
            // Local feeler steering supplements wandering; no navigation package or global scan.
            if (Physics.SphereCast(transform.position + Vector3.up * .8f, .5f, direction, out var hit, 2.7f, Ecosystem.ObstacleMask))
                direction = Vector3.Cross(Vector3.up, hit.normal).normalized;
            actor.DesiredDirection = direction;
            actor.WantsSprint = target && actor.Vitals.Energy < actor.Stats.MaxEnergy * .2f && actor.Vitals.Stamina > actor.Stats.MaxStamina * .5f && Vector3.Distance(transform.position, destination) > 7;
            actor.Intent = target ? "Seeking brightfruit" : mate ? "Seeking partner" : "Wandering";
            stuckTime = actor.Motor.Speed < .1f ? stuckTime + Time.deltaTime : 0;
        }
    }
}
