using UnityEngine;
using UnityEngine.UI;
namespace WildType
{
    // Procedural UI geometry: no font glyph dependency, texture, mesh or material instances.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class CourtshipHeart : MaskableGraphic
    {
        const int Segments = 40;
        public CourtshipHeart() { useLegacyMeshGeneration = false; }
        static readonly Vector2[] Outline = BuildOutline();
        static Vector2[] BuildOutline()
        {
            var result = new Vector2[Segments];
            for (int i = 0; i < Segments; i++)
            {
                float t = i * Mathf.PI * 2 / Segments;
                result[i] = new Vector2(16 * Mathf.Pow(Mathf.Sin(t), 3),
                    13 * Mathf.Cos(t) - 5 * Mathf.Cos(2 * t) - 2 * Mathf.Cos(3 * t) - Mathf.Cos(4 * t)) / 34;
            }
            return result;
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); var r = rectTransform.rect;
            vh.AddVert(Vector3.zero, color, Vector2.zero);
            for (int i = 0; i < Segments; i++) vh.AddVert(new Vector3(Outline[i].x * r.width, Outline[i].y * r.height), color, Vector2.zero);
            for (int i = 0; i < Segments; i++) vh.AddTriangle(0, i + 1, (i + 1) % Segments + 1);
        }
    }
}
