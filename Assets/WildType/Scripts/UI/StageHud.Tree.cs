using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace WildType
{
    public sealed partial class StageHud
    {
        public const int TreePoolCapacity=24;
        GameObject treePanel; CreatureId treeId;
        Button treeLocate,treeControl,treeEnter;
        TMP_Text treeHeading,treeHint,treeSelection;
        ScrollRect treeScroll; RectTransform treeContent,treeViewport;
        FamilyTreeLinks treeLinks;
        readonly FamilyTreeLayout treeLayout=new FamilyTreeLayout();
        readonly List<CreatureLineageRecord> treeIndex=new List<CreatureLineageRecord>(LineageArchive.Capacity);
        readonly List<FamilyTreeCard> treePool=new List<FamilyTreeCard>();
        readonly Dictionary<FamilyTreeCard,ResourceBars> treeBars=new Dictionary<FamilyTreeCard,ResourceBars>();
        LineageArchive treeArchive; int treeRevision=-1;CreatureId treeController;
        bool bindingTree;CreatureId pendingCenter;
        public CreatureId TreeFocus=>treeId;
        public FamilyTreeLayout TreeLayout=>treeLayout;
        public ScrollRect TreeScroll=>treeScroll;
        public void ShowTree(CreatureId id){if(session.Generations.Archive.Get(id)==null)return;treeId=id;pendingCenter=id;Show(JournalView.Chronicle);}
        public void ShowIdentity(CreatureId id){ShowProfile(id);sections[2]=true;RefreshProfile();Select(sectionButtons[2]);EnsureProfileVisible(sectionButtons[2].transform as RectTransform);}
        void BuildFamilyTree(RectTransform panel)
        {
            var area=Box(panel,"Family Tree",348,-130,1184,740,new Vector2(0,1),0);treePanel=area.gameObject;
            treeHeading=Text(area,"",20,0,1142,68,24);
            treeEnter=livingTab;
            ButtonAt(area,"Living jump",20,76,490,44,JumpLiving,21,"Find living descendant");
            ButtonAt(area,"My creature",522,76,222,44,()=>CenterTree(session.Player.Life.Id,true),21);
            ButtonAt(area,"Tree events",756,76,406,44,()=>Show(JournalView.Events),21,"Recent births / deaths");
            treeViewport=Box(area,"Tree viewport",20,-132,1116,412,new Vector2(0,1),.58f);treeViewport.gameObject.AddComponent<RectMask2D>();
            treeScroll=treeViewport.gameObject.AddComponent<ScrollRect>();treeScroll.viewport=treeViewport;
            treeScroll.movementType=ScrollRect.MovementType.Clamped;treeScroll.inertia=false;treeScroll.scrollSensitivity=16;
            treeContent=Box(treeViewport,"Tree canvas",0,0,1116,412,new Vector2(0,1),0);treeScroll.content=treeContent;
            var edgeObject=new GameObject("Recorded parent links",typeof(RectTransform),typeof(CanvasRenderer),typeof(FamilyTreeLinks));edgeObject.transform.SetParent(treeContent,false);
            treeLinks=edgeObject.GetComponent<FamilyTreeLinks>();treeLinks.raycastTarget=false;treeLinks.rectTransform.anchorMin=treeLinks.rectTransform.anchorMax=treeLinks.rectTransform.pivot=new Vector2(0,1);
            treeLinks.rectTransform.anchoredPosition=Vector2.zero;treeLinks.Layout=treeLayout;
            for(int i=0;i<TreePoolCapacity;i++){
                var button=ButtonAt(treeContent,"Tree record",0,0,FamilyTreeLayout.CardWidth,FamilyTreeLayout.CardHeight,()=>{},20);
                navigation.Remove(button);button.navigation=new Navigation{mode=Navigation.Mode.None};button.transition=Selectable.Transition.None;
                var node=button.gameObject.AddComponent<FamilyTreeCard>();node.Navigate=NavigateTree;node.Focused=c=>{treeId=c.Id;UpdateTreeActions();};node.Explain=ExplainTreeCard;
                button.onClick.AddListener(()=>ShowProfile(node.Id));var text=button.GetComponentInChildren<TMP_Text>();text.alignment=TextAlignmentOptions.TopLeft;
                text.rectTransform.offsetMin=new Vector2(14,-140);text.rectTransform.offsetMax=new Vector2(FamilyTreeLayout.CardWidth-14,-8);
                treeBars.Add(node,MakeResourceBars(button.transform,14,146,FamilyTreeLayout.CardWidth-28,31,19));
                treePool.Add(node);button.gameObject.SetActive(false);
            }
            var horizontal=TreeBar(area,"Tree horizontal",20,552,1116,16,false);var vertical=TreeBar(area,"Tree vertical",1144,132,18,412,true);
            treeScroll.horizontalScrollbar=horizontal;treeScroll.verticalScrollbar=vertical;
            treeScroll.onValueChanged.AddListener(_=>BindTree());
            treeSelection=Text(area,"",20,-576,1140,34,22);
            treeLocate=ButtonAt(area,"Tree Highlight",20,616,238,46,()=>session.Locator.Select(TreeActor()),22,"Highlight in world");
            treeControl=ButtonAt(area,"Tree Take control",270,616,240,46,()=>session.TakeControl(TreeActor()),22,"Take Control");
            ButtonAt(area,"Tree full record",522,616,222,46,()=>ShowIdentity(treeId),22,"Full record");
            ButtonAt(area,"Tree index",756,616,406,46,()=>Show(JournalView.Records),22,"All Family Records");
            treeHint=Text(area,"",20,-674,1142,62,21);
            Help(treeLocate,"Resume as your current creature and briefly highlight this relative in the world. No walking, teleporting or control transfer.");
            Help(treeControl,"Continue playing as this living descendant with its own existing age and resources. Only descendants of your current creature are eligible.");
        }
        Scrollbar TreeBar(RectTransform parent,string name,float x,float y,float w,float h,bool vertical)
        {
            var track=Box(parent,name,x,-y,w,h,new Vector2(0,1),.75f);var bar=track.gameObject.AddComponent<Scrollbar>();
            var handle=Box(track,"Handle",0,0,w,h,new Vector2(0,1),1);handle.GetComponent<Image>().color=new Color(.7f,.65f,.4f);
            handle.anchorMin=Vector2.zero;handle.anchorMax=Vector2.one;handle.offsetMin=handle.offsetMax=Vector2.zero;
            bar.handleRect=handle;bar.targetGraphic=handle.GetComponent<Image>();bar.direction=vertical?Scrollbar.Direction.BottomToTop:Scrollbar.Direction.LeftToRight;bar.navigation=new Navigation{mode=Navigation.Mode.None};return bar;
        }
        CreatureAgent TreeActor(){var record=archive?.Get(treeId);return record==null?null:LineageChronicle.LivingActor(session,record);}
        void RefreshFamilyTree()
        {
            if(treeArchive!=archive||treeRevision!=archive.Revision||treeController!=session.Player.Life.Id){
                treeArchive=archive;treeRevision=archive.Revision;treeController=session.Player.Life.Id;
                session.CollectFamilyHistory(treeIndex);treeLayout.Build(archive,treeIndex);treeContent.sizeDelta=treeLayout.Size;treeLinks.rectTransform.sizeDelta=treeLayout.Size;
                if(!treeLayout.Positions.ContainsKey(treeId))treeId=treeController;pendingCenter=treeId;
            }
            treeHeading.text="Family Tree · "+treeLayout.Records.Count+" recorded relatives\nLive bars show current needs · Earlier branches stay here.";
            if(pendingCenter.IsValid){var id=pendingCenter;pendingCenter=default;CenterTree(id,false);}else BindTree();
            UpdateTreeActions();
        }
        void UpdateTreeActions()
        {
            if(!treeSelection||archive==null)return;
            var actor=TreeActor();treeLocate.gameObject.SetActive(actor&&session.CanLocateFamily(actor)&&!session.GameOver);
            treeControl.gameObject.SetActive(actor&&session.Generations.IsLivingDescendant(actor,session.Player.Life.Id));
            treeSelection.text="Selected: "+session.Names.PersonalName(treeId)+" · Select card for its profile";
            treeLinks.Selected=treeId;treeLinks.SetVerticesDirty();
        }
        public void CenterTree(CreatureId id,bool focus)
        {
            if(!treeLayout.Positions.TryGetValue(id,out var at))return;treeId=id;
            var view=treeViewport.rect.size;treeContent.anchoredPosition=new Vector2(-Mathf.Clamp(at.x+FamilyTreeLayout.CardWidth/2-view.x/2,0,Mathf.Max(0,treeLayout.Size.x-view.x)),Mathf.Clamp(at.y-12,0,Mathf.Max(0,treeLayout.Size.y-view.y)));
            BindTree();UpdateTreeActions();if(focus)foreach(var node in treePool)if(node.gameObject.activeSelf&&node.Id==id){EventSystem.current.SetSelectedGameObject(node.gameObject);break;}
        }
        public void NavigateTree(CreatureId id,int dx,int dy)
        {
            var next=treeLayout.Move(id,dx,dy);if(next!=id)CenterTree(next,true);
            else if(dx<0)Select(livingTab);else if(dy>0)Select(treeControl.gameObject.activeSelf?treeControl:treeEnter);
        }
        void JumpLiving()
        {
            int start=treeLayout.Records.FindIndex(r=>r.CreatureId==treeId);
            for(int i=1;i<=treeLayout.Records.Count;i++){var r=treeLayout.Records[(Mathf.Max(0,start)+i)%treeLayout.Records.Count];var a=LineageChronicle.LivingActor(session,r);
                if(a&&session.Generations.IsLivingDescendant(a,session.Player.Life.Id)){CenterTree(r.CreatureId,true);return;}}
            treeHint.text="No living descendants of your current creature. Earlier relatives remain in the tree.";
        }
        void BindTree()
        {
            if(bindingTree||!treePanel.activeInHierarchy||archive==null)return;bindingTree=true;
            var view=new Rect(-treeContent.anchoredPosition.x,treeContent.anchoredPosition.y,treeViewport.rect.width,treeViewport.rect.height);
            var visible=new List<CreatureLineageRecord>();foreach(var r in treeLayout.Records)if(treeLayout.Bounds(r.CreatureId).Overlaps(view))visible.Add(r);
            // Keep existing visible identities in their pool slots, avoiding hover/focus flicker during scrolling.
            var used=new HashSet<CreatureId>();foreach(var n in treePool)if(n.gameObject.activeSelf&&visible.Exists(r=>r.CreatureId==n.Id))used.Add(n.Id);else {
                // A recycled visual must never retain input focus for a different identity.
                if(EventSystem.current&&EventSystem.current.currentSelectedGameObject==n.gameObject)EventSystem.current.SetSelectedGameObject(treeEnter.gameObject);
                n.gameObject.SetActive(false);n.Id=default;n.ResetState();
            }
            foreach(var r in visible){if(used.Contains(r.CreatureId))continue;var slot=treePool.Find(n=>!n.gameObject.activeSelf);if(slot==null)break;slot.Id=r.CreatureId;slot.gameObject.SetActive(true);used.Add(r.CreatureId);}
            foreach(var n in treePool){if(!n.gameObject.activeSelf)continue;var r=archive.Get(n.Id);var at=treeLayout.Positions[n.Id];n.GetComponent<RectTransform>().anchoredPosition=new Vector2(at.x,-at.y);
                var actor=LineageChronicle.LivingActor(session,r);bool descendant=actor&&session.Generations.IsLivingDescendant(actor,session.Player.Life.Id);
                string state=archive.TryDeath(r.CreatureId,out _)?"Deceased":actor?(actor.Life.Adult?"Living · Adult":"Living · Juvenile"):"Unavailable";
                n.BaseColor=descendant?new Color(.20f,.37f,.27f,1):new Color(.19f,.25f,.27f,1);n.Paint();
                string fullName=session.Names.PersonalName(n.Id);string compact=fullName.Length>22?fullName.Substring(0,21)+"…":fullName;
                SetButtonText(n.GetComponent<Button>(),"<b>"+compact+"</b>\nGen "+r.Generation+" · "+LineageChronicle.Relationship(archive,session.Player.Life.Id,r)+"\n"+state+(descendant?"\n<size=18>"+InheritedChoices.Callout(session,r,actor)+"</size>":""));
                treeBars[n].Read(actor);
            }
            treeLinks.Visible=view;treeLinks.SetVerticesDirty();bindingTree=false;
            if(string.IsNullOrEmpty(treeHint.text))treeHint.text="Scroll or drag empty space. Arrows / D-pad move between cards; left at the edge returns to the menu. Lines link recorded parents only.";
        }
        void ExplainTreeCard(FamilyTreeCard node)
        {
            var r=archive.Get(node.Id);if(r==null)return;treeLinks.Selected=node.Id;treeLinks.SetVerticesDirty();
            var actor=LineageChronicle.LivingActor(session,r);
            string need=actor ? (actor.Vitals.Energy<=.001f?"Starving":actor.Vitals.Energy<=actor.Stats.MaxEnergy*.25f?"Hungry":"") : "";
            if(actor&&actor.Vitals.Health<60)need+=(need.Length>0?" · ":"")+"Low health";
            treeHint.text=session.Names.PersonalName(node.Id)+(need.Length>0?" · "+need:"")+"\n"+(r.Generation==0?"No parents recorded. Select to read the profile.":"Parents: "+session.Names.PersonalName(r.FirstParentId)+" + "+session.Names.PersonalName(r.SecondParentId)+".");
        }
    }
}
