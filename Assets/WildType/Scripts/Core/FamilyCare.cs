using UnityEngine;
namespace WildType
{
    // One bounded session coordinator. Timers/counters live on the existing creature life;
    // no lingering target jobs, resource sources or unbounded care history.
    public sealed class FamilyCare : MonoBehaviour
    {
        StageSession session;
        public int Shares { get; private set; }
        public void ResetRun(StageSession value) { session = value; Shares = 0; }
        bool Living(CreatureAgent a) => a && a.Session == session && a.Life && !a.Vitals.Dead &&
            session.Generations.Owns(a) && session.Generations.Archive.Get(a.Life.Id).Alive;
        public bool IsChild(CreatureAgent parent, CreatureAgent child) => session && Living(parent) && Living(child) &&
            FamilyCareRules.DirectChild(parent.Life.Id, session.Generations.Archive.Get(child.Life.Id));
        public bool Visible(CreatureAgent parent, CreatureAgent child) => !Physics.Linecast(
            parent.transform.position + Vector3.up * parent.CurrentHeight * .7f,
            child.transform.position + Vector3.up * child.CurrentHeight * .7f, Ecosystem.WorldMask, QueryTriggerInteraction.Ignore);
        public CreatureAgent NearbyChild(CreatureAgent parent)
        {
            if (!Living(parent)) return null;
            var pin = parent == session.Player ? session.Locator.Target : null;
            if (pin && IsChild(parent, pin) && !pin.Life.Adult && Vector3.Distance(parent.transform.position, pin.transform.position) <= FamilyCareRules.Range) return pin;
            CreatureAgent nearest = null; float best = FamilyCareRules.Range * FamilyCareRules.Range;
            foreach (var child in session.Creatures)
            {
                if (!IsChild(parent, child) || child.Life.Adult || !Visible(parent, child)) continue;
                float distance = (parent.transform.position - child.transform.position).sqrMagnitude;
                if (distance <= best) { nearest = child; best = distance; }
            }
            return nearest;
        }
        public string Reason(CreatureAgent parent, CreatureAgent child)
        {
            if (!session || !session.Ready || session.Paused || !session.Generations.enabled) return "Care unavailable while paused";
            if (!IsChild(parent, child)) return "Approach your living juvenile child to share";
            if (!parent.Life.Adult || child.Life.Adult) return "Only adult parents can feed juvenile children";
            if (parent.Life.Age >= parent.Stats.LifespanSeconds || child.Life.Age >= child.Stats.LifespanSeconds) return "Creature at end of lifespan";
            if (!CreatureMotor.Finite(parent.transform.position) || !CreatureMotor.Finite(child.transform.position) ||
                Vector3.Distance(parent.transform.position, child.transform.position) > FamilyCareRules.Range) return "Move within 3.5 m of your child";
            if (!Visible(parent, child)) return "Move around the obstacle to your child";
            if (session.Generations.Reserved(parent) || session.Generations.Reserved(child)) return "Finish courtship before sharing";
            if (parent.Life.CareCooldown > 0) return $"Share again in {Mathf.CeilToInt(parent.Life.CareCooldown)}s";
            if (child.Stats.MaxEnergy - child.Vitals.Energy < 1) return "Your child's energy is full";
            if (!FamilyCareRules.Transfer(parent.Vitals.Energy, parent.Stats.MaxEnergy, child.Vitals.Energy, child.Stats.MaxEnergy, out _, out _))
                return "Eat first — keep 25% of your own energy";
            return "";
        }
        public bool TryShare(CreatureAgent parent, CreatureAgent child, out string response)
        {
            response = Reason(parent, child); if (response.Length > 0) return false;
            if (!FamilyCareRules.Transfer(parent.Vitals.Energy, parent.Stats.MaxEnergy, child.Vitals.Energy, child.Stats.MaxEnergy, out float cost, out float gain) ||
                !parent.Vitals.TransferEnergy(child.Vitals, cost, gain)) { response = "Care target or resources changed"; return false; }
            // Instant, single-threaded transfer after validation: death/movement cannot interleave.
            parent.Life.RecordCare(); Shares++;
            parent.Visual.Feed(); child.Visual.Feed();
            response = $"Shared with {GenerationLoop.ShortId(child.Life.Id)}: you −{cost:0.0} energy · child +{gain:0.0}";
            if (parent.IsPlayer || child.IsPlayer) session.ShowNotice(response, 4);
            return true;
        }
        public bool TryPlayerShare()
        {
            bool result = TryShare(session.Player, NearbyChild(session.Player), out string response);
            if (!result) session.ShowNotice(response, 3); return result;
        }
        public bool TryAutonomous(CreatureAgent parent)
        {
            if (!Living(parent) || parent.IsPlayer || parent.Vitals.Energy < parent.Stats.MaxEnergy * .65f || parent.Life.CareCooldown > 0) return false;
            var child = NearbyChild(parent);
            return child && child.Vitals.Energy < child.Stats.MaxEnergy * .5f && TryShare(parent, child, out _);
        }
    }
}
