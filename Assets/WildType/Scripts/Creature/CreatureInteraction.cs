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
        public bool TryEat(FoodPlant target = null)
        {
            if (!actor || actor.Session.Paused || actor.Vitals.Dead || actor.Vitals.Energy >= actor.Stats.MaxEnergy - .5f) return false;
            if (!target) target = Nearest();
            if (!target || !target.Available || Vector3.Distance(actor.transform.position, target.transform.position) > Reach) return false;
            float nutrition = target.Consume();
            if (nutrition <= 0) return false;
            float gain = actor.Vitals.Eat(nutrition);
            actor.Visual.Feed(); actor.Session.Fx.Burst(point.position, new Color(.7f, 1, .25f), 16, 1.1f);
            if (actor.IsPlayer) actor.Session.ShowNotice("+" + Mathf.RoundToInt(gain) + " energy", 2);
            return true;
        }
    }
}
