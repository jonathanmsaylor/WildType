using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace WildType
{
    public sealed partial class StageHud
    {
        // Fixed five nodes: two recorded parents, a focus and two paged children. Never shrink a large family.
        GameObject treePanel;
        CreatureId treeId;
        int childPage;
        readonly CreatureId[] treeNodes=new CreatureId[5];
        readonly Button[] treeRecords=new Button[5],treeBranches=new Button[5];
        readonly RectTransform[] treeCards=new RectTransform[5];
        readonly List<CreatureLineageRecord> treeChildren=new List<CreatureLineageRecord>(LineageArchive.Capacity);
        readonly List<CreatureLineageRecord> treeIndex=new List<CreatureLineageRecord>(LineageArchive.Capacity);
        TMP_Text treeHeading,childrenHeading;
        Button treeLocate,treeControl,childPrevious,childNext,recordBranch;
        readonly GameObject[] parentLinks=new GameObject[2],childLinks=new GameObject[2];
        public CreatureId TreeFocus => treeId;
        public void ShowTree(CreatureId id)
        {
            if (session.Generations.Archive.Get(id)==null)return;
            treeId=id;childPage=0;Show(JournalView.Chronicle);
        }
        void BuildFamilyTree(RectTransform panel)
        {
            var area=Box(panel,"Family Tree",348,-130,1184,740,new Vector2(0,1),0);treePanel=area.gameObject;
            treeHeading=Text(area,"",20,-4,1144,76,25);
            parentLinks[0]=TreeLine(area,301,210,587,258);parentLinks[1]=TreeLine(area,881,210,587,258);
            childLinks[0]=TreeLine(area,587,372,301,450);childLinks[1]=TreeLine(area,587,372,881,450);
            for(int i=0;i<5;i++)
            {
                int slot=i;float x=i==2?306:i%2==0?20:600;float y=i<2?96:i==2?258:450;
                // Child columns are independent of slot parity.
                if(i>=3)x=i==3?20:600;
                treeCards[i]=Box(area,"Tree relative",x,-y,562,114,new Vector2(0,1),1);
                treeCards[i].GetComponent<Image>().color=CardColors[i==2?3:i<2?1:0];
                treeRecords[i]=ButtonAt(treeCards[i],"Tree record",8,6,370,102,()=>ShowDetails(treeNodes[slot]),22);
                treeRecords[i].GetComponent<Image>().color=new Color(1,1,1,0);
                treeBranches[i]=ButtonAt(treeCards[i],"Tree branch",390,36,160,44,()=>ShowTree(treeNodes[slot]),20,"View branch");
            }
            treeBranches[2].gameObject.SetActive(false);
            treeLocate=ButtonAt(area,"Tree Highlight",20,382,240,48,()=>session.Locator.Select(TreeActor()),22,"Highlight");
            treeControl=ButtonAt(area,"Tree Take control",906,382,256,48,()=>session.TakeControl(TreeActor()),22,"Take Control");
            childrenHeading=Text(area,"",20,-635,550,36,22);
            childPrevious=ButtonAt(area,"Previous children",20,580,260,46,()=>{childPage--;refresh=0;},22);
            childNext=ButtonAt(area,"Next children",300,580,260,46,()=>{childPage++;refresh=0;},22);
            ButtonAt(area,"Tree index",600,580,562,46,()=>Show(JournalView.Records),22,"All Family Records");
            ButtonAt(area,"Previous relative",20,680,260,48,()=>StepRelative(-1),22);
            ButtonAt(area,"Next relative",300,680,260,48,()=>StepRelative(1),22);
            Text(area,"Name → full record · View branch → family\nAll Family Records includes earlier branches.",600,-660,562,72,22);
            recordBranch=ButtonAt(detailsPanel.transform,"Record branch",600,480,550,46,()=>ShowTree(detailId),22,"View this Family Tree branch");
        }
        GameObject TreeLine(RectTransform parent,float x,float y,float endX,float endY)
        {
            var box=Box(parent,"Recorded parent-child link",0,0,1184,740,new Vector2(0,1),0);
            float mid=(y+endY)*.5f;
            foreach(var rect in new[]{new Vector4(x,y,3,mid-y),new Vector4(Mathf.Min(x,endX),mid,Mathf.Abs(endX-x)+3,3),new Vector4(endX,mid,3,endY-mid)})
                Box(box,"Link",rect.x,-rect.y,rect.z,rect.w,new Vector2(0,1),1).GetComponent<Image>().color=new Color(.79f,.73f,.49f,.85f);
            return box.gameObject;
        }
        CreatureAgent TreeActor(){var record=archive.Get(treeId);return record==null?null:LineageChronicle.LivingActor(session,record);}
        void StepRelative(int step)
        {
            session.CollectFamilyHistory(treeIndex);if(treeIndex.Count==0)return;
            int at=treeIndex.FindIndex(r=>r.CreatureId==treeId);at=(at+step+treeIndex.Count)%treeIndex.Count;
            ShowTree(treeIndex[at].CreatureId);
        }
        void RefreshFamilyTree()
        {
            var current=archive.Get(treeId)??archive.Get(session.Player.Life.Id);treeId=current.CreatureId;
            archive.CollectChildren(treeId,treeChildren);
            int pages=Mathf.Max(1,Mathf.CeilToInt(treeChildren.Count/2f));childPage=Mathf.Clamp(childPage,0,pages-1);
            treeNodes[0]=current.FirstParentId;treeNodes[1]=current.SecondParentId;treeNodes[2]=treeId;
            treeNodes[3]=childPage*2<treeChildren.Count?treeChildren[childPage*2].CreatureId:default;
            treeNodes[4]=childPage*2+1<treeChildren.Count?treeChildren[childPage*2+1].CreatureId:default;
            treeHeading.text=$"{session.Names.PersonalName(treeId)}'s Family Tree\n"+(current.Generation==0?"Founder · No parents recorded":"Two recorded parents above; children below");
            childrenHeading.text=$"{treeChildren.Count} recorded children · Page {childPage+1}/{pages}";
            for(int i=0;i<treeNodes.Length;i++)
            {
                var record=archive.Get(treeNodes[i]);treeCards[i].gameObject.SetActive(record!=null);if(record==null)continue;
                var actor=LineageChronicle.LivingActor(session,record);
                string life=archive.TryDeath(record.CreatureId,out _)?"Deceased":actor?(actor.Life.Adult?"Adult":"Juvenile"):"Unavailable · no death record";
                string relation=i<2?"Parent":i==2?LineageChronicle.Relationship(archive,session.Player.Life.Id,record):"Child";
                SetButtonText(treeRecords[i],$"<b>{session.Names.PersonalName(record.CreatureId)}</b> · Gen {record.Generation}\n{relation} · {life}\nOpen full record");
            }
            for(int i=0;i<2;i++){parentLinks[i].SetActive(treeNodes[i].IsValid);childLinks[i].SetActive(treeNodes[i+3].IsValid);}
            var focus=TreeActor();treeLocate.gameObject.SetActive(focus&&session.CanLocateFamily(focus)&&!session.GameOver);
            treeControl.gameObject.SetActive(focus&&session.Generations.IsLivingDescendant(focus,session.Player.Life.Id));
            childPrevious.interactable=childPage>0;childNext.interactable=childPage<pages-1;
        }
    }
}
