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
        Mesh markingsMesh;
        MaterialPropertyBlock colors;
        public CreatureAppearance Appearance { get; private set; }
        public Mesh MarkingsMesh => markingsMesh;
        public float Feeding => feed;
        public void Build(CreatureAgent creature)
        {
            colors = new MaterialPropertyBlock();
            actor = creature; Appearance = new CreatureAppearance(actor.Genome);
            var shape = Appearance;
            float size = shape.Size, leg = shape.LegLength;
            bodyHeight = shape.BodyHeight;
            Color primary = shape.Coat, accent = shape.Accent;
            torso = Part("Torso", transform, new Vector3(0, bodyHeight, 0), shape.BodyScale, primary);
            // Chest remains beneath the coat surface; all bands follow torso breathing instead of floating above it.
            Part("Chest", torso, new Vector3(0, -.06f, .18f), new Vector3(.91f, .9f, .68f), primary);
            head = Part("Head", transform, new Vector3(0, bodyHeight + .30f * size, shape.BodyLength * .48f), new Vector3(.85f, .9f, .98f) * size, primary);
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
                float x = (i % 2 == 0 ? -1 : 1) * shape.BodyWidth * .36f;
                float z = (i < 2 ? .29f : -.32f) * shape.BodyLength;
                var hip = new GameObject("LegJoint").transform; hip.SetParent(transform, false);
                hip.localPosition = new Vector3(x, leg, z); hips.Add(hip);
                Part("Shoulder", hip, Vector3.up * .03f, Vector3.one * .4f * size, primary);
                Part("UpperLeg", hip, Vector3.down * leg * .25f, new Vector3(shape.LegThickness, leg * .54f, shape.LegThickness * 1.12f), primary);
                var knee = new GameObject("KneeJoint").transform; knee.SetParent(hip, false); knee.localPosition = Vector3.down * leg * .48f; knees.Add(knee);
                Part("Knee", knee, Vector3.zero, Vector3.one * .25f * size, accent);
                Part("LowerLeg", knee, Vector3.down * leg * .23f, new Vector3(shape.LegThickness * .8f, leg * .5f, shape.LegThickness * .9f), primary);
                feet.Add(Part("Foot", knee, new Vector3(0, -leg * .46f, .1f * size), new Vector3(.36f, .17f, .52f) * size, Color.Lerp(primary, Color.black, .35f)));
            }
            Transform parent = transform;
            for (int i = 0; i < 3; i++)
            {
                var joint = new GameObject("TailJoint").transform; joint.SetParent(parent, false);
                joint.localPosition = i == 0 ? new Vector3(0, bodyHeight + .12f * size, -shape.BodyLength * .44f) : new Vector3(0, 0, -.53f * size);
                Part("Tail", joint, new Vector3(0, 0, -.26f * size), new Vector3(.32f - i * .08f, .3f - i * .07f, .66f) * size, i == 2 ? accent : primary);
                tail.Add(joint); parent = joint;
            }
            BuildBands();
        }
        void BuildBands()
        {
            // One small mesh per lifetime, laid on the ellipsoid and scaled with its parent torso.
            // Constant topology + continuous gene-derived width keeps ordinary inheritance legible.
            const int rows = 4, columns = 24, bands = 3;
            var vertices = new Vector3[bands * (rows + 1) * (columns + 1)];
            var indices = new int[bands * rows * columns * 6]; int index = 0;
            for (int band = 0; band < bands; band++)
                for (int row = 0; row <= rows; row++)
                    for (int column = 0; column <= columns; column++)
                    {
                        float z = (band - 1) * .28f + (row / (float)rows - .5f) * Appearance.BandWidth;
                        float angle = Mathf.Lerp(-115, 115, column / (float)columns) * Mathf.Deg2Rad;
                        float radius = Mathf.Sqrt(.505f * .505f - z * z);
                        int v = band * (rows + 1) * (columns + 1) + row * (columns + 1) + column;
                        vertices[v] = new Vector3(Mathf.Sin(angle) * radius, Mathf.Cos(angle) * radius, z);
                        if (row == rows || column == columns) continue;
                        // Outward winding: across the back (+theta) then toward the head (+z).
                        indices[index++] = v; indices[index++] = v + columns + 1; indices[index++] = v + 1;
                        indices[index++] = v + 1; indices[index++] = v + columns + 1; indices[index++] = v + columns + 2;
                    }
            markingsMesh = new Mesh { name = "Inherited coat bands" };
            markingsMesh.vertices = vertices; markingsMesh.triangles = indices;
            markingsMesh.RecalculateNormals(); markingsMesh.RecalculateBounds();
            var obj = new GameObject("Coat bands", typeof(MeshFilter), typeof(MeshRenderer)); obj.layer = 10;
            obj.transform.SetParent(torso, false); obj.GetComponent<MeshFilter>().sharedMesh = markingsMesh;
            var renderer = obj.GetComponent<MeshRenderer>(); renderer.sharedMaterial = actor.Session.prototypeMaterial;
            colors.SetColor("_BaseColor", Appearance.Mark); renderer.SetPropertyBlock(colors);
        }
        Transform Part(string label, Transform parent, Vector3 position, Vector3 scale, Color color)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere); obj.name = label;
            Destroy(obj.GetComponent<Collider>()); obj.layer = 10;
            obj.transform.SetParent(parent, false); obj.transform.localPosition = position; obj.transform.localScale = scale;
            var renderer = obj.GetComponent<Renderer>(); renderer.sharedMaterial = actor.Session.prototypeMaterial;
            colors.SetColor("_BaseColor", color); renderer.SetPropertyBlock(colors);
            return obj.transform;
        }
        public void Feed() { feed = .7f; }
        void LateUpdate()
        {
            if (!actor || actor.Session.Paused) return;
            float dt = Time.deltaTime; clock += dt; feed = Mathf.Max(0, feed - dt);
            float speed = actor.Motor.Speed;
            phase += speed / Mathf.Max(.5f, Appearance.LegLength * transform.localScale.x * 1.8f) * Mathf.PI * 2 * dt;
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
        void OnDestroy() { if (markingsMesh) Destroy(markingsMesh); }
    }
}
