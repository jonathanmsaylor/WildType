using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace WildType
{
    // Original, deterministic procedural geometry. Only called during construction, never per frame.
    public sealed class PainterMesh
    {
        readonly List<Vector3> vertices=new List<Vector3>();
        readonly List<Color> colors=new List<Color>();
        readonly List<Vector2> uv=new List<Vector2>();
        readonly List<int> triangles=new List<int>();
        public int VertexCount=>vertices.Count;
        public void Leaf(Vector3 basePoint,Vector3 tip,Vector3 side,Color color)
        {
            int k=vertices.Count;
            for(int row=0;row<=4;row++){float t=row*.25f;float width=Mathf.Sin(Mathf.PI*t);Vector3 p=Vector3.Lerp(basePoint,tip,t)+Vector3.up*Mathf.Sin(Mathf.PI*t)*side.magnitude*.3f;
                Add(p-side*width,color*.94f,new Vector2(0,t));Add(p+side*width,color,new Vector2(1,t));
                if(row<4){int n=k+row*2;Tri(n,n+2,n+1);Tri(n+1,n+2,n+3);}}
        }
        public void Tube(Vector3[] centers,Vector2[] radii,Color color,int sides=14,bool longitudinalUV=false)
        {
            int first=vertices.Count;
            for(int j=0;j<centers.Length;j++){
                Vector3 tangent=(centers[Mathf.Min(j+1,centers.Length-1)]-centers[Mathf.Max(0,j-1)]).normalized;
                Vector3 x=Vector3.Cross(tangent,Mathf.Abs(tangent.y)>.95f?Vector3.forward:Vector3.up).normalized;
                Vector3 y=Vector3.Cross(x,tangent).normalized;
                for(int k=0;k<=sides;k++){
                    float a=k*Mathf.PI*2/sides;
                    Add(centers[j]+x*(Mathf.Cos(a)*radii[j].x)+y*(Mathf.Sin(a)*radii[j].y),color,
                        new Vector2(j/(float)(centers.Length-1),longitudinalUV?Mathf.Sin(a)*.5f+.5f:k/(float)sides));
                    if(j<centers.Length-1&&k<sides){int n=first+j*(sides+1)+k;Tri(n,n+sides+1,n+1);Tri(n+1,n+sides+1,n+sides+2);}
                }
            }
        }
        public void Ellipsoid(Vector3 center,Vector3 size,Color color,int rings=10,int sides=16)
        {
            var p=new Vector3[rings+1];var r=new Vector2[rings+1];
            for(int i=0;i<=rings;i++){float a=i*Mathf.PI/rings;p[i]=center+Vector3.forward*(Mathf.Cos(a)*size.z);r[i]=new Vector2(Mathf.Max(.001f,Mathf.Sin(a)*size.x),Mathf.Max(.001f,Mathf.Sin(a)*size.y));}
            Tube(p,r,color,sides);
        }
        void Add(Vector3 v,Color c,Vector2 tex){vertices.Add(v);colors.Add(c.linear);uv.Add(tex);}
        void Tri(int a,int b,int c){triangles.Add(a);triangles.Add(b);triangles.Add(c);}
        public Mesh Build(string name){var mesh=new Mesh{name=name,indexFormat=vertices.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16};mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;}
        public static Mesh Body(CreatureAppearance a)
        {
            // A continuous haunch, ribcage, rising neck and tapered muzzle, rather than intersecting spheres.
            float[] z={-.56f,-.49f,-.34f,-.12f,.12f,.30f,.39f,.45f,.52f,.63f,.73f,.79f};
            float[] y={-.02f,.00f,.02f,.01f,.04f,.12f,.36f,.66f,.79f,.77f,.66f,.65f};
            float[] w={.015f,.33f,.48f,.50f,.44f,.32f,.24f,.20f,.24f,.22f,.14f,.01f};
            float[] h={.015f,.32f,.47f,.49f,.46f,.39f,.31f,.28f,.30f,.24f,.12f,.01f};
            const int subdivisions=4;int count=(z.Length-1)*subdivisions+1;var centers=new Vector3[count];var radii=new Vector2[count];
            for(int i=0;i<count;i++){float t=i/(float)subdivisions;int j=Mathf.Min((int)t,z.Length-2);float f=t-j;
                centers[i]=new Vector3(0,a.BodyHeight+Smooth(y,j,f)*a.Size,Smooth(z,j,f)*a.BodyLength);
                radii[i]=new Vector2(Mathf.Max(.002f,Smooth(w,j,f)*a.BodyWidth),Mathf.Max(.002f,Smooth(h,j,f)*a.BodyDepth));}
            var mesh=new PainterMesh();mesh.Tube(centers,radii,Color.white,32,true);return mesh.Build("Inherited organic coat");
        }
        public static Mesh Mountains(int layer)
        {
            const int nx=160,nz=44;var mesh=new PainterMesh();
            for(int z=0;z<=nz;z++)for(int x=0;x<=nx;x++){
                float px=Mathf.Lerp(-330,330,x/(float)nx),pz=160+layer*70+z*2.8f;
                float ridge=Mathf.Sin(z/(float)nz*Mathf.PI);float peaks=.45f+Mathf.PerlinNoise(px*.009f+layer*11,layer*7+31)*.75f;
                float crags=Mathf.PerlinNoise(px*.047f+71,pz*.035f)*.65f+Mathf.PerlinNoise(px*.13f,pz*.12f)*.25f;
                float height=-8+ridge*(30+layer*15+peaks*55+crags*29);
                float snow=Mathf.SmoothStep(0,1,Mathf.InverseLerp(66+layer*8,88+layer*8,height+crags*22));
                Color rock=Color.Lerp(new Color(.23f,.33f,.45f),new Color(.47f,.57f,.68f),layer*.28f+crags*.25f);
                Color c=Color.Lerp(rock,Color.Lerp(new Color(.86f,.87f,.81f),new Color(.63f,.73f,.81f),layer*.2f),snow);
                mesh.Add(new Vector3(px,height,pz),c,new Vector2(x/(float)nx,z/(float)nz));
                if(x<nx&&z<nz){int i=z*(nx+1)+x;mesh.Tri(i,i+nx+1,i+1);mesh.Tri(i+1,i+nx+1,i+nx+2);}
            }
            return mesh.Build("Original eroded alpine ridge "+layer);
        }
        static float Smooth(float[] p,int i,float t){float a=p[Mathf.Max(0,i-1)],b=p[i],c=p[i+1],d=p[Mathf.Min(p.Length-1,i+2)];return .5f*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t);}
    }
}
