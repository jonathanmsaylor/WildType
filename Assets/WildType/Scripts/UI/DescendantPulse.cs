using UnityEngine;
namespace WildType
{
    // One reusable presenter. No material instantiation, lights, particles or new creature meshes.
    // Restores the exact original property blocks when selection changes or ends.
    public sealed class DescendantPulse : MonoBehaviour
    {
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        static readonly Color Gold = new Color(1, .88f, .58f);
        StageSession session;
        CreatureAgent shown;
        Renderer[] renderers;
        MaterialPropertyBlock[] originals, working;
        Color[] colors;
        public CreatureAgent Shown => shown;
        public float Amount { get; private set; }
        public void Configure(StageSession value) { session = value; }
        void LateUpdate()
        {
            var target = session ? session.Locator.Target : null;
            if (!ReferenceEquals(target, shown))
            {
                Restore(); shown = target;
                if (shown && shown.Visual)
                {
                    renderers = shown.Visual.GetComponentsInChildren<Renderer>();
                    originals = new MaterialPropertyBlock[renderers.Length]; working = new MaterialPropertyBlock[renderers.Length]; colors = new Color[renderers.Length];
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        originals[i] = new MaterialPropertyBlock(); working[i] = new MaterialPropertyBlock();
                        renderers[i].GetPropertyBlock(originals[i]); renderers[i].GetPropertyBlock(working[i]);
                        colors[i] = originals[i].HasColor(BaseColor) ? originals[i].GetColor(BaseColor) :
                            renderers[i].sharedMaterial && renderers[i].sharedMaterial.HasProperty(BaseColor) ? renderers[i].sharedMaterial.GetColor(BaseColor) : Color.white;
                    }
                }
            }
            Amount = target && !session.Paused ? DescendantLocator.Pulse(session.Locator.Elapsed) : 0;
            if (renderers == null) return;
            for (int i = 0; i < renderers.Length; i++) if (renderers[i])
            {
                Color tint = Color.Lerp(colors[i], Gold, Amount); tint.a = colors[i].a;
                working[i].SetColor(BaseColor, tint); renderers[i].SetPropertyBlock(working[i]);
            }
        }
        void Restore()
        {
            if (renderers != null) for (int i = 0; i < renderers.Length; i++) if (renderers[i]) renderers[i].SetPropertyBlock(originals[i]);
            renderers = null; originals = working = null; colors = null; shown = null; Amount = 0;
        }
        void OnDisable() { Restore(); }
        void OnDestroy() { Restore(); }
    }
}
