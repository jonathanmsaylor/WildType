using UnityEngine;
namespace WildType
{
    public sealed class HerbivoreBrain : MonoBehaviour
    {
        CreatureAgent actor;
        System.Random random;
        FoodPlant target;
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
            if (!actor || actor.Session.Paused || actor.Vitals.Dead) return;
            decision -= Time.deltaTime;
            if (target && !target.Available) target = null;
            if (decision <= 0)
            {
                decision = .35f + (float)random.NextDouble() * .15f;
                if (actor.Vitals.Energy < actor.Stats.MaxEnergy * .78f)
                    target = actor.Session.World.NearestFood(transform.position, actor.Stats.Vision);
                else target = null;
                if (!target && (Vector3.Distance(transform.position, wander) < 2 || stuckTime > 2)) { ChooseWander(); stuckTime = 0; }
            }
            Vector3 destination = target ? target.transform.position : wander;
            Vector3 direction = destination - transform.position; direction.y = 0;
            if (target && direction.magnitude < actor.Interaction.Reach * .88f)
            { actor.DesiredDirection = Vector3.zero; actor.WantsSprint = false; actor.Interaction.TryEat(target); target = null; return; }
            direction.Normalize();
            // Local feeler steering supplements wandering; no navigation package or global scan.
            if (Physics.SphereCast(transform.position + Vector3.up * .8f, .5f, direction, out var hit, 2.7f, Ecosystem.ObstacleMask))
                direction = Vector3.Cross(Vector3.up, hit.normal).normalized;
            actor.DesiredDirection = direction;
            actor.WantsSprint = target && actor.Vitals.Energy < actor.Stats.MaxEnergy * .2f && actor.Vitals.Stamina > actor.Stats.MaxStamina * .5f && Vector3.Distance(transform.position, destination) > 7;
            actor.Intent = target ? "Seeking brightfruit" : "Wandering";
            stuckTime = actor.Motor.Speed < .1f ? stuckTime + Time.deltaTime : 0;
        }
    }
}
