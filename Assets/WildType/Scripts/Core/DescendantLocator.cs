using UnityEngine;
namespace WildType
{
    // Presentation selection, not a navigation command or an extension of care eligibility.
    public sealed class DescendantLocator : MonoBehaviour
    {
        public const float Duration = 4;
        StageSession session;
        CreatureAgent target, owner;
        public float Remaining { get; private set; }
        public CreatureAgent Target
        {
            get
            {
                if (!session || !session.Ready || !owner || owner != session.Player || owner.Vitals.Dead ||
                    !target || target.Vitals.Dead || !session.Generations.Owns(target) || Remaining <= 0) Clear();
                return target;
            }
        }
        public float Elapsed => Duration - Remaining;
        public void ResetRun(StageSession value) { session = value; Clear(); }
        public void Clear() { target = owner = null; Remaining = 0; }
        public bool Select(CreatureAgent descendant)
        {
            if (!session || !session.Ready || session.GameOver || !session.Player || session.Player.Vitals.Dead ||
                session.Names.HasPrompt || !session.CanLocateFamily(descendant)) return false;
            // Ancestry is immutable during a run. Validate once here, membership/liveness on every read.
            target = descendant; owner = session.Player; Remaining = Duration;
            session.SetPaused(false);
            session.ShowNotice("Locating " + session.Names.PersonalName(target.Life.Id), Duration);
            return true; // Selecting the same target restarts the pulse, never toggles it off.
        }
        void Update()
        {
            if (!Target || session.Paused) return;
            Remaining = Mathf.Max(0, Remaining - Time.deltaTime);
            if (Remaining == 0) Clear();
        }
        public static float Pulse(float elapsed)
        {
            if (float.IsNaN(elapsed) || float.IsInfinity(elapsed) || elapsed <= 0 || elapsed >= Duration) return 0;
            float fade = Mathf.Clamp01(elapsed / .25f) * Mathf.Clamp01((Duration - elapsed) / .6f);
            float wave = Mathf.Sin(elapsed * Mathf.PI / 1.2f);
            return .38f * wave * wave * fade;
        }
    }
}
