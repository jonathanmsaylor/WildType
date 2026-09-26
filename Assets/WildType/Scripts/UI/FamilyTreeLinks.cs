using UnityEngine;
using UnityEngine.UI;
namespace WildType
{
    // One clipped mesh for edges. No per-edge GameObjects; a highlighted path is never a new relationship.
    public sealed class FamilyTreeLinks : MaskableGraphic
    {
        public FamilyTreeLayout Layout;
        public CreatureId Selected;
        public Rect Visible;
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();if(Layout==null)return;
            foreach(var link in Layout.Links){if(!Layout.Bounds(link.child).Overlaps(Visible))continue;
                bool selected=link.parent==Selected||link.child==Selected;
                if(!selected&&!Layout.Bounds(link.parent).Overlaps(Visible))continue;
                var a=Layout.Positions[link.parent];var b=Layout.Positions[link.child];
                a+=new Vector2(FamilyTreeLayout.CardWidth*.5f,FamilyTreeLayout.CardHeight);
                b+=new Vector2(FamilyTreeLayout.CardWidth*.5f,0);
                var tint=selected?new Color(.98f,.82f,.34f,1):new Color(.69f,.72f,.64f,.35f);
                // Distinct short ports and a straight route avoid implying junctions where links cross.
                var start=a+new Vector2(0,14);var end=b-new Vector2(0,14);
                Line(mesh,a,start,tint,selected?3:2);Line(mesh,start,end,tint,selected?3:2);Line(mesh,end,b,tint,selected?3:2);
                if(selected){Line(mesh,b,b+new Vector2(-5,-7),tint,2);Line(mesh,b,b+new Vector2(5,-7),tint,2);}}
        }
        void Line(VertexHelper mesh,Vector2 a,Vector2 b,Color tint,float width)
        {
            var bounds=Rect.MinMaxRect(Mathf.Min(a.x,b.x)-width,Mathf.Min(a.y,b.y)-width,Mathf.Max(a.x,b.x)+width,Mathf.Max(a.y,b.y)+width);
            if(!bounds.Overlaps(Visible))return;
            a.y=-a.y;b.y=-b.y;var n=new Vector2(-(b-a).y,(b-a).x).normalized*width*.5f;int first=mesh.currentVertCount;
            foreach(var p in new[]{a-n,a+n,b+n,b-n})mesh.AddVert(p,tint,Vector2.zero);
            mesh.AddTriangle(first,first+1,first+2);mesh.AddTriangle(first,first+2,first+3);
        }
    }
}
