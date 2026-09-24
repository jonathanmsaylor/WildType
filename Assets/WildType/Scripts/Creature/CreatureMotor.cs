using UnityEngine;
namespace WildType
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class CreatureMotor : MonoBehaviour
    {
        CharacterController controller;
        Phenotype stats;
        Vector3 horizontal;
        float vertical;
        float growth = 1;
        public float Speed { get; private set; }
        public bool Sprinting { get; private set; }
        public bool Grounded => controller && controller.isGrounded;
        public Vector3 GroundNormal { get; private set; } = Vector3.up;
        public void Configure(Phenotype value)
        {
            stats = value; controller = GetComponent<CharacterController>();
            controller.radius = .46f * stats.Size; controller.height = Mathf.Max(stats.Height, controller.radius * 2.1f);
            controller.center = Vector3.up * controller.height * .5f;
            controller.stepOffset = .32f * stats.Size; controller.slopeLimit = 48; controller.skinWidth = .045f;
        }
        public void SetGrowth(float scale)
        {
            growth = Mathf.Clamp(scale, .48f, 1);
            controller.radius = .46f * stats.Size * growth;
            controller.height = Mathf.Max(stats.Height * growth, controller.radius * 2.1f);
            controller.center = Vector3.up * controller.height * .5f;
            controller.stepOffset = .32f * stats.Size * growth;
        }
        public void Tick(Vector3 direction, bool sprint, CreatureVitals vitals, float dt)
        {
            if (!controller || !controller.enabled) return;
            direction.y = 0; direction = Vector3.ClampMagnitude(direction, 1);
            Sprinting = sprint && vitals.CanSprint && direction.sqrMagnitude > .01f;
            float limit = (Sprinting ? stats.SprintSpeed : stats.WalkSpeed) * Mathf.Sqrt(growth);
            if (vitals.Energy < stats.MaxEnergy * .15f) limit *= Mathf.Lerp(.4f, 1, vitals.Energy / (stats.MaxEnergy * .15f));
            if (vitals.Dead) { direction = Vector3.zero; limit = 0; Sprinting = false; }
            horizontal = Vector3.MoveTowards(horizontal, direction * limit, stats.Acceleration * (direction == Vector3.zero ? 1.5f : 1) * dt);
            vertical = Grounded ? -3f : Mathf.Max(-30, vertical - 22 * dt);
            Vector3 previous = transform.position;
            controller.Move((horizontal + Vector3.up * vertical) * dt);
            Vector3 planar = transform.position; planar.y = 0;
            if (planar.magnitude > Ecosystem.PlayRadius)
            {
                Vector3 boundary = planar.normalized * Ecosystem.PlayRadius;
                controller.Move(new Vector3(boundary.x - transform.position.x, 0, boundary.z - transform.position.z));
                horizontal = Vector3.ProjectOnPlane(horizontal, planar.normalized);
            }
            Vector3 travelled = transform.position - previous; travelled.y = 0;
            Speed = travelled.magnitude / Mathf.Max(dt, .0001f);
            if (direction.sqrMagnitude > .01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 1 - Mathf.Exp(-stats.TurnRate * dt));
            if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out var hit, 3, Ecosystem.GroundMask))
                GroundNormal = hit.normal;
            if (!Finite(transform.position) || transform.position.y < -20) Teleport(new Vector3(0, Ecosystem.Height(0, 0) + 2, 0));
        }
        public void Teleport(Vector3 position)
        {
            if (!controller) controller = GetComponent<CharacterController>();
            controller.enabled = false; transform.position = position; controller.enabled = true;
            horizontal = Vector3.zero; vertical = 0; Speed = 0;
        }
        public static bool Finite(Vector3 v) => !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) || float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z));
    }
}
