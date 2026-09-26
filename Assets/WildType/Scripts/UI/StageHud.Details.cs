using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace WildType
{
    public sealed partial class StageHud
    {
        GameObject geneCards;
        TMP_Text detailHeading, helpBubble;
        JournalHelpTarget activeHelp;
        readonly Button[] geneButtons = new Button[GenomeGeneCatalog.Count];
        readonly JournalHelpTarget[] geneHelp = new JournalHelpTarget[GenomeGeneCatalog.Count];
        Button recordHelp;
        static readonly Color[] CardColors = {new Color(.20f,.29f,.35f,.97f),new Color(.22f,.33f,.25f,.97f),new Color(.38f,.29f,.21f,.97f),new Color(.33f,.25f,.36f,.97f)};
        void BuildGeneCards(RectTransform area)
        {
            geneCards = Box(area,"Exact Genes",0,0,1184,550,new Vector2(0,1),0).gameObject;
            detailHeading = Text(geneCards.transform,"",20,-4,1140,42,24);
            for(int category=0;category<4;category++)
            {
                var card=Box(geneCards.transform,GeneGuide.Title(category)+" card",20+(category%2)*580,-52-(category/2)*254,562,244,new Vector2(0,1),1);
                card.GetComponent<Image>().color=CardColors[category];
                // Layered accent strokes and a small original silhouette echo the meadow's painted palette.
                var stroke=Box(card,"Pigment edge",6,-3,548,3,new Vector2(0,1),1);stroke.GetComponent<Image>().color=new Color(.8f,.76f,.52f,.6f);
                var icon=new GameObject("Category motif",typeof(RectTransform),typeof(CanvasRenderer),typeof(JournalMotif));icon.transform.SetParent(card,false);
                var motif=icon.GetComponent<JournalMotif>();motif.Kind=category;motif.color=new Color(.94f,.84f,.54f);motif.raycastTarget=false;
                motif.rectTransform.anchorMin=motif.rectTransform.anchorMax=new Vector2(0,1);motif.rectTransform.anchoredPosition=new Vector2(30,-31);motif.rectTransform.sizeDelta=new Vector2(36,36);
                Text(card,GeneGuide.Title(category),58,-9,490,30,24);
                Text(card,GeneGuide.Summary(category),58,-37,490,26,20);
                int row=0;
                for(int i=0;i<GenomeGeneCatalog.Count;i++)
                {
                    var gene=GenomeGeneCatalog.At(i);if(GeneGuide.Category(gene)!=category)continue;
                    var button=ButtonAt(card,"Gene "+gene,14,62+row++*30,534,30,()=>{},20);
                    button.GetComponent<Image>().color=new Color(.12f,.15f,.16f,.38f);
                    var label=button.GetComponentInChildren<TMP_Text>();label.alignment=TextAlignmentOptions.Left;
                    var target=button.gameObject.AddComponent<JournalHelpTarget>();target.Show=ShowHelp;target.Hide=HideHelp;button.onClick.AddListener(target.Reveal);
                    geneButtons[i]=button;geneHelp[i]=target;
                }
            }
            helpBubble=Text(Box(area,"Explanation bubble",20,-558,1142,122,new Vector2(0,1),.98f),"",16,-8,1110,106,22);
            helpBubble.name="Gene explanation";
            recordHelp=ButtonAt(area,"Explain record",600,682,560,48,()=>ShowRecordHelp(),22,"What do these words mean?");
            var recordTarget=recordHelp.gameObject.AddComponent<JournalHelpTarget>();recordTarget.Show=_=>ShowRecordHelp();recordTarget.Hide=_=>{};
            detailLeft.rectTransform.sizeDelta=new Vector2(552,530);detailRight.rectTransform.sizeDelta=new Vector2(552,530);
        }
        void ClearHelp(){activeHelp=null;if(helpBubble)helpBubble.text=detailSection==0?"Hover, focus or select a gene to learn what it does. Comparisons assume other genes stay the same; no trait wins everywhere.":"Select ‘What do these words mean?’ for help with this record.";}
        void ShowHelp(JournalHelpTarget target){activeHelp=target;if(helpBubble)helpBubble.text=target.Explanation;}
        void HideHelp(JournalHelpTarget target){if(activeHelp==target)ClearHelp();}
        void ShowRecordHelp()
        {
            activeHelp=null;
            helpBubble.text=detailSection==1?"Parent A / B → child compares recorded adult traits at birth. Stride is travel speed; steering is turning; reserve is stored food; idle is maintenance energy. Mutations are changes, not improvements.":
                detailSection==2?"Movement values are adult limits, not current speed. Steering is radians turned each second. Acceleration is how quickly speed builds. Maintenance is energy spent even while still. Food multiplier changes energy gained per bite.":
                "An ID is a permanent identity within this run, even when names repeat. A recorded death has a cause and age. An unavailable actor without a death record is not assumed dead. Highlight and control use IDs, never name matching.";
        }
        void RefreshGeneCards(CreatureLineageRecord entry, CreatureAgent actor)
        {
            geneCards.SetActive(detailSection==0);detailLeft.gameObject.SetActive(detailSection!=0);detailRight.gameObject.SetActive(detailSection!=0);
            recordHelp.gameObject.SetActive(detailSection!=0);
            recordBranch.gameObject.SetActive(detailSection==3);
            detailHeading.text=session.Names.PersonalName(entry.CreatureId)+" · Exact Genes · Page 1";
            for(int i=0;i<geneButtons.Length;i++)
            {
                var gene=GenomeGeneCatalog.At(i);
                SetButtonText(geneButtons[i],GenomeGeneCatalog.Name(gene)+"   "+(actor?GenomeGeneCatalog.Get(actor.Genome,gene).ToString("0.000",System.Globalization.CultureInfo.InvariantCulture):"unavailable")+"  ?");
                geneHelp[i].Explanation=actor?GeneGuide.Help(actor.Genome,gene):GenomeGeneCatalog.Name(gene)+"\nCurrent genome unavailable. Parent comparisons and recorded mutations remain on page 2. No value has been invented.";
            }
            if(activeHelp&&activeHelp.isActiveAndEnabled)helpBubble.text=activeHelp.Explanation;
            else if(detailSection==0)ClearHelp();
        }
        string FullRecord(CreatureLineageRecord entry, CreatureAgent actor)
        {
            var a=archive;bool died=a.TryDeath(entry.CreatureId,out var death);
            string life=died?"Recorded death: "+LineageChronicle.Cause(death.Cause)+$"\nAge at death: {JournalReadout.Exact(entry.AgeAt(death.Time))} seconds":actor?"Living · exact age "+JournalReadout.Exact(actor.Life.Age)+" seconds":"Actor unavailable; no recorded death.";
            return $"IDENTITY RECORD\nStable ID: {entry.CreatureId.Value}\nGeneration: {entry.Generation}\nBirth order suffix: {session.Names.BirthOrder(entry.CreatureId)}\nBirth time: {entry.BirthTime:G17} seconds\nChildren born: {entry.OffspringCount}\n\n"+life+"\n\nParent A: "+RecordName(entry.FirstParentId)+"\nParent B: "+RecordName(entry.SecondParentId)+"\nLineage founder: "+RecordName(entry.FounderId);
        }
        string RecordName(CreatureId id)=>id.IsValid?session.Names.PersonalName(id)+" · "+id.Value:"Not recorded (founder)";
    }
}
