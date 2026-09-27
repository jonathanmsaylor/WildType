using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace WildType
{
    public sealed partial class StageHud
    {
        GameObject overviewPanel;
        TMP_Text overviewIdentity, needsHeading, overviewStatus;
        ResourceBars overviewBars;
        readonly Button[] needsButtons = new Button[3];
        readonly CreatureId[] needsIds = new CreatureId[3];
        readonly List<CreatureAgent> needs = new List<CreatureAgent>(24);
        public int FamilyNeedCount => needs.Count;
        // The same warning thresholds as ordinary play, without diagnosing unavailable actors.
        public static string FamilyNeed(CreatureAgent a) => !a || a.Vitals.Dead ? "" :
            GameplayText.Status(a.Vitals.Energy, a.Stats.MaxEnergy, a.Vitals.Health, a.Vitals.SprintLocked);
        sealed class ResourceBars
        {
            public GameObject Root;
            public TMP_Text[] Labels = new TMP_Text[3];
            public Image[] Fills = new Image[3];
            public void Read(CreatureAgent actor)
            {
                Root.SetActive(actor && !actor.Vitals.Dead);
                if (!Root.activeSelf) return;
                float[] values = {actor.Vitals.Energy, actor.Vitals.Health, actor.Vitals.Stamina};
                float[] maximum = {actor.Stats.MaxEnergy, 100, actor.Stats.MaxStamina};
                string[] names = {"Energy", "Health", "Stamina"};
                for (int i=0;i<3;i++) {
                    bool low = i==0 && values[i]<=maximum[i]*.25f;
                    Labels[i].text=names[i]+(low?" LOW":"")+"  "+JournalReadout.Whole(values[i])+"/"+JournalReadout.Whole(maximum[i]);
                    Fills[i].rectTransform.localScale=new Vector3(Mathf.Clamp01(values[i]/maximum[i]),1,1);
                    Fills[i].color=low?new Color(1,.55f,.3f):i==0?new Color(.68f,.85f,.39f):i==1?new Color(.96f,.65f,.49f):new Color(.45f,.8f,.92f);
                }
            }
        }
        ResourceBars MakeResourceBars(Transform parent,float x,float y,float width,float spacing,int font)
        {
            var root=Box(parent,"Live resources",x,-y,width,spacing*3,new Vector2(0,1),0);
            root.GetComponent<Image>().raycastTarget=false;
            var result=new ResourceBars{Root=root.gameObject};
            string[] names={"Energy","Health","Stamina"};
            for(int i=0;i<3;i++) {
                result.Labels[i]=Text(root,"",0,-i*spacing,width,spacing-7,font);result.Labels[i].name=names[i]+" live value";
                var track=Box(root,names[i]+" live track",0,-i*spacing-spacing+8,width,5,new Vector2(0,1),.85f);track.GetComponent<Image>().raycastTarget=false;
                result.Fills[i]=Box(track,names[i]+" live fill",0,0,width,5,new Vector2(0,1),1).GetComponent<Image>();result.Fills[i].raycastTarget=false;
            }
            return result;
        }
        void BuildOverview(RectTransform panel)
        {
            var area=Box(panel,"Journal overview",348,-130,1184,740,new Vector2(0,1),0);overviewPanel=area.gameObject;
            overviewIdentity=Text(area,"",20,-4,1128,76,30);
            overviewBars=MakeResourceBars(area,20,88,550,56,25);
            overviewStatus=Text(area,"",600,-88,530,168,25);
            needsHeading=Text(area,"",20,-284,1128,68,25);
            for(int i=0;i<3;i++) {int index=i;needsButtons[i]=ButtonAt(area,"Family need "+i,20,364+i*98,1128,88,()=>ShowTree(needsIds[index]),23);
                needsButtons[i].GetComponentInChildren<TMP_Text>().alignment=TextAlignmentOptions.Left;
                Help(needsButtons[i],"Open this descendant in the Family Tree. Reading does not feed, heal or transfer control. Only eligible direct juvenile children can receive paid care nearby.");}
            ButtonAt(area,"Overview events",20,674,550,50,()=>Show(JournalView.Events),24,"Recent births / deaths");
            ButtonAt(area,"Overview family",594,674,554,50,()=>Show(JournalView.Chronicle),24,"See Family Tree");
        }
        void RefreshOverview()
        {
            var player=session.Player;
            overviewIdentity.text="<b>"+session.Names.PersonalName(player.Life.Id)+"</b>\n"+session.World.Zone(player.transform.position);
            overviewBars.Read(player);
            overviewStatus.text=player.Vitals.Dead?"Life ended. Your family records remain.":FamilyNeed(player);
            if(overviewStatus.text.Length==0)overviewStatus.text="No urgent needs right now.";
            needs.Clear();
            foreach(var a in session.Generations.LivingDescendants(player.Life.Id))if(FamilyNeed(a).Length>0)needs.Add(a);
            needs.Sort((a,b)=>{
                float Priority(CreatureAgent c)=>c.Vitals.Energy/c.Stats.MaxEnergy+(c.Life.Adult?.4f:0)+(session.Care.IsChild(player,c)?0:.2f);
                int value=Priority(a).CompareTo(Priority(b));return value!=0?value:a.Life.Id.CompareTo(b.Life.Id);
            });
            needsHeading.text="<b>Family Needs Attention</b>\n"+(needs.Count==0?"No urgent needs among your living descendants.":needs.Count+(needs.Count==1?" living descendant":" living descendants")+" · select to find them in the tree"+(needs.Count>3?" · showing 3 first":""));
            for(int i=0;i<3;i++) {
                needsButtons[i].gameObject.SetActive(i<needs.Count);needsIds[i]=default;if(i>=needs.Count)continue;
                var a=needs[i];needsIds[i]=a.Life.Id;
                string name=session.Names.PersonalName(a.Life.Id);if(name.Length>24)name=name.Substring(0,23)+"…";
                SetButtonText(needsButtons[i],"<b>"+name+"</b> · "+(a.Life.Adult?"Adult":"Juvenile")+" · "+session.World.Zone(a.transform.position)+"\n"+FamilyNeed(a).Split('\n')[0]);
            }
        }
    }
}
