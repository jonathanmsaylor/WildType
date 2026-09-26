using UnityEngine;
using UnityEngine.UI;
namespace WildType
{
    // Original ink-like UI shapes: authored geometry, no font glyphs or downloaded assets.
    public sealed class JournalMotif : MaskableGraphic
    {
        public int Kind;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (Kind == 0) { Leaf(vh, -.18f, -.10f, .18f, .30f); Leaf(vh, .18f, .15f, .18f, .30f); }
            else if (Kind == 1) { Leaf(vh, 0, 0, .44f, .24f); Leaf(vh, 0, 0, .10f, .15f); }
            else if (Kind == 2) { Leaf(vh, -.16f, .06f, .25f, .34f); Leaf(vh, .16f, .06f, .25f, .34f); }
            else { Leaf(vh, -.22f, .19f, .17f, .19f); Leaf(vh, .22f, .19f, .17f, .19f); Leaf(vh, 0, -.19f, .21f, .22f); }
        }
        void Leaf(VertexHelper vh, float x, float y, float w, float h)
        {
            var r = rectTransform.rect; int start = vh.currentVertCount;
            Vector2[] p = {new Vector2(x,y-h),new Vector2(x-w,y-.06f),new Vector2(x-w*.75f,y+h*.55f),new Vector2(x,y+h),new Vector2(x+w,y+.04f),new Vector2(x+w*.6f,y-h*.65f)};
            foreach(var v in p) vh.AddVert(new Vector3(v.x*r.width,v.y*r.height),color,Vector2.zero);
            for(int i=1;i<p.Length-1;i++)vh.AddTriangle(start,start+i,start+i+1);
        }
    }
}
