using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace WildType
{
    // A 68 m-wide visual study; all original terrain and obstacle colliders remain untouched.
    public sealed class PainterlyMeadow:MonoBehaviour
    {
        public const float Radius=34;
        readonly List<Mesh> owned=new List<Mesh>();
        readonly List<MeshFilter> filters=new List<MeshFilter>();
        readonly List<Mesh> originals=new List<Mesh>(),trimmed=new List<Mesh>();
        GameObject art;Material material;
        public int GeneratedVertices {get;private set;}
        public int TreeCount {get;private set;}
        public void Build(StageSession session,Material shared)
        {
            if(art)return;material=shared;art=new GameObject("Limited meadow study");art.transform.SetParent(transform,false);
            var terrain=GameObject.Find("250m ecosystem terrain");var scenery=GameObject.Find("Procedural scenery");
            if(terrain)Cut(terrain.GetComponent<MeshFilter>(),true);
            if(scenery)foreach(var f in scenery.GetComponentsInChildren<MeshFilter>())Cut(f,false);
            var random=new System.Random(73411);var grass=new PainterMesh();var flowers=new PainterMesh();
            for(int n=0;n<14500;n++){
                float a=Next(random)*Mathf.PI*2,r=Mathf.Sqrt(Next(random))*(Radius-1),x=Mathf.Cos(a)*r,z=Mathf.Sin(a)*r;
                if(Mathf.Abs(x-Mathf.Sin(z*.11f)*2)<1.2f||NearFood(session,x,z))continue;
                Vector3 p=Ground(x,z);float patch=Mathf.PerlinNoise(x*.13f+21,z*.13f+37);
                Color tint=Color.Lerp(new Color(.27f,.41f,.12f),new Color(.66f,.70f,.25f),Next(random)*.65f+patch*.35f);
                float height=Mathf.Lerp(.18f,.63f,Next(random))*Mathf.Lerp(.65f,1,patch);
                for(int blade=0;blade<3;blade++){float angle=Next(random)*6.283f;Vector3 side=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));grass.Leaf(p,p+Vector3.up*height+side*.15f,Vector3.Cross(side,Vector3.up)*.035f,tint);}
                if(n%12==0&&patch>.40f){float h=height+.20f;Vector3 stem=p+Vector3.up*h;flowers.Leaf(p,stem,Vector3.right*.015f,new Color(.27f,.41f,.15f));
                    Color petal=n%3==0?new Color(.91f,.59f,.63f):n%3==1?new Color(.94f,.88f,.64f):new Color(.59f,.57f,.80f);
                    for(int k=0;k<5;k++){float angle=k*6.283f/5;Vector3 d=new Vector3(Mathf.Cos(angle),.22f,Mathf.Sin(angle));flowers.Leaf(stem,stem+d*.14f,Vector3.Cross(d,Vector3.up)*.062f,petal);}
                    flowers.Ellipsoid(stem+Vector3.up*.012f,new Vector3(.035f,.021f,.035f),new Color(.87f,.64f,.19f),4,6);
                }
            }
            Add("Bent grass and leaves",grass.Build("Meadow brush grass"),false);Add("Wildflower drifts",flowers.Build("Original five-petal wildflowers"),false);
            var bark=new PainterMesh();var canopy=new PainterMesh();
            if(scenery)foreach(var collider in scenery.GetComponentsInChildren<Collider>()){
                Vector3 p=collider.transform.position;p.y=0;
                if(p.sqrMagnitude>Radius*Radius)continue;
                if(collider.name=="Rock"&&collider is MeshCollider rock){var o=new GameObject("Painted existing rock",typeof(MeshFilter),typeof(MeshRenderer));o.transform.SetParent(art.transform,false);o.transform.SetPositionAndRotation(collider.transform.position,collider.transform.rotation);o.transform.localScale=collider.transform.lossyScale;o.GetComponent<MeshFilter>().sharedMesh=rock.sharedMesh;var r=o.GetComponent<MeshRenderer>();r.sharedMaterial=material;var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",new Color(.51f,.53f,.43f));r.SetPropertyBlock(block);continue;}
                if(collider.name!="Trunk")continue;
                p=Ground(p.x,p.z);float height=collider.transform.lossyScale.y*4.6f;
                Tree(p,height,random,bark,canopy);TreeCount++;
            }
            Add("Organic branching trunks",bark.Build("Meadow branches"),true);Add("Broken leaf canopy",canopy.Build("Meadow foliage"),true);
            // Distant non-colliding painted ridge silhouettes; outside the 250 m ecosystem.
            for(int layer=0;layer<3;layer++)Add("Atmospheric ridge "+layer,PainterMesh.Mountains(layer),false);
            SetPainted(true);
        }
        static bool NearFood(StageSession s,float x,float z){foreach(var f in s.World.Foods){if(!f)continue;var p=f.transform.position;if((p.x-x)*(p.x-x)+(p.z-z)*(p.z-z)<2.1f)return true;}return false;}
        static float Next(System.Random r)=>(float)r.NextDouble();
        static Vector3 Ground(float x,float z){if(Physics.Raycast(new Vector3(x,60,z),Vector3.down,out var hit,100,Ecosystem.GroundMask))return hit.point+Vector3.up*.014f;return new Vector3(x,Ecosystem.Height(x,z),z);}
        void Tree(Vector3 p,float h,System.Random r,PainterMesh bark,PainterMesh leaf)
        {
            bark.Tube(new[]{p,p+new Vector3(.15f,h*.3f,0),p+new Vector3(-.15f,h*.65f,.12f),p+Vector3.up*h},new[]{Vector2.one*.32f,Vector2.one*.24f,Vector2.one*.16f,Vector2.one*.02f},new Color(.36f,.30f,.20f),12);
            for(int b=0;b<9;b++){
                float angle=b*2.4f;Vector3 start=p+Vector3.up*(h*(.42f+b*.045f));Vector3 end=start+new Vector3(Mathf.Cos(angle)*h*.28f,h*.19f,Mathf.Sin(angle)*h*.28f);
                bark.Tube(new[]{start,Vector3.Lerp(start,end,.5f)+Vector3.up*.12f,end},new[]{Vector2.one*.11f,Vector2.one*.065f,Vector2.one*.006f},new Color(.42f,.34f,.22f),8);
                for(int n=0;n<170;n++){
                    float a=Next(r)*6.283f,y=Next(r)*2-1;Vector3 offset=new Vector3(Mathf.Cos(a)*Mathf.Sqrt(1-y*y),y*.62f,Mathf.Sin(a)*Mathf.Sqrt(1-y*y))*h*.18f*Mathf.Pow(Next(r),.333f);
                    Vector3 center=end+offset;float yaw=Next(r)*6.283f;Vector3 d=new Vector3(Mathf.Cos(yaw),Next(r)*.7f-.1f,Mathf.Sin(yaw));
                    Color c=Color.Lerp(new Color(.18f,.32f,.15f),new Color(.66f,.69f,.26f),Mathf.Clamp01(.5f+y*.3f+Next(r)*.25f));
                    leaf.Leaf(center-d*.25f,center+d*.50f,Vector3.Cross(d,Vector3.up)*.23f,c);
                }
            }
        }
        void Cut(MeshFilter f,bool terrain)
        {
            if(!f||!f.sharedMesh||!f.sharedMesh.isReadable)return;Mesh source=f.sharedMesh;Vector3[] vertices=source.vertices;var copy=Instantiate(source);copy.name=source.name+" outside study";
            var patch=new List<int>();
            // Keep each connected original decorative piece whole at the study edge.
            // A radial per-triangle cut through a crown would leave visible holes.
            bool[] pieceInside=terrain?null:ClassifyPieces(source,vertices,f.transform);
            for(int sub=0;sub<source.subMeshCount;sub++){
                var keep=new List<int>();int[] indices=source.GetTriangles(sub);
                for(int i=0;i<indices.Length;i+=3){Vector3 center=f.transform.TransformPoint((vertices[indices[i]]+vertices[indices[i+1]]+vertices[indices[i+2]])/3);bool inside=terrain?center.x*center.x+center.z*center.z<Radius*Radius:pieceInside[indices[i]];
                    var target=inside?patch:keep;target.Add(indices[i]);target.Add(indices[i+1]);target.Add(indices[i+2]);}
                copy.SetTriangles(keep,sub);
            }
            owned.Add(copy);filters.Add(f);originals.Add(source);trimmed.Add(copy);
            if(terrain){var ground=Instantiate(source);ground.name="Exact original terrain surface study";ground.subMeshCount=1;ground.SetTriangles(patch,0);var color=new Color[vertices.Length];for(int i=0;i<color.Length;i++){float t=Mathf.PerlinNoise(vertices[i].x*.08f+12,vertices[i].z*.08f+29);color[i]=Color.Lerp(new Color(.28f,.40f,.14f),new Color(.60f,.65f,.28f),t).linear;}ground.colors=color;Add("Meadow ground wash",ground,true);}
        }
        static bool[] ClassifyPieces(Mesh mesh,Vector3[] vertices,Transform transform)
        {
            // PrototypeBuilder combines 515-vertex spheres and 88-vertex cylinders in order.
            // Their duplicated pole/seam vertices are not necessarily topologically connected.
            int stride=mesh.name=="Fern canopy combined"||mesh.name=="Weathered rock combined"?515:mesh.name=="Warm bark combined"?88:0;
            if(stride>0&&vertices.Length%stride==0){var pieces=new bool[vertices.Length];for(int start=0;start<vertices.Length;start+=stride){Vector3 center=Vector3.zero;for(int i=0;i<stride;i++)center+=transform.TransformPoint(vertices[start+i]);center/=stride;bool within=center.x*center.x+center.z*center.z<Radius*Radius;for(int i=0;i<stride;i++)pieces[start+i]=within;}return pieces;}
            var parent=new int[vertices.Length];for(int i=0;i<parent.Length;i++)parent[i]=i;
            int Root(int n){while(parent[n]!=n){parent[n]=parent[parent[n]];n=parent[n];}return n;}
            var indices=mesh.triangles;for(int i=0;i<indices.Length;i+=3){int a=Root(indices[i]),b=Root(indices[i+1]),c=Root(indices[i+2]);parent[b]=a;parent[c]=a;}
            var centers=new Vector3[vertices.Length];var count=new int[vertices.Length];for(int i=0;i<vertices.Length;i++){int r=Root(i);centers[r]+=transform.TransformPoint(vertices[i]);count[r]++;}
            var inside=new bool[vertices.Length];for(int i=0;i<vertices.Length;i++){int r=Root(i);Vector3 p=centers[r]/Mathf.Max(1,count[r]);inside[i]=p.x*p.x+p.z*p.z<Radius*Radius;}return inside;
        }
        void Add(string label,Mesh mesh,bool shadows){owned.Add(mesh);GeneratedVertices+=mesh.vertexCount;var o=new GameObject(label,typeof(MeshFilter),typeof(MeshRenderer));o.transform.SetParent(art.transform,false);o.GetComponent<MeshFilter>().sharedMesh=mesh;var r=o.GetComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=shadows?ShadowCastingMode.On:ShadowCastingMode.Off;}
        public void SetPainted(bool value){if(!art)return;art.SetActive(value);for(int i=0;i<filters.Count;i++)if(filters[i])filters[i].sharedMesh=value?trimmed[i]:originals[i];}
        void OnDestroy(){for(int i=0;i<filters.Count;i++)if(filters[i])filters[i].sharedMesh=originals[i];foreach(var m in owned)if(m)Destroy(m);}
    }
}
