using TMPro;
using UnityEngine;
namespace WildType
{
    // Two local observations, never a remote food map or an AI-intention readout.
    public sealed class ForageCues : MonoBehaviour
    {
        public const int Capacity=2;
        StageSession session;RectTransform canvas;Camera view;
        readonly TMP_Text[] labels=new TMP_Text[Capacity];readonly RectTransform[] panels=new RectTransform[Capacity];readonly FoodPlant[] selected=new FoodPlant[Capacity];
        float refresh;
        public int VisibleCount{get;private set;}
        public FoodPlant At(int i)=>i>=0&&i<Capacity?selected[i]:null;
        public void Configure(StageSession s,RectTransform root){session=s;canvas=root;view=s.orbit.GetComponent<Camera>();
            for(int i=0;i<Capacity;i++){
                var panel=new GameObject("Forage cue",typeof(RectTransform),typeof(UnityEngine.UI.Image));panel.transform.SetParent(root,false);panels[i]=panel.GetComponent<RectTransform>();panels[i].sizeDelta=new Vector2(406,114);var background=panel.GetComponent<UnityEngine.UI.Image>();background.color=new Color(.035f,.065f,.07f,.72f);background.raycastTarget=false;
                var go=new GameObject("Forage observation",typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(panel.transform,false);
                var t=go.GetComponent<TextMeshProUGUI>();t.font=s.hudFont;t.fontSize=21;t.alignment=TextAlignmentOptions.Center;
                t.color=new Color(1,.95f,.72f);t.outlineWidth=.2f;t.raycastTarget=false;t.rectTransform.sizeDelta=new Vector2(390,114);
                var shadow=go.AddComponent<UnityEngine.UI.Shadow>();shadow.effectColor=new Color(0,0,0,1);shadow.effectDistance=new Vector2(2,-2);labels[i]=t;panel.SetActive(false);
            }
        }
        public static bool WithinSight(CreatureAgent actor,Vector3 point)=>actor&&Vector3.Distance(actor.transform.position,point)<=actor.Stats.Vision&&
            !Physics.Linecast(actor.transform.position+Vector3.up,point+Vector3.up,Ecosystem.WorldMask,QueryTriggerInteraction.Ignore);
        public static float WalkEstimate(CreatureAgent actor,Vector3 destination)
        {
            float distance=Mathf.Max(0,Vector3.Distance(actor.transform.position,destination)-actor.Interaction.Reach);
            float total=0;
            for(int i=0;i<8;i++){
                float speed=EcologyRules.TravelLimit(actor.Stats,Vector3.Lerp(actor.transform.position,destination,(i+.5f)/8),false)*Mathf.Sqrt(actor.Life.Growth);
                if(actor.Vitals.Energy<actor.Stats.MaxEnergy*.15f)speed*=Mathf.Lerp(.4f,1,actor.Vitals.Energy/(actor.Stats.MaxEnergy*.15f));
                total+=distance/8/Mathf.Max(.1f,speed)*(actor.Stats.PassiveDrain+actor.Stats.MoveCost*Mathf.Clamp01(speed/actor.Stats.WalkSpeed));
            }return total;
        }
        bool Visible(Vector3 point){var p=view.WorldToViewportPoint(point);return p.z>0&&p.x>.32f&&p.x<.88f&&p.y>.22f&&p.y<.8f&&
            !Physics.Linecast(view.transform.position,point,Ecosystem.WorldMask,QueryTriggerInteraction.Ignore);}
        public static bool Approaching(CreatureAgent other,FoodPlant food)
        {if(!other||other.Vitals.Dead||other.Motor.Speed<.2f||!food||!food.Available)return false;var d=food.transform.position-other.transform.position;d.y=0;return d.magnitude<6&&Vector3.Dot(other.Motor.TravelHeading,d.normalized)>.85f;}
        bool VisibleApproach(FoodPlant f){foreach(var a in session.Creatures)if(a&&a!=session.Player&&Approaching(a,f)&&WithinSight(session.Player,a.transform.position)&&Visible(a.transform.position+Vector3.up*a.CurrentHeight))return true;return false;}
        public static string Readout(CreatureAgent a,FoodPlant f,bool approach)
        {
            float distance=Vector3.Distance(a.transform.position,f.transform.position);
            string state=f.Available?"Ripe · gain up to "+JournalReadout.Approx(Mathf.Min(a.Stats.MaxEnergy-a.Vitals.Energy,f.nutrition*a.Stats.NutritionFactor))+" energy":"Empty · needs time to regrow";
            return f.DisplayName+" · "+JournalReadout.Distance(distance)+"\n"+state+(f.Available?"\nDirect walk estimate: "+JournalReadout.Approx(WalkEstimate(a,f.transform.position))+" energy":"\nTry another visible patch")+(approach?"\nGrazer approaching":"");
        }
        void LateUpdate(){
            bool active=session&&session.Ready&&!session.Paused&&session.Player&&!session.Player.Vitals.Dead;
            if(!active){for(int i=0;i<Capacity;i++){panels[i].gameObject.SetActive(false);selected[i]=null;}VisibleCount=0;refresh=0;return;}
            refresh-=Time.unscaledDeltaTime;if(refresh<=0){refresh=.2f;for(int i=0;i<Capacity;i++)selected[i]=null;
                FoodPlant ripe=null,empty=null;float ripeDistance=float.MaxValue,emptyDistance=float.MaxValue;
                foreach(var f in session.World.Foods){
                    if(!f||!WithinSight(session.Player,f.transform.position)||!Visible(Point(f)))continue;
                    // The existing E prompt owns reachable ripe food; never overlay a second action label.
                    float d=Vector3.Distance(session.Player.transform.position,f.transform.position);
                    if(f.Available&&d<=session.Player.Interaction.Reach)continue;
                    if(f.Available&&d<ripeDistance){ripe=f;ripeDistance=d;}
                    if(!f.Available&&d<emptyDistance){empty=f;emptyDistance=d;}
                    for(int i=0;i<Capacity;i++)if(!selected[i]||d<Vector3.Distance(session.Player.transform.position,selected[i].transform.position)){
                        for(int j=Capacity-1;j>i;j--)selected[j]=selected[j-1];selected[i]=f;break;}
                }
                // If both states are actually visible, keep a ripe alternative beside depletion.
                if(ripe&&empty){selected[0]=ripe;selected[1]=empty;}
                for(int i=0;i<Capacity;i++)if(selected[i])labels[i].text=Readout(session.Player,selected[i],VisibleApproach(selected[i]));
            }
            VisibleCount=0;
            for(int i=0;i<Capacity;i++){
                var f=selected[i];bool show=f&&WithinSight(session.Player,f.transform.position)&&Visible(Point(f));panels[i].gameObject.SetActive(show);if(!show)continue;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas,view.WorldToScreenPoint(Point(f)),null,out var local);panels[i].anchoredPosition=local+Vector2.up*62;
                if(i>0&&panels[0].gameObject.activeSelf){var delta=panels[i].anchoredPosition-panels[0].anchoredPosition;if(Mathf.Abs(delta.x)<416&&Mathf.Abs(delta.y)<120)panels[i].anchoredPosition+=Vector2.up*124;}
                VisibleCount++;
            }
        }
        static Vector3 Point(FoodPlant f)=>f.transform.position+Vector3.up*(f.Region==Habitat.Dry?2.8f:1.8f);
    }
}
