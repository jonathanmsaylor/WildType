using UnityEngine;
namespace WildType
{
    public sealed class OrbitCamera : MonoBehaviour
    {
        CreatureAgent target;
        StageSession session;
        float yaw, pitch = 22, desiredDistance = 8, distance = 8, bob;
        Vector3 focus;
        public float DesiredDistance => desiredDistance;
        public float ActualDistance { get; private set; }
        public float Yaw => yaw;
        public void Configure(CreatureAgent actor, StageSession stage)
        {
            target = actor; session = stage; yaw = 0; pitch = 22;
            desiredDistance = distance = Mathf.Max(7, actor.Stats.Size * 7);
            focus = actor.transform.position + Vector3.up * actor.CurrentHeight * .72f;
            LateUpdate();
        }
        public void Orbit(Vector2 look, float zoom)
        {
            if (!target) return;
            yaw = Mathf.Repeat(yaw + look.x, 360);
            pitch = Mathf.Clamp(pitch - look.y, 8, 66);
            desiredDistance = Mathf.Clamp(desiredDistance - zoom * .8f, Mathf.Max(3.6f, target.Stats.Size * 4), 19);
        }
        void LateUpdate()
        {
            if (!target || session.Paused) return;
            float dt = Time.deltaTime;
            bob += target.Motor.Speed * dt * 1.8f;
            Vector3 aim = target.transform.position + Vector3.up * (target.CurrentHeight * .72f + Mathf.Sin(bob) * .018f * Mathf.Clamp01(target.Motor.Speed));
            focus = Vector3.Lerp(focus, aim, 1 - Mathf.Exp(-12 * dt));
            distance = Mathf.Lerp(distance, desiredDistance, 1 - Mathf.Exp(-10 * dt));
            Quaternion orientation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 backward = orientation * Vector3.back;
            float safeDistance = distance;
            if (Physics.SphereCast(focus, .28f, backward, out var hit, distance, Ecosystem.WorldMask, QueryTriggerInteraction.Ignore))
                safeDistance = Mathf.Max(.35f, hit.distance - .2f);
            ActualDistance = safeDistance;
            transform.SetPositionAndRotation(focus + backward * safeDistance, orientation);
        }
    }
}
