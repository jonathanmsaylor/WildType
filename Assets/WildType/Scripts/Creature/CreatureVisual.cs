using System.Collections.Generic;
using UnityEngine;
namespace WildType
{
    // Presentation only: replacing this VisualRoot never removes simulation components.
    public sealed class CreatureVisual : MonoBehaviour
    {
        CreatureAgent actor;
        Transform torso, head;
        readonly List<Transform> hips = new List<Transform>(4);
        readonly List<Transform> knees = new List<Transform>(4);
        readonly List<Transform> feet = new List<Transform>(4);
        readonly List<Transform> tail = new List<Transform>(3);
        float phase, clock, feed, dust;
        float bodyHeight;
        public void Build(CreatureAgent creature)
        {
            actor = creature; var g = actor.Genome;
            float size = g.bodySize, leg = g.legLength * size;
            bodyHeight = leg + size * .32f;
            Color primary = g.camouflage;
            Color accent = Color.Lerp(primary, new Color(.96f, .72f, .35f), .65f);
            torso = Part("Torso", transform, new Vector3(0, bodyHeight, 0), new Vector3(1.25f, 1, 2.05f) * size, primary);
            Part("Chest", torso, new Vector3(0, .04f, .25f), new Vector3(.96f, 1.02f, .76f), primary);
            head = Part("Head", transform, new Vector3(0, bodyHeight + .36f * size, .98f * size), Vector3.one * .93f * size, primary);
            Part("Muzzle", head, new Vector3(0, -.18f, .44f), new Vector3(.73f, .47f, .64f), accent);
            for (int sign = -1; sign <= 1; sign += 2)
            {
                Part("Eye", head, new Vector3(sign * .39f, .12f, .3f), new Vector3(.19f, .24f, .18f), new Color(.035f, .06f, .07f));
                Part("Glint", head, new Vector3(sign * .405f, .18f, .365f), Vector3.one * .055f, Color.white);
                var ear = Part("Ear", head, new Vector3(sign * .35f, .48f, -.1f), new Vector3(.24f, .48f, .22f), accent);
                ear.localRotation = Quaternion.Euler(0, 0, sign * -18);
            }
            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0 ? -1 : 1) * .48f * size;
                float z = (i < 2 ? .59f : -.66f) * size;
                var hip = new GameObject("LegJoint").transform; hip.SetParent(transform, false);
                hip.localPosition = new Vector3(x, leg, z); hips.Add(hip);
                Part("Shoulder", hip, Vector3.up * .03f, Vector3.one * .4f * size, primary);
                Part("UpperLeg", hip, Vector3.down * leg * .25f, new Vector3(.22f * size, leg * .54f, .25f * size), primary);
                var knee = new GameObject("KneeJoint").transform; knee.SetParent(hip, false); knee.localPosition = Vector3.down * leg * .48f; knees.Add(knee);
                Part("Knee", knee, Vector3.zero, Vector3.one * .25f * size, accent);
                Part("LowerLeg", knee, Vector3.down * leg * .23f, new Vector3(.18f * size, leg * .5f, .2f * size), primary);
                feet.Add(Part("Foot", knee, new Vector3(0, -leg * .46f, .1f * size), new Vector3(.36f, .17f, .52f) * size, Color.Lerp(primary, Color.black, .35f)));
            }
            Transform parent = transform;
            for (int i = 0; i < 3; i++)
            {
                var joint = new GameObject("TailJoint").transform; joint.SetParent(parent, false);
                joint.localPosition = i == 0 ? new Vector3(0, bodyHeight + .12f * size, -.88f * size) : new Vector3(0, 0, -.53f * size);
                Part("Tail", joint, new Vector3(0, 0, -.26f * size), new Vector3(.32f - i * .08f, .3f - i * .07f, .66f) * size, i == 2 ? accent : primary);
                tail.Add(joint); parent = joint;
            }
            for (int i = 0; i < 4; i++)
                Part("DorsalMark", transform, new Vector3(0, bodyHeight + .49f * size, (-.6f + i * .35f) * size), new Vector3(.31f, .05f, .19f) * size, accent);
        }
        Transform Part(string label, Transform parent, Vector3 position, Vector3 scale, Color color)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere); obj.name = label;
            Destroy(obj.GetComponent<Collider>()); obj.layer = 10;
            obj.transform.SetParent(parent, false); obj.transform.localPosition = position; obj.transform.localScale = scale;
            var renderer = obj.GetComponent<Renderer>(); renderer.sharedMaterial = actor.Session.prototypeMaterial;
            var block = new MaterialPropertyBlock(); block.SetColor("_BaseColor", color); renderer.SetPropertyBlock(block);
            return obj.transform;
        }
        public void Feed() { feed = .7f; }
        void LateUpdate()
        {
            if (!actor || actor.Session.Paused) return;
            float dt = Time.deltaTime; clock += dt; feed = Mathf.Max(0, feed - dt);
            float speed = actor.Motor.Speed;
            phase += speed / Mathf.Max(.5f, actor.Genome.legLength * actor.Stats.Size * 1.8f) * Mathf.PI * 2 * dt;
            float amount = Mathf.Clamp01(speed / actor.Stats.SprintSpeed);
            var lean = Quaternion.FromToRotation(Vector3.up, transform.parent.InverseTransformDirection(actor.Motor.GroundNormal));
            transform.localRotation = Quaternion.Slerp(transform.localRotation, actor.Vitals.Dead ? Quaternion.Euler(0, 0, 75) : lean, 1 - Mathf.Exp(-5 * dt));
            torso.localPosition = new Vector3(0, bodyHeight + (Mathf.Sin(clock * 1.7f) * .018f + Mathf.Sin(phase * 2) * .025f * amount) * actor.Stats.Size, 0);
            head.localRotation = Quaternion.Euler(feed > 0 ? Mathf.Sin(feed * 18) * 13 : Mathf.Sin(clock * 1.2f) * 2, Mathf.Sin(clock * .7f) * 3, 0);
            for (int i = 0; i < hips.Count; i++)
            {
                float step = Mathf.Sin(phase + (i == 0 || i == 3 ? 0 : Mathf.PI));
                hips[i].localRotation = Quaternion.Euler(step * 28 * amount, 0, 0);
                knees[i].localRotation = Quaternion.Euler(Mathf.Max(0, -step) * 42 * amount, 0, 0);
                feet[i].localRotation = Quaternion.Euler(-step * 12 * amount, 0, 0);
            }
            for (int i = 0; i < tail.Count; i++) tail[i].localRotation = Quaternion.Euler(-5, Mathf.Sin(clock * 2.2f - i * .7f) * (5 + amount * 8), 0);
            dust -= dt;
            if (!actor.Vitals.Dead && actor.Motor.Grounded && speed > 1 && dust <= 0)
            { dust = .28f; actor.Session.Fx.Burst(transform.position + Vector3.up * .1f, new Color(.7f, .6f, .4f, .45f), 2, .3f); }
        }
    }
}
