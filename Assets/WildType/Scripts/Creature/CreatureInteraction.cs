using UnityEngine;
namespace WildType
{
    public sealed class CreatureInteraction : MonoBehaviour
    {
        CreatureAgent actor;
        public Transform point;
        public float Reach => 1.7f + actor.Stats.Size * .65f;
        public void Configure(CreatureAgent creature) { actor = creature; }
        public FoodPlant Nearest() => actor.Session.World.NearestFood(actor.transform.position, Reach);
        public int MealsEaten { get; private set; }
        public string EatReason(FoodPlant target = null)
        {
            if (!actor || actor.Vitals.Dead) return "This creature cannot eat — its life has ended.";
            if (actor.Session.Paused) return "Resume play before eating.";
            if (actor.Vitals.Energy >= actor.Stats.MaxEnergy - .5f) return "Energy is full — save the fruit for later.";
            if (!target) target = Nearest();
            if (!target) return "No ripe food in reach — walk closer to fruit, or try another patch.";
            if (!target.Available) return "This plant is regrowing — look for ripe fruit nearby.";
            if (Vector3.Distance(actor.transform.position, target.transform.position) > Reach) return "Food is too far away — walk closer.";
            return "";
        }
        public bool TryEat(FoodPlant target = null)
        {
            string reason = EatReason(target);
            if (reason.Length > 0) { if (actor && actor.IsPlayer && !actor.Session.Paused) actor.Session.ShowNotice(reason, 5); return false; }
            if (!target) target = Nearest();
            if (!target || !target.Available || Vector3.Distance(actor.transform.position, target.transform.position) > Reach) return false;
            float nutrition = target.Consume();
            if (nutrition <= 0) return false;
            float gain = actor.Vitals.Eat(nutrition);
            MealsEaten++;
            actor.Visual.Feed(); actor.Session.Fx.Burst(point.position, new Color(.7f, 1, .25f), 16, 1.1f);
            if (actor.IsPlayer) actor.Session.ShowNotice($"Ate {target.DisplayName}: +{gain:0.0} energy. The plant will regrow.", 4);
            return true;
        }
    }
}
