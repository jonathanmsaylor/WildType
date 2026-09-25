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
        float decision, stuckTime, searchTime, memoryAge, targetTime, rejectedTime;
        Vector3 rememberedFood;
        bool hasMemory;
        FoodPlant rejected;
        public int Relocations { get; private set; }
        public int Meals { get; private set; }
        public Vector3 SearchDestination => wander;
        public FoodPlant Target => target;
        public void Configure(CreatureAgent creature, int seed)
        { actor = creature; random = new System.Random(seed); ChooseWander(); decision = (float)random.NextDouble() * .4f; }
        void ChooseWander(bool searching = false)
        {
            Vector2 direction = new Vector2((float)random.NextDouble() - .5f, (float)random.NextDouble() - .5f).normalized;
            if (searching)
            {
                Relocations++;
                // Remember only our own recent successful meal, not whether an unseen plant has regrown.
                if (hasMemory && memoryAge < 50 && memoryAge > 18 && Vector3.Distance(transform.position, rememberedFood) > actor.Stats.Vision)
                { wander = rememberedFood; hasMemory = false; return; }
                // A nearby visible terrain edge is a search opportunity, not knowledge of remote food.
                float x = transform.position.x;
                if (Mathf.Abs(Mathf.Abs(x) - 35) < actor.Stats.Vision * .75f)
                    direction = new Vector2(x > 35 ? -1 : x < -35 ? 1 : Mathf.Sign(x), direction.y * .6f).normalized;
            }
            wander = transform.position + new Vector3(direction.x, 0, direction.y) * (searching ? 28 + (float)random.NextDouble() * 16 : 8 + (float)random.NextDouble() * 16);
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
            memoryAge += Time.deltaTime; rejectedTime -= Time.deltaTime;
            if (rejectedTime <= 0) rejected = null;
            bool hungry = actor.Vitals.Energy < actor.Stats.MaxEnergy * Mathf.Max(.78f, actor.Stats.ReproductionEnergyFraction + .04f);
            searchTime = hungry && !target ? searchTime + Time.deltaTime : 0;
            targetTime = target ? targetTime + Time.deltaTime : 0;
            if (target && targetTime > 16)
            { rejected = target; rejectedTime = 12; target = null; ChooseWander(true); }
            if (target && !target.Available) target = null;
            if (decision <= 0)
            {
                decision = .35f + (float)random.NextDouble() * .15f;
                if (hungry)
                {
                    var seen = actor.Session.World.Forage(actor, rejected);
                    if (seen != target) targetTime = 0;
                    target = seen;
                }
                else target = null;
                if (target || !generations.enabled || generations.IndividualReason(actor).Length > 0) mate = null;
                if (!target && !mate && generations.enabled && generations.IndividualReason(actor).Length == 0 &&
                    random.NextDouble() < .35f * actor.Stats.ReproductiveMotivation)
                    mate = generations.FindPartner(actor, actor.Stats.Vision, true);
                if (!target && !mate && (Vector3.Distance(transform.position, wander) < 2 || stuckTime > 2 || searchTime > 9))
                { ChooseWander(hungry); stuckTime = 0; searchTime = 0; }
            }
            Vector3 destination = target ? target.transform.position : mate ? mate.transform.position : wander;
            Vector3 direction = destination - transform.position; direction.y = 0;
            if (target && direction.magnitude < actor.Interaction.Reach * .88f)
            {
                actor.DesiredDirection = Vector3.zero; actor.WantsSprint = false;
                if (actor.Interaction.TryEat(target)) { rememberedFood = target.transform.position; memoryAge = 0; hasMemory = true; Meals++; }
                target = null; return;
            }
            if (mate && direction.magnitude < GenerationLoop.MateRange * .8f)
            {
                actor.DesiredDirection = Vector3.zero; actor.WantsSprint = false;
                actor.Intent = mate.IsPlayer ? "Waiting for your mating choice" : "Seeking partner";
                if (!mate.IsPlayer) generations.TryMate(actor, mate, out _);
                return;
            }
            direction.Normalize();
            // Local feeler steering supplements wandering; no navigation package or global scan.
            float radius = .46f * actor.Stats.Size * actor.Life.Growth;
            Vector3 origin = transform.position + Vector3.up * Mathf.Max(.8f, radius);
            if (Physics.SphereCast(origin, radius, direction, out var hit, 2.7f, Ecosystem.ObstacleMask))
            {
                Vector3 tangent = Vector3.Cross(Vector3.up, hit.normal).normalized;
                if (Vector3.Dot(tangent, direction) < 0) tangent = -tangent;
                direction = (tangent + hit.normal * .55f).normalized;
            }
            actor.DesiredDirection = direction;
            actor.WantsSprint = target && actor.Vitals.Energy < actor.Stats.MaxEnergy * .2f && actor.Vitals.Stamina > actor.Stats.MaxStamina * .5f && Vector3.Distance(transform.position, destination) > 7;
            actor.Intent = target ? "Seeking " + target.DisplayName.ToLowerInvariant() : mate ? "Seeking partner" : hungry ? "Searching new forage" : "Wandering";
            stuckTime = actor.Motor.Speed < .1f ? stuckTime + Time.deltaTime : 0;
        }
    }
}
