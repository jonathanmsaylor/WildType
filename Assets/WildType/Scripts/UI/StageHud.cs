using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;
namespace WildType
{
    public sealed class StageHud : MonoBehaviour
    {
        public enum JournalView { Living, Chronicle, Details, Guide, Events }
        public JournalView View { get; private set; }
        public bool TipsVisible { get; private set; } = true;
        StageSession session; PlayerInputBridge input;
        GameObject ordinary, pausePanel, namePanel, cardsPanel, detailsPanel, guidePanel, eventsPanel;
        TMP_Text identity, region, status, notice, care, tips, briefEvents, title, scope, detailLeft, detailRight, guide, turnover, namePreview;
        TMP_InputField nameInput; CreatureId namingId, focusId, detailId;
        readonly TMP_Text[] resources = new TMP_Text[3]; readonly Image[] bars = new Image[3];
        readonly RectTransform[] rows = new RectTransform[3]; readonly TMP_Text[] labels = new TMP_Text[3];
        readonly Button[] control = new Button[3], locate = new Button[3], inspect = new Button[3];
        readonly CreatureAgent[] choices = new CreatureAgent[3], located = new CreatureAgent[3];
        readonly CreatureId[] rowIds = new CreatureId[3];
        readonly List<CreatureLineageRecord> records = new List<CreatureLineageRecord>(LineageArchive.Capacity);
        readonly List<Button> navigation = new List<Button>();
        LineageArchive archive; Button resume, nextPage, previousPage, detailPage, tipsButton, livingTab;
        int page, detailSection; float refresh; FoodPlant highlighted;
        bool ate, born, completed; Vector3 startingPoint;
        readonly StringBuilder builder = new StringBuilder(1600);
        public void Configure(StageSession value)
        {
            session=value; input=GetComponent<PlayerInputBridge>();
            var root=new GameObject("WildType HUD",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));root.transform.SetParent(transform,false);
            root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scale=root.GetComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1920,1080);scale.matchWidthOrHeight=1;
            root.AddComponent<FamilyWorldCues>().Configure(session,root.GetComponent<RectTransform>());
            ordinary=new GameObject("Ordinary play",typeof(RectTransform));ordinary.transform.SetParent(root.transform,false);
            var stretch=ordinary.GetComponent<RectTransform>();stretch.anchorMin=Vector2.zero;stretch.anchorMax=Vector2.one;stretch.offsetMin=stretch.offsetMax=Vector2.zero;
            var survival=Box(ordinary.transform,"Survival",24,-24,340,235,new Vector2(0,1),.66f);
            Text(survival,"W I L D T Y P E",18,-12,304,34,26);
            string[] names={"Energy","Health","Stamina"};Color[] colors={new Color(.68f,.85f,.39f),new Color(.96f,.65f,.49f),new Color(.45f,.8f,.92f)};
            for(int i=0;i<3;i++){
                resources[i]=Text(survival,"",18,-53-i*56,304,32,24);resources[i].name=names[i]+" value";
                var track=Box(survival,names[i]+" track",18,-89-i*56,304,8,new Vector2(0,1),.85f);
                bars[i]=Box(track,names[i]+" fill",0,0,304,8,new Vector2(0,1),1).GetComponent<Image>();bars[i].color=colors[i];
            }
            var family=Box(ordinary.transform,"Lineage",24,-271,340,150,new Vector2(0,1),.52f);
            identity=Text(family,"",18,-13,304,125,24);identity.name="Family identity";
            region=Text(ordinary.transform,"",-24,-26,590,72,24,new Vector2(1,1));region.alignment=TextAlignmentOptions.Right;Shade(region);
            status=Banner(ordinary.transform,"Survival warning",0,-130,890,115,new Vector2(.5f,1));
            notice=Banner(ordinary.transform,"Action feedback",0,-32,870,85,new Vector2(.5f,1));
            care=Banner(ordinary.transform,"Care cost",0,190,820,86,new Vector2(.5f,0));
            tips=Banner(ordinary.transform,"First steps",24,24,645,100,new Vector2(0,0));tips.alignment=TextAlignmentOptions.Left;
            briefEvents=Banner(ordinary.transform,"Recent lives",-24,24,560,145,new Vector2(1,0));briefEvents.alignment=TextAlignmentOptions.Left;briefEvents.fontSize=22;
            BuildJournal(root.transform);BuildNaming(root.transform);
            var events=new GameObject("UI input",typeof(EventSystem),typeof(InputSystemUIInputModule));events.transform.SetParent(root.transform);
            events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
        void BuildJournal(Transform root)
        {
            var panel=Box(root,"Pause",0,0,1560,900,new Vector2(.5f,.5f),.94f);pausePanel=panel.gameObject;
            title=Text(panel,"FAMILY JOURNAL\nSimulation paused",28,-24,1460,90,28);title.name="Journal title";
            resume=ButtonAt(panel,"Resume",28,134,292,56,()=>session.SetPaused(false));
            livingTab=ButtonAt(panel,"Living descendants",28,202,292,56,()=>Show(JournalView.Living));
            ButtonAt(panel,"Chronicle",28,270,292,56,()=>Show(JournalView.Chronicle));
            ButtonAt(panel,"Details",28,338,292,56,()=>ShowDetails(session.Player.Life.Id));
            ButtonAt(panel,"Recent births / deaths",28,406,292,56,()=>Show(JournalView.Events));
            ButtonAt(panel,"Getting started",28,474,292,56,()=>Show(JournalView.Guide));
            tipsButton=ButtonAt(panel,"Hide tips",28,542,292,56,()=>{TipsVisible=!(TipsVisible&&!completed);completed=false;refresh=0;});
            ButtonAt(panel,"Restart same seed",28,634,292,56,()=>session.Restart(false));
            ButtonAt(panel,"Reseed new run",28,702,292,56,()=>session.Restart(true));
            ButtonAt(panel,"Quit",28,770,292,56,session.Quit);
            Text(panel,"Restart / reseed clears this run's family history.",28,-836,292,54,20);
            var cardArea=Box(panel,"Descendants",348,-130,1184,740,new Vector2(0,1),0);cardsPanel=cardArea.gameObject;
            scope=Text(cardArea,"",16,-4,1138,130,25);scope.name="Family scope";
            for(int i=0;i<3;i++){
                int slot=i;rows[i]=Box(cardArea,"Relative card",16,-145-i*164,1140,152,new Vector2(0,1),.5f);
                labels[i]=Text(rows[i],"",18,-10,754,130,23);labels[i].name="Relative identity";
                // Cards are text, never a hidden control-transfer button.
                locate[i]=ButtonAt(rows[i],"Locate",798,8,320,40,()=>session.Locator.Select(located[slot]),22,"Highlight · Locate");
                control[i]=ButtonAt(rows[i],"Take control",798,55,320,40,()=>{if(session.TakeControl(choices[slot])){page=0;refresh=0;}},22);
                inspect[i]=ButtonAt(rows[i],"Relative details",798,102,320,40,()=>ShowDetails(rowIds[slot]),22,"Details");
            }
            previousPage=ButtonAt(cardArea,"Previous page",16,654,240,52,()=>{page=Mathf.Max(0,page-1);refresh=0;});
            nextPage=ButtonAt(cardArea,"Next page",270,654,240,52,()=>{page++;refresh=0;});
            Text(cardArea,"Highlight resumes play; you stay in your creature.\nTake control switches creatures; age and resources stay.",530,-646,630,86,23);
            var detailArea=Box(panel,"Details view",348,-130,1184,740,new Vector2(0,1),0);detailsPanel=detailArea.gameObject;
            detailLeft=Text(detailArea,"",20,-8,552,645,23);detailLeft.name="Inherited traits";detailLeft.lineSpacing=3;
            detailRight=Text(detailArea,"",600,-8,552,645,23);detailRight.name="Exact traits";detailRight.lineSpacing=3;
            detailPage=ButtonAt(detailArea,"More details",20,678,480,52,()=>{detailSection=(detailSection+1)%DetailPages();refresh=0;});
            var guideArea=Box(panel,"Getting started view",348,-130,1184,740,new Vector2(0,1),0);guidePanel=guideArea.gameObject;
            guide=Text(guideArea,"",20,-8,1144,664,25);
            ButtonAt(guideArea,"Skip / return to family",20,678,440,52,()=>{TipsVisible=false;Show(JournalView.Living);});
            var eventArea=Box(panel,"Recent history",348,-130,1184,740,new Vector2(0,1),0);eventsPanel=eventArea.gameObject;
            turnover=Text(eventArea,"",20,-8,1134,710,25);turnover.name="Turnover events";turnover.lineSpacing=7;
            pausePanel.SetActive(false);
        }
        public void Show(JournalView view)
        {
            View=view;page=0;refresh=0;
            if(session.Ready&&!session.Paused)session.SetPaused(true);
        }
        public void ShowDetails(CreatureId id){detailId=id;detailSection=0;Show(JournalView.Details);}
        void Update()
        {
            if(!session||!session.Ready||!session.Player)return;
            session.Names.ValidatePrompt();bool naming=session.Names.HasPrompt;namePanel.SetActive(naming);
            if(naming&&namingId!=session.Names.Pending){namingId=session.Names.Pending;nameInput.text="";namePreview.text="Keep default: "+session.Names.PersonalName(namingId);nameInput.Select();nameInput.ActivateInputField();}
            if(!naming)namingId=default;
            bool opening=session.Paused&&!naming&&!pausePanel.activeSelf;
            pausePanel.SetActive(session.Paused&&!naming);ordinary.SetActive(!session.Paused);
            if(opening){refresh=0;if(born)completed=true;}
            refresh-=Time.unscaledDeltaTime;if(refresh>0)return;refresh=.1f;
            var a=session.Player;var v=a.Vitals;var loop=session.Generations;
            if(archive!=loop.Archive){archive=loop.Archive;View=JournalView.Living;page=0;detailSection=0;detailId=default;ate=born=completed=false;TipsVisible=true;startingPoint=a.transform.position;records.Clear();}
            if(focusId!=a.Life.Id){focusId=a.Life.Id;page=0;View=JournalView.Living;detailId=focusId;}
            var record=archive.Get(focusId);ate|=a.Interaction.MealsEaten>0;born|=record.OffspringCount>0;
            float[] values={v.Energy,v.Health,v.Stamina},maximum={a.Stats.MaxEnergy,100,a.Stats.MaxStamina};string[] names={"Energy","Health","Stamina"};
            for(int i=0;i<3;i++){resources[i].text=$"{names[i]}  {values[i]:0}/{maximum[i]:0}";bars[i].rectTransform.localScale=new Vector3(Mathf.Clamp01(values[i]/maximum[i]),1,1);}
            identity.text=$"<b>{session.Names.PersonalName(focusId)}</b>\n{GenerationLoop.ShortId(focusId)} · Gen {record.Generation} · {(a.Life.Adult?"Adult":"Juvenile")}\n{loop.LivingChildren(focusId)} living children\n{(input.UsingGamepad?"North":input.JournalKey)} · Family journal";
            region.text=session.World.Zone(a.transform.position)+$"\nPopulation {session.Population}/24 · Births {loop.Births} · Deaths {loop.Deaths}";
            SetBanner(status,GameplayText.Status(v.Energy,a.Stats.MaxEnergy,v.Health,v.SprintLocked));
            SetBanner(notice,session.CurrentNotice);
            SetBanner(care,GameplayText.Care(session,a,session.Care.NearbyChild(a),input.CareKey));
            string step=born?$"New child! {input.JournalKey} → family journal.\nHighlight finds it; Take control switches to it.":ate?$"Find a well-fed adult partner.\n{input.MateKey} tries mating; failures explain why.":Vector3.Distance(startingPoint,a.transform.position)>3?$"Find ripe fruit; {input.EatKey} eats when close.\nFood restores energy, not health.":input.UsingGamepad?"Move: left stick · Look: right stick\nNorth → Getting started / Hide tips (pauses).":"Move: W A S D · Look: mouse\nTab → Getting started / Hide tips (pauses).";
            SetBanner(tips,TipsVisible&&!completed?step:"");RefreshTurnover();
            if(highlighted)highlighted.Highlight(false);highlighted=a.Interaction.Nearest();if(highlighted&&!session.Paused&&a.Interaction.EatReason(highlighted).Length==0)highlighted.Highlight(true);
            if(session.Paused&&!naming){RefreshJournal();if(opening)Select(session.GameOver?livingTab:resume);RepairNavigation();}
        }
        void RefreshJournal()
        {
            var a=session.Player;var living=session.Generations.LivingDescendants(a.Life.Id);
            title.text="FAMILY JOURNAL · Simulation paused — no energy is spent while reading";
            if(session.GameOver){archive.TryDeath(a.Life.Id,out var death);bool known=archive.TryDeath(a.Life.Id,out _);title.text=$"LIFE ENDED · {session.Names.PersonalName(a.Life.Id)} · {(known?LineageChronicle.Cause(death.Cause):"cause unknown")}\n"+(living.Count==0?"No living descendants. Restart or reseed to begin again.":"Choose a descendant with Take control, or restart. No replacement is created.");}
            resume.interactable=!session.GameOver;SetButtonText(tipsButton,TipsVisible&&!completed?"Hide first-step tips":"Show first-step tips");
            bool cards=View==JournalView.Living||View==JournalView.Chronicle;
            cardsPanel.SetActive(cards);detailsPanel.SetActive(View==JournalView.Details);guidePanel.SetActive(View==JournalView.Guide);eventsPanel.SetActive(View==JournalView.Events);
            guide.text=GameplayText.Guide(input.UsingGamepad);
            if(View==JournalView.Details){RefreshDetails();return;}if(!cards)return;
            records.Clear();if(View==JournalView.Chronicle)session.CollectFamilyHistory(records);else foreach(var actor in living)records.Add(archive.Get(actor.Life.Id));
            int pages=Mathf.Max(1,Mathf.CeilToInt(records.Count/3f));page=Mathf.Clamp(page,0,pages-1);
            scope.text=View==JournalView.Chronicle?$"Family chronicle · page {page+1}/{pages}\nRelationships to {session.Names.Label(a.Life.Id)} (you).\nIncludes earlier controlled branches. Siblings: highlight only, no care or control.":$"Living descendants of {session.Names.Label(a.Life.Id)}\n{living.Count} living · page {page+1}/{pages}. Children, grandchildren and later descendants.\nAfter a control switch this list follows your new creature; other family stays in Chronicle.";
            previousPage.interactable=page>0;nextPage.interactable=page<pages-1;
            for(int i=0;i<3;i++){
                int index=page*3+i;bool shown=index<records.Count;rows[i].gameObject.SetActive(shown);choices[i]=located[i]=null;rowIds[i]=default;if(!shown)continue;
                var entry=records[index];var actor=LineageChronicle.LivingActor(session,entry);rowIds[i]=entry.CreatureId;
                bool canControl=actor&&session.Generations.IsLivingDescendant(actor,a.Life.Id);bool canLocate=actor&&session.CanLocateFamily(actor)&&!session.GameOver;
                choices[i]=canControl?actor:null;located[i]=canLocate?actor:null;
                control[i].interactable=canControl;locate[i].gameObject.SetActive(canLocate);locate[i].interactable=canLocate;
                labels[i].text=LineageChronicle.Card(session,entry,actor,canControl);
            }
        }
        int DetailPages(){var record=archive.Get(detailId);int mutations=record?.ImportantMutations?.Length??0;return 3+Mathf.CeilToInt(Mathf.Max(0,mutations-6)/6f);}
        void RefreshDetails()
        {
            var entry=archive.Get(detailId)??archive.Get(session.Player.Life.Id);var actor=LineageChronicle.LivingActor(session,entry);
            string age=actor?$"Age {actor.Life.Age:0}s":archive.TryDeath(entry.CreatureId,out var d)?$"Lived {entry.AgeAt(d.Time):0}s · {LineageChronicle.Cause(d.Cause)}":"Age unavailable · no recorded death";
            string heading=$"<b>{session.Names.PersonalName(entry.CreatureId)}</b> · {GenerationLoop.ShortId(entry.CreatureId)} · Gen {entry.Generation}\n"+LineageChronicle.Relationship(archive,session.Player.Life.Id,entry)+" · "+age+"\n\n";
            if(detailSection==0){detailLeft.text=heading+(actor?GameplayText.Build(actor.Genome)+$"\nEnergy {actor.Vitals.Energy:0.0}/{actor.Stats.MaxEnergy:0.0} · Health {actor.Vitals.Health:0.0}/100\nStamina {actor.Vitals.Stamina:0.0}/{actor.Stats.MaxStamina:0.0}\n{session.World.Zone(actor.transform.position)} · {Vector3.Distance(session.Player.transform.position,actor.transform.position):0} m away\n\n"+GameplayText.Tradeoffs:"Actor unavailable. No living stats inferred.\nUse recorded inheritance on the next page.");detailRight.text=actor?GameplayText.Numbers(actor):"No current resource or movement values available.";}
            if(detailSection==1){detailLeft.text=heading+archive.Changes(entry.CreatureId);detailRight.text=GameplayText.Mutations(entry.ImportantMutations,0,6)+"\n\nParent A: "+session.Names.Label(entry.FirstParentId)+"\nParent B: "+session.Names.Label(entry.SecondParentId)+"\n\nParent values are birth-time records.\nMutation is probabilistic, not a promised benefit.";}
            if(detailSection==2){detailLeft.text=heading+(actor?GameplayText.Genes(actor.Genome):"Exact current genome unavailable after actor cleanup.\nThe recorded parent comparisons remain on page 2.");detailRight.text="READING THESE NUMBERS\n\nEnergy: stored food used for living, moving, mating and care. At zero, health falls.\n\nHealth: damage is not healed by food or care. At zero, this life ends.\n\nStamina: sprinting uses it; walking or resting recovers it. It is not food.\n\nThese are different resources. No single number measures genetic success.";}
            if(detailSection>=3){detailLeft.text=heading+"Additional recorded mutations";detailRight.text=GameplayText.Mutations(entry.ImportantMutations,(detailSection-2)*6,6);}
            SetButtonText(detailPage,$"Next details page · {detailSection+1}/{DetailPages()}");
        }
        void RefreshTurnover()
        {
            builder.Clear();builder.Append("RECENT BIRTHS / DEATHS · last 4\n\n");var shortText=new StringBuilder("RECENT LIVES · journal for details\n");int n=0;
            foreach(var e in session.Generations.Turnover.Recent){
                string kind=e.Kind==TurnoverKind.Birth?"Born":"Died";string cause=e.Kind==TurnoverKind.Death?" · "+LineageChronicle.Cause(e.Cause):"";
                builder.Append(kind).Append(" · ").Append(session.Names.PersonalName(e.CreatureId)).Append(" · ").Append(GenerationLoop.ShortId(e.CreatureId)).Append(" · Gen ").Append(e.Generation).Append(cause).Append('\n');
                builder.Append("Parents ").Append(GenerationLoop.ShortId(e.FirstParentId)).Append(" + ").Append(GenerationLoop.ShortId(e.SecondParentId)).Append(" · Lineage ").Append(GenerationLoop.ShortId(e.FounderId)).Append("\n\n");
                if(n++<2)shortText.Append(kind).Append(' ').Append(session.Names.PersonalName(e.CreatureId)).Append(' ').Append(GenerationLoop.ShortId(e.CreatureId)).Append(cause).Append('\n');
            }
            if(n==0)builder.Append("No births or deaths recorded this run.");turnover.text=builder.ToString();SetBanner(briefEvents,n>0?shortText.ToString().TrimEnd():"");
        }
        void RepairNavigation()
        {
            // A bounded ordered ring works with arrows/D-pad even after paging or hiding a button.
            var active=new List<Button>();foreach(var b in navigation)if(b&&b.gameObject.activeInHierarchy&&b.interactable)active.Add(b);
            for(int i=0;i<active.Count;i++){var n=new Navigation{mode=Navigation.Mode.Explicit};n.selectOnUp=n.selectOnLeft=active[(i+active.Count-1)%active.Count];n.selectOnDown=n.selectOnRight=active[(i+1)%active.Count];active[i].navigation=n;}
            if(EventSystem.current){var selected=EventSystem.current.currentSelectedGameObject;var s=selected?selected.GetComponent<Selectable>():null;if(!s||!s.IsActive()||!s.IsInteractable())if(active.Count>0)Select(active[0]);}
        }
        static void Select(Button button){if(EventSystem.current&&button&&button.interactable)EventSystem.current.SetSelectedGameObject(button.gameObject);}
        static void SetButtonText(Button button,string value)=>button.GetComponentInChildren<TMP_Text>().text=value;
        static void SetBanner(TMP_Text text,string value){text.text=value;text.transform.parent.gameObject.SetActive(value.Length>0);}
        RectTransform Box(Transform parent,string name,float x,float y,float w,float h,Vector2 anchor,float alpha)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=anchor;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);go.GetComponent<Image>().color=new Color(.035f,.065f,.07f,alpha);return r;
        }
        TMP_Text Text(Transform parent,string value,float x,float y,float w,float h,int size,Vector2? anchor=null)
        {
            var go=new GameObject("Text",typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);var t=go.GetComponent<TextMeshProUGUI>();var r=t.rectTransform;r.anchorMin=r.anchorMax=r.pivot=anchor??new Vector2(0,1);r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);t.font=session.hudFont;t.fontSize=size;t.color=new Color(.96f,.97f,.92f);t.text=value;t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;return t;
        }
        static void Shade(TMP_Text t){var shadow=t.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.85f);shadow.effectDistance=new Vector2(1,-1);}
        TMP_Text Banner(Transform parent,string name,float x,float y,float w,float h,Vector2 anchor)
        {
            var panel=Box(parent,name,x,y,w,h,anchor,.76f);var t=Text(panel,"",14,-10,w-28,h-20,25);t.alignment=TextAlignmentOptions.Center;return t;
        }
        Button ButtonAt(Transform parent,string name,float x,float y,float w,float h,UnityEngine.Events.UnityAction action,int size=24,string label=null)
        {
            var r=Box(parent,name,x,-y,w,h,new Vector2(0,1),1);r.GetComponent<Image>().color=new Color(.17f,.27f,.27f,1);var b=r.gameObject.AddComponent<Button>();b.targetGraphic=r.GetComponent<Image>();
            var colors=b.colors;colors.highlightedColor=new Color(.75f,.83f,.68f);colors.selectedColor=new Color(.75f,.83f,.68f);colors.disabledColor=new Color(.5f,.5f,.5f);b.colors=colors;
            var t=Text(r,label??name,8,-3,w-16,h-6,size);t.alignment=TextAlignmentOptions.Center;b.onClick.AddListener(action);navigation.Add(b);return b;
        }
        void BuildNaming(Transform root)
        {
            var panel=Box(root,"Name your child",0,0,800,400,new Vector2(.5f,.5f),.94f);namePanel=panel.gameObject;
            Text(panel,"WELCOME TO THE FAMILY · Simulation paused",28,-24,744,40,27);
            Text(panel,"Name your child, or keep its default. Birth order is added:\nDave becomes Dave 1 for your first child. No energy is spent here.",28,-78,744,68,24);
            var field=Box(panel,"Child name",28,-162,744,56,new Vector2(0,1),1);nameInput=field.gameObject.AddComponent<TMP_InputField>();nameInput.targetGraphic=field.GetComponent<Image>();nameInput.characterLimit=16;
            var viewport=new GameObject("Viewport",typeof(RectTransform),typeof(RectMask2D));viewport.transform.SetParent(field,false);var view=viewport.GetComponent<RectTransform>();view.anchorMin=Vector2.zero;view.anchorMax=Vector2.one;view.offsetMin=new Vector2(12,2);view.offsetMax=new Vector2(-12,-2);
            nameInput.textViewport=view;nameInput.textComponent=Text(view,"",0,0,720,52,26);nameInput.placeholder=Text(view,"Type a name (optional; keyboard)",0,0,720,52,26);
            namePreview=Text(panel,"",28,-242,744,38,24);
            nameInput.onValueChanged.AddListener(value=>namePreview.text=FamilyNames.CleanName(value).Length==0?"Keep default: "+session.Names.PersonalName(session.Names.Pending):FamilyNames.Format(value,session.Names.BirthOrder(session.Names.Pending)));
            nameInput.onSubmit.AddListener(value=>{if(session.Names.HasPrompt)session.Names.Submit(value);});
            var confirm=ButtonAt(panel,"Name child",28,310,350,56,()=>session.Names.Submit(nameInput.text));var skip=ButtonAt(panel,"Skip",422,310,350,56,()=>session.Names.Submit(""),24,"Keep default / Skip");
            var nav=new Navigation{mode=Navigation.Mode.Explicit,selectOnDown=confirm,selectOnRight=skip};nameInput.navigation=nav;
            confirm.navigation=new Navigation{mode=Navigation.Mode.Explicit,selectOnUp=nameInput,selectOnRight=skip,selectOnDown=skip};skip.navigation=new Navigation{mode=Navigation.Mode.Explicit,selectOnUp=nameInput,selectOnLeft=confirm,selectOnDown=confirm};namePanel.SetActive(false);
        }
        void OnDestroy(){if(highlighted)highlighted.Highlight(false);}
    }
}
