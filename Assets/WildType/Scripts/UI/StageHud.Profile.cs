using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace WildType
{
    public sealed partial class StageHud
    {
        GameObject profilePanel; CreatureId profileId; Vector2 savedTreePosition; CreatureId savedTreeId;
        RectTransform profileContent,profileViewport; ScrollRect profileScroll;
        readonly Button[] profileCards=new Button[4];
        readonly JournalHelpTarget[] profileHelp=new JournalHelpTarget[4];
        TMP_Text profileIdentity,profileExplanation,biomeText; CanvasGroup biomeGroup; Button profileLocate,profileControl;
        public readonly BiomeEntryReadout BiomeReadout=new BiomeEntryReadout();
        public CreatureId ProfileId=>profileId;
        public ScrollRect ProfileScroll=>profileScroll;
        public void ShowProfile(CreatureId id)
        {
            if(session.Generations.Archive.Get(id)==null)return;
            if(View==JournalView.Chronicle){savedTreePosition=treeContent.anchoredPosition;savedTreeId=id;treeId=id;}
            profileId=id;for(int i=0;i<3;i++)sections[i]=false;
            profileContent.anchoredPosition=Vector2.zero;Show(JournalView.Profile);
        }
        public void BackToTree()
        {
            Show(JournalView.Chronicle);pendingCenter=default;
            treeId=savedTreeId.IsValid?savedTreeId:session.Player.Life.Id;treeContent.anchoredPosition=savedTreePosition;
        }
        void BuildProfile(RectTransform panel)
        {
            var area=Box(panel,"Creature profile",348,-130,1184,740,new Vector2(0,1),0);profilePanel=area.gameObject;
            ButtonAt(area,"Back to Family Tree",12,0,250,48,BackToTree,23,"Back to tree");
            profileLocate=ButtonAt(area,"Profile Highlight",274,0,280,48,()=>session.Locator.Select(ProfileActor()),23,"Highlight in world");
            profileControl=ButtonAt(area,"Profile Take Control",566,0,280,48,()=>session.TakeControl(ProfileActor()),23,"Take Control");
            ButtonAt(area,"Scroll profile up",858,0,140,48,()=>ScrollProfile(-280),22,"Scroll up");
            ButtonAt(area,"Scroll profile down",1010,0,136,48,()=>ScrollProfile(280),22,"Scroll down");
            profileIdentity=Text(area,"",12,-60,1134,94,24);profileIdentity.name="Profile identity";
            profileViewport=Box(area,"Creature page viewport",12,-160,1134,450,new Vector2(0,1),.05f);
            profileViewport.gameObject.AddComponent<RectMask2D>();
            profileScroll=profileViewport.gameObject.AddComponent<ScrollRect>();profileScroll.viewport=profileViewport;profileScroll.horizontal=false;
            profileScroll.movementType=ScrollRect.MovementType.Clamped;profileScroll.inertia=false;profileScroll.scrollSensitivity=32;
            profileContent=Box(profileViewport,"Creature page content",0,0,1134,600,new Vector2(0,1),0);profileScroll.content=profileContent;
            profileScroll.verticalScrollbar=TreeBar(area,"Creature page scrollbar",1154,160,16,450,true);
            for(int i=0;i<4;i++){
                var b=ButtonAt(profileContent,"Profile card "+i,12+(i%2)*564,(i/2)*206,550,196,()=>{},23);profileCards[i]=b;
                b.GetComponent<Image>().color=CardColors[i];var t=b.GetComponentInChildren<TMP_Text>();t.alignment=TextAlignmentOptions.TopLeft;
                var help=b.gameObject.AddComponent<JournalHelpTarget>();profileHelp[i]=help;help.Show=ShowHelp;help.Hide=HideHelp;b.onClick.AddListener(help.Reveal);
                var icon=new GameObject("Category motif",typeof(RectTransform),typeof(CanvasRenderer),typeof(JournalMotif));icon.transform.SetParent(b.transform,false);
                var motif=icon.GetComponent<JournalMotif>();motif.Kind=i;motif.color=new Color(.94f,.84f,.54f);motif.raycastTarget=false;
                motif.rectTransform.anchorMin=motif.rectTransform.anchorMax=new Vector2(1,1);motif.rectTransform.anchoredPosition=new Vector2(-24,-22);motif.rectTransform.sizeDelta=new Vector2(26,26);
            }
            BuildSections();
            profileExplanation=Text(Box(area,"Profile explanation",12,-624,1134,112,new Vector2(0,1),.96f),"",14,-6,1106,100,21);
            profileExplanation.name="Profile explanation text";
            Help(profileLocate,"Briefly highlight this relative, keeping control of your own creature.");
            Help(profileControl,"Continue as this living descendant with its own age and resources. Not available for siblings or ancestors.");
        }
        CreatureAgent ProfileActor(){var r=archive?.Get(profileId);return r==null?null:LineageChronicle.LivingActor(session,r);}
        void RefreshProfile()
        {
            var r=archive.Get(profileId);if(r==null){BackToTree();return;}var a=ProfileActor();
            bool dead=archive.TryDeath(profileId,out var death);
            string state=dead?"Deceased · "+LineageChronicle.Cause(death.Cause)+" · lived "+JournalReadout.Whole(r.AgeAt(death.Time))+" s":a?(a.Life.Adult?"Living adult":"Living juvenile"):"Unavailable · no recorded death";
            string place=a?session.World.Zone(a.transform.position)+" · "+JournalReadout.Distance(Vector3.Distance(session.Player.transform.position,a.transform.position)):"Location unavailable";
            string parents=r.Generation==0?"Founder · parents not recorded":"Parents: "+session.Names.PersonalName(r.FirstParentId)+" + "+session.Names.PersonalName(r.SecondParentId);
            profileIdentity.text="<b>"+session.Names.PersonalName(profileId)+"</b> · Gen "+r.Generation+" · "+LineageChronicle.Relationship(archive,session.Player.Life.Id,r)+"\n"+state+" · "+place+"\n"+parents;
            for(int i=0;i<4;i++){
                // One available named parent up front; both complete records remain below.
                var parentId=r.FirstParentId;var p=InheritedChoices.Parent(session,parentId);
                if(!p){parentId=r.SecondParentId;p=InheritedChoices.Parent(session,parentId);}
                string comparison=a&&p?InheritedChoices.Pair(a.Stats,p.Stats,InheritedChoices.Name(session,parentId,true),i):r.Generation==0?"Founder · no parents recorded.":"Parent traits unavailable; birth record below.";
                SetButtonText(profileCards[i],"<b>"+GeneGuide.Title(i)+"</b>\n"+InheritedChoices.Summary(a,i)+"\n\n"+comparison);
                string practical=i==0?"Adult open-ground pace excludes growth and hunger. Long legs add stride but widen turns; Fernwood caps speed by turning.":i==1?"Vision limits scouting labels and AI searches, not camera zoom or eating reach. Metabolism increases bite energy and stamina recovery but also upkeep.":i==2?"Energy is food reserve; stamina fuels sprinting. Large bodies generally cost more to maintain. A bigger tank alone does not guarantee longer survival.":"Coat and markings are inherited appearance, not survival bonuses. Mating needs health, maturity, reserve, proximity and cooldown. Mutation is not improvement.";
                profileHelp[i].Explanation=practical+(a?"\n"+InheritedChoices.Values(a.Stats,i):" Current traits unavailable.")+" Both parents: expand Parent Comparisons below.";
            }
            float height=LayoutSections(r,a,418);
            profileContent.sizeDelta=new Vector2(1134,Mathf.Max(450,height));
            profileLocate.gameObject.SetActive(a&&session.CanLocateFamily(a)&&!session.GameOver);
            profileControl.gameObject.SetActive(a&&session.Generations.IsLivingDescendant(a,session.Player.Life.Id));
            if(activeHelp&&activeHelp.isActiveAndEnabled)profileExplanation.text=activeHelp.Explanation;
        }
        void EnsureProfileVisible(RectTransform target)
        {
            if(!target||!profileContent||!target.IsChildOf(profileContent)||!EventSystem.current||EventSystem.current.currentSelectedGameObject!=target.gameObject)return;
            Canvas.ForceUpdateCanvases();
            var bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(profileViewport,target);
            float dy=bounds.max.y>0?-bounds.max.y:bounds.min.y < -profileViewport.rect.height ? -profileViewport.rect.height-bounds.min.y:0;
            var at=profileContent.anchoredPosition;at.y=Mathf.Clamp(at.y+dy,0,Mathf.Max(0,profileContent.rect.height-profileViewport.rect.height));profileContent.anchoredPosition=at;
        }
        void ScrollProfile(float distance){var at=profileContent.anchoredPosition;at.y=Mathf.Clamp(at.y+distance,0,Mathf.Max(0,profileContent.rect.height-profileViewport.rect.height));profileContent.anchoredPosition=at;}
        void BuildBiomeNotice(Transform parent)
        {
            var panel=Box(parent,"Biome entry",-24,-110,490,152,new Vector2(1,1),.9f);
            biomeGroup=panel.gameObject.AddComponent<CanvasGroup>();biomeGroup.blocksRaycasts=false;biomeGroup.alpha=0;
            biomeText=Text(panel,"",14,-10,462,132,23);biomeText.name="Biome stat notice";
        }
        void UpdateBiomeNotice()
        {
            var a=session.Player;
            BiomeReadout.Tick(session.Generations.Archive,a.Vitals.Dead?default:a.Life.Id,a.Stats,a.transform.position,session.Paused,Time.deltaTime);
            biomeGroup.alpha=BiomeReadout.Alpha;biomeText.text=BiomeReadout.Text;
        }
    }
}
