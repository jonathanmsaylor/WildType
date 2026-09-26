using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace WildType
{
    public sealed partial class StageHud
    {
        GameObject profilePanel;CreatureId profileId;Vector2 savedTreePosition;CreatureId savedTreeId;
        readonly Button[] profileCards=new Button[5];readonly JournalHelpTarget[] profileHelp=new JournalHelpTarget[5];
        TMP_Text profileExplanation,biomeText;CanvasGroup biomeGroup;Button profileLocate,profileControl;
        public readonly BiomeEntryReadout BiomeReadout=new BiomeEntryReadout();
        public CreatureId ProfileId=>profileId;
        public void ShowProfile(CreatureId id)
        {
            if(session.Generations.Archive.Get(id)==null)return;
            if(View==JournalView.Chronicle){savedTreePosition=treeContent.anchoredPosition;savedTreeId=id;treeId=id;}
            profileId=id;Show(JournalView.Profile);profileExplanation.text="Hover or focus a card for its practical meaning and precise values.";
        }
        public void BackToTree()
        {
            Show(JournalView.Chronicle);pendingCenter=default;
            treeId=savedTreeId.IsValid?savedTreeId:session.Player.Life.Id;treeContent.anchoredPosition=savedTreePosition;
        }
        void BuildProfile(RectTransform panel)
        {
            var area=Box(panel,"Creature profile",348,-130,1184,740,new Vector2(0,1),0);profilePanel=area.gameObject;
            ButtonAt(area,"Back to Family Tree",20,0,550,48,BackToTree,23);
            ButtonAt(area,"Profile Details",590,0,572,48,()=>ShowDetails(profileId),23,"Details · Exact Genes first");
            float[] y={60,184,282,380,478},h={116,90,90,90,74};
            for(int i=0;i<5;i++){
                var b=ButtonAt(area,"Profile card "+i,20,y[i],1142,h[i],()=>{},22);profileCards[i]=b;
                b.GetComponent<Image>().color=CardColors[i%4];var t=b.GetComponentInChildren<TMP_Text>();t.alignment=TextAlignmentOptions.TopLeft;
                var help=b.gameObject.AddComponent<JournalHelpTarget>();profileHelp[i]=help;
                help.Show=target=>profileExplanation.text=target.Explanation;help.Hide=_=>{};b.onClick.AddListener(help.Reveal);
            }
            profileLocate=ButtonAt(area,"Profile Highlight",20,564,550,48,()=>session.Locator.Select(ProfileActor()),23,"Highlight in world");
            profileControl=ButtonAt(area,"Profile Take Control",590,564,572,48,()=>session.TakeControl(ProfileActor()),23,"Take Control");
            profileExplanation=Text(Box(area,"Profile explanation",20,-624,1142,112,new Vector2(0,1),.96f),"",14,-6,1114,100,21);
            profileExplanation.name="Profile explanation text";
            Help(profileLocate,"Briefly highlight this relative, keeping control of your own creature.");
            Help(profileControl,"Continue as this living descendant with its own age and resources. Not available for siblings or ancestors.");
        }
        CreatureAgent ProfileActor(){var r=archive?.Get(profileId);return r==null?null:LineageChronicle.LivingActor(session,r);}
        void RefreshProfile()
        {
            var r=archive.Get(profileId);if(r==null){BackToTree();return;}var a=ProfileActor();
            bool dead=archive.TryDeath(profileId,out var death);
            string state=dead?"Deceased · "+LineageChronicle.Cause(death.Cause):a?(a.Life.Adult?"Living adult":"Living juvenile") :"Unavailable · no recorded death";
            string place=a?session.World.Zone(a.transform.position):"Location unavailable";
            string parents=r.Generation==0?"Parents not recorded (founder)":"Parents: "+session.Names.PersonalName(r.FirstParentId)+" + "+session.Names.PersonalName(r.SecondParentId);
            SetButtonText(profileCards[0],"<b>"+session.Names.PersonalName(profileId)+"</b> · Gen "+r.Generation+"\n"+state+" · "+place+"\n"+parents);
            SetButtonText(profileCards[1],"<b>Movement</b> · "+CreatureProfileText.Movement(session,r,a,false));
            SetButtonText(profileCards[2],"<b>Finding Food</b>\n"+(a?"Food-search range: "+JournalReadout.Whole(a.Stats.Vision)+" m · Food energy per bite: "+CreatureProfileText.N(a.Stats.NutritionFactor)+"×\nWider searches reach more food; vision also costs maintenance energy.":"Current foraging traits unavailable. Recorded inheritance remains in Details."));
            SetButtonText(profileCards[3],"<b>Survival</b>\n"+(a?"Energy reserve: "+JournalReadout.Whole(a.Stats.MaxEnergy)+" · Sprint stamina: "+JournalReadout.Whole(a.Stats.MaxStamina)+"\nInherited lifespan: "+JournalReadout.Whole(a.Stats.LifespanSeconds)+" s · Hunger can end life sooner.":dead?"Recorded age at death: "+JournalReadout.Whole(r.AgeAt(death.Time))+" s · "+LineageChronicle.Cause(death.Cause):"No death recorded. Current resources and lifespan are unavailable."));
            SetButtonText(profileCards[4],"<b>Family</b> · "+LineageChronicle.Relationship(archive,session.Player.Life.Id,r)+"\n"+CreatureProfileText.Family(session,profileId));
            profileHelp[0].Explanation=session.Names.PersonalName(profileId)+" · "+parents+". Full identity and recorded death evidence are in Details. Missing actors are not assumed dead.";
            profileHelp[1].Explanation=CreatureProfileText.Movement(session,r,a,true);
            profileHelp[2].Explanation=a?"Vision sets the autonomous food-search radius, not camera zoom or eating reach. Vision: "+JournalReadout.Exact(a.Stats.Vision)+" m. Food multiplier: "+JournalReadout.Exact(a.Stats.NutritionFactor)+". Regions change each plant's nutrition and regrowth, not these genes.":"The archive has no complete current genome. See Details for retained birth comparisons and mutations.";
            profileHelp[3].Explanation=a?"Reserve stores food; stamina fuels sprinting. Large bodies usually store more but cost more energy. Exact reserve "+JournalReadout.Exact(a.Stats.MaxEnergy)+", stamina "+JournalReadout.Exact(a.Stats.MaxStamina)+", idle drain "+JournalReadout.Exact(a.Stats.PassiveDrain)+" energy/s, lifespan "+JournalReadout.Exact(a.Stats.LifespanSeconds)+" s. Juvenile movement is slower; the movement comparison uses adult values.":"Only actual death records support a cause or age at death. No inherited lifespan is reconstructed from an incomplete archive.";
            profileHelp[4].Explanation="Children counts use recorded parent IDs. Unavailable means no living registered actor and no recorded death. Highlight keeps your control; Take Control only offers living descendants. Care remains limited to eligible direct children.";
            profileLocate.gameObject.SetActive(a&&session.CanLocateFamily(a)&&!session.GameOver);
            profileControl.gameObject.SetActive(a&&session.Generations.IsLivingDescendant(a,session.Player.Life.Id));
        }
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
