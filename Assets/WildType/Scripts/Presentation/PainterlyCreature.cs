using System.Collections.Generic;
using UnityEngine;
namespace WildType
{
    // Optional presentation under the existing VisualRoot. Genome, motor, collider and growth stay owned by the game.
    [DefaultExecutionOrder(200)]
    public sealed class PainterlyCreature:MonoBehaviour
    {
        CreatureAgent actor;Transform body,tail;
        readonly Transform[] legs=new Transform[4];readonly Transform[] shins=new Transform[4];
        readonly List<Mesh> owned=new List<Mesh>();
        Renderer[] original;GameObject art;Material material;float phase,clock;Vector3 bodyOrigin;
        public CreatureAppearance Appearance {get;private set;}
        public Mesh CoatMesh {get;private set;}
        public bool Painted=>art&&art.activeSelf;
        public void Build(CreatureAgent creature,Material shared)
        {
            if(art)return;actor=creature;material=shared;Appearance=new CreatureAppearance(actor.Genome);var a=Appearance;
            original=actor.Visual.GetComponentsInChildren<Renderer>();
            art=new GameObject("Painterly visual study");art.transform.SetParent(transform,false);
            CoatMesh=PainterMesh.Body(a);body=Part("Continuous body and neck",art.transform,CoatMesh,a.Coat,true);
            float size=a.Size;
            for(int sign=-1;sign<=1;sign+=2){
                var details=new PainterMesh();Vector3 ear=new Vector3(sign*a.BodyWidth*.18f,a.BodyHeight+size*.97f,a.BodyLength*.52f);
                details.Leaf(ear,ear+new Vector3(sign*.24f,.46f,-.10f)*size,new Vector3(.14f,0,.04f)*size,Color.white);
                details.Leaf(ear+Vector3.forward*.018f,ear+new Vector3(sign*.18f,.37f,-.07f)*size,new Vector3(.075f,0,.025f)*size,new Color(.64f,.60f,.44f));
                Part("Leaf ear",body,details.Build("Original ear"),a.Accent);
                var eye=new PainterMesh();eye.Ellipsoid(new Vector3(sign*a.BodyWidth*.19f,a.BodyHeight+size*.82f,a.BodyLength*.635f),new Vector3(.06f,.09f,.073f)*size,Color.white);
                Part("Gentle eye",body,eye.Build("Eye"),new Color(.035f,.05f,.065f));
                var glint=new PainterMesh();glint.Ellipsoid(new Vector3(sign*a.BodyWidth*.222f,a.BodyHeight+size*.85f,a.BodyLength*.66f),Vector3.one*.021f*size,Color.white,6,8);
                Part("Eye light",body,glint.Build("Eye light"),new Color(.98f,.94f,.76f));
            }
            for(int i=0;i<4;i++){
                float x=(i%2==0?-1:1)*a.BodyWidth*.31f,z=(i<2?.27f:-.33f)*a.BodyLength;
                legs[i]=Joint("Shoulder pivot",art.transform,new Vector3(x,a.LegLength,z));
                var upper=new PainterMesh();upper.Tube(new[]{new Vector3(0,.36f*size,0),new Vector3(0,.16f*size,0),Vector3.down*a.LegLength*.18f,new Vector3(0,-a.LegLength*.48f,i<2?.06f:-.12f)},new[]{Vector2.one*.015f,new Vector2(a.LegThickness*.85f,a.LegThickness*.9f),new Vector2(a.LegThickness*.65f,a.LegThickness*.68f),Vector2.one*a.LegThickness*.35f},Color.white,18);
                Part("Tapered upper limb",legs[i],upper.Build("Upper limb"),a.Coat);
                shins[i]=Joint("Hock pivot",legs[i],new Vector3(0,-a.LegLength*.48f,i<2?.06f:-.12f));
                var lower=new PainterMesh();lower.Tube(new[]{Vector3.up*.03f,Vector3.down*a.LegLength*.25f,new Vector3(0,-a.LegLength*.47f,.09f*size)},new[]{Vector2.one*a.LegThickness*.38f,Vector2.one*a.LegThickness*.26f,Vector2.one*a.LegThickness*.31f},Color.white,14);
                Part("Slender lower limb",shins[i],lower.Build("Lower limb"),Color.Lerp(a.Coat,a.Mark,.22f));
                var hoof=new PainterMesh();hoof.Ellipsoid(new Vector3(0,-a.LegLength*.47f,.15f*size),new Vector3(.115f,.08f,.19f)*size,Color.white,8,12);Part("Soft hoof",shins[i],hoof.Build("Hoof"),Color.Lerp(a.Coat,new Color(.12f,.15f,.17f),.72f));
            }
            tail=Joint("Tail sway",art.transform,new Vector3(0,a.BodyHeight+.11f*size,-a.BodyLength*.47f));
            var plume=new PainterMesh();plume.Tube(new[]{Vector3.zero,new Vector3(0,-.16f,-.32f)*size,new Vector3(0,-.4f,-.72f)*size,new Vector3(0,-.49f,-1.08f)*size},new[]{Vector2.one*.18f*size,Vector2.one*.16f*size,Vector2.one*.09f*size,Vector2.one*.004f},Color.white,18);
            Part("Tapered tail",tail,plume.Build("Tail"),a.Coat);
            var fur=new PainterMesh();for(int i=0;i<20;i++){float t=i/19f;Vector3 p=new Vector3(0,a.BodyHeight+a.BodyDepth*(.44f+.06f*Mathf.Sin(t*3)),Mathf.Lerp(-.32f,.26f,t)*a.BodyLength);fur.Leaf(p,p+new Vector3(0,.07f,-.12f)*size,Vector3.right*.045f*size,Color.white);}
            Part("Dorsal brush tufts",body,fur.Build("Brush tufts"),a.Accent);
            bodyOrigin=body.localPosition;SetPainted(true);
        }
        Transform Joint(string label,Transform parent,Vector3 position){var t=new GameObject(label).transform;t.SetParent(parent,false);t.localPosition=position;return t;}
        Transform Part(string label,Transform parent,Mesh mesh,Color tint,bool bands=false)
        {
            owned.Add(mesh);var go=new GameObject(label,typeof(MeshFilter),typeof(MeshRenderer));go.layer=10;go.transform.SetParent(parent,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;var r=go.GetComponent<MeshRenderer>();r.sharedMaterial=material;
            var b=new MaterialPropertyBlock();b.SetColor("_BaseColor",tint);b.SetColor("_MarkColor",Appearance.Mark);b.SetFloat("_Pattern",bands?1:0);b.SetFloat("_BandWidth",Appearance.BandWidth);r.SetPropertyBlock(b);return go.transform;
        }
        public void SetPainted(bool value){if(!art)return;art.SetActive(value);foreach(var r in original)if(r)r.enabled=!value;}
        void LateUpdate()
        {
            if(!actor||!Painted||actor.Session.Paused)return;float dt=Time.deltaTime;clock+=dt;
            float speed=actor.Motor.Speed,amount=Mathf.Clamp01(speed/actor.Stats.SprintSpeed);
            phase+=speed/Mathf.Max(.5f,Appearance.LegLength*transform.localScale.x*1.8f)*Mathf.PI*2*dt;
            body.localPosition=bodyOrigin+Vector3.up*((Mathf.Sin(clock*1.7f)*.011f+Mathf.Sin(phase*2)*.025f*amount)*Appearance.Size);
            float grazing=actor.Visual.Feeding>0?Mathf.Sin(actor.Visual.Feeding*4)*7:0;
            body.localRotation=Quaternion.Euler(grazing+Mathf.Sin(clock*1.2f)*.6f,0,Mathf.Sin(phase)*1.2f*amount);
            for(int i=0;i<4;i++){float step=Mathf.Sin(phase+(i==0||i==3?0:Mathf.PI));legs[i].localRotation=Quaternion.Euler(step*26*amount,0,0);shins[i].localRotation=Quaternion.Euler(Mathf.Max(0,-step)*43*amount,0,0);}
            tail.localRotation=Quaternion.Euler(Mathf.Sin(clock*1.3f)*3,Mathf.Sin(clock*1.8f)*(5+amount*8),0);
        }
        void OnDestroy(){foreach(var mesh in owned)if(mesh)Destroy(mesh);}
    }
}
