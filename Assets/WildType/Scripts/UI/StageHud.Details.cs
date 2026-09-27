using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace WildType
{
    public sealed partial class StageHud
    {
        static readonly Color[] CardColors={new Color(.20f,.29f,.35f,.97f),new Color(.22f,.33f,.25f,.97f),new Color(.38f,.29f,.21f,.97f),new Color(.33f,.25f,.36f,.97f)};
        readonly bool[] sections=new bool[3];
        readonly Button[] sectionButtons=new Button[3],geneButtons=new Button[GenomeGeneCatalog.Count];
        readonly TMP_Text[] sectionText=new TMP_Text[3];
        readonly GameObject[] sectionBodies=new GameObject[3];
        readonly JournalHelpTarget[] geneHelp=new JournalHelpTarget[GenomeGeneCatalog.Count];
        JournalHelpTarget activeHelp;
        public bool SectionOpen(int index)=>index>=0&&index<3&&sections[index];
        public void ToggleSection(int index){if(index<0||index>=3)return;sections[index]=!sections[index];ClearHelp();RefreshProfile();RepairNavigation();}
        void ClearHelp(){activeHelp=null;if(profileExplanation)profileExplanation.text="Hover, focus or select a trait for its meaning. Scroll for optional exact records.";}
        void ShowHelp(JournalHelpTarget target){activeHelp=target;if(profileExplanation)profileExplanation.text=target.Explanation;EnsureProfileVisible(target.transform as RectTransform);}
        void HideHelp(JournalHelpTarget target){if(activeHelp==target)ClearHelp();}
        void BuildSections()
        {
            string[] names={"Exact Genes","Parent Comparisons & Mutations","Family Record"};
            for(int i=0;i<3;i++){
                int slot=i;sectionButtons[i]=ButtonAt(profileContent,names[i],12,0,1110,48,()=>ToggleSection(slot),24,"+ "+names[i]);
                var target=sectionButtons[i].gameObject.AddComponent<JournalHelpTarget>();target.Show=ShowHelp;target.Hide=HideHelp;
                target.Explanation=i==0?"Every precise gene and its practical meaning. Also retains current resources and adult calculations.":i==1?"Adult comparisons with both available parents, plus retained birth comparisons and recorded mutations. Missing genomes are not reconstructed.":"Permanent identity, recorded parent links, birth information and confirmed death evidence.";
                sectionBodies[i]=Box(profileContent,names[i]+" expanded",12,0,1110,100,new Vector2(0,1),.65f).gameObject;
                sectionText[i]=Text(sectionBodies[i].transform,"",16,-12,1078,100,23);sectionText[i].name=names[i]+" record";
            }
            for(int i=0;i<geneButtons.Length;i++){
                var gene=GenomeGeneCatalog.At(i);var b=ButtonAt(sectionBodies[0].transform,"Gene "+gene,16,0,1078,80,()=>{},23);geneButtons[i]=b;
                b.GetComponent<Image>().color=CardColors[GeneGuide.Category(gene)];b.GetComponentInChildren<TMP_Text>().alignment=TextAlignmentOptions.TopLeft;
                var help=b.gameObject.AddComponent<JournalHelpTarget>();help.Show=ShowHelp;help.Hide=HideHelp;b.onClick.AddListener(help.Reveal);geneHelp[i]=help;
            }
            // Focus follows visual reading order, including the expanded exact-gene rows.
            foreach(var button in sectionButtons)navigation.Remove(button);
            foreach(var button in geneButtons)navigation.Remove(button);
            navigation.Add(sectionButtons[0]);navigation.AddRange(geneButtons);
            navigation.Add(sectionButtons[1]);navigation.Add(sectionButtons[2]);
        }
        float LayoutSections(CreatureLineageRecord r,CreatureAgent a,float y)
        {
            string[] titles={"Exact Genes","Parent Comparisons & Mutations","Family Record"};
            for(int i=0;i<3;i++){
                var button=sectionButtons[i];button.GetComponent<RectTransform>().anchoredPosition=new Vector2(12,-y);
                SetButtonText(button,(sections[i]?"− ":"+ ")+titles[i]);y+=56;sectionBodies[i].SetActive(sections[i]);if(!sections[i])continue;
                var rect=(RectTransform)sectionBodies[i].transform;rect.anchoredPosition=new Vector2(12,-y);float height=12;
                if(i==0){
                    for(int j=0;j<geneButtons.Length;j++){
                        var gene=GenomeGeneCatalog.At(j);string value=a?JournalReadout.Exact(GenomeGeneCatalog.Get(a.Genome,gene)):"unavailable";
                        string help=a?GeneGuide.Help(a.Genome,gene):"Current genome unavailable. Retained birth comparisons below; no value inferred.";
                        SetButtonText(geneButtons[j],"<b>"+GenomeGeneCatalog.Name(gene)+" · "+value+"</b>\n"+(a?GeneGuide.Explain(gene):help));geneHelp[j].Explanation=help;
                        var t=geneButtons[j].GetComponentInChildren<TMP_Text>();float h=Mathf.Max(84,t.GetPreferredValues(t.text,1062,Mathf.Infinity).y+14);
                        var br=geneButtons[j].GetComponent<RectTransform>();br.anchoredPosition=new Vector2(16,-height);br.sizeDelta=new Vector2(1078,h);t.rectTransform.sizeDelta=new Vector2(1062,h-6);height+=h+8;
                    }
                    sectionText[i].text=a?PreciseResources(a):"Current resources unavailable; no values inferred.";
                } else if(i==1){
                    var b=new StringBuilder("ADULT POTENTIAL · same conditions, not current growth or hunger\n");
                    for(int c=0;c<4;c++)b.Append('\n').Append(GeneGuide.Title(c)).Append('\n').Append(InheritedChoices.Comparisons(session,r,a,c,true)).Append('\n');
                    b.Append("\nRETAINED AT BIRTH · Parent A: ").Append(RecordName(r.FirstParentId)).Append(" · Parent B: ").Append(RecordName(r.SecondParentId)).Append('\n').Append(archive.Changes(r.CreatureId));
                    if(r.ImportantMutations!=null&&r.ImportantMutations.Length>0){b.Append("\n\nRECORDED MUTATIONS · changes, not improvements");foreach(var m in r.ImportantMutations)b.Append('\n').Append(m.Major?"Major: ":"").Append(m.TraitName).Append(" · ").Append(JournalReadout.Exact(m.InheritedValue)).Append(" → ").Append(JournalReadout.Exact(m.ChildValue));}
                    sectionText[i].text=b.ToString();
                } else sectionText[i].text=FullRecord(r,a);
                var text=sectionText[i];float th=text.GetPreferredValues(text.text,1078,Mathf.Infinity).y+8;
                text.rectTransform.anchoredPosition=new Vector2(16,-height);text.rectTransform.sizeDelta=new Vector2(1078,th);height+=th+16;
                rect.sizeDelta=new Vector2(1110,height);y+=height+12;
            }return y;
        }
        string PreciseResources(CreatureAgent a)
        {
            var p=a.Stats;
            return "CURRENT RESOURCES\nEnergy "+JournalReadout.Exact(a.Vitals.Energy)+" / "+JournalReadout.Exact(p.MaxEnergy)+" · Health "+JournalReadout.Exact(a.Vitals.Health)+" / 100 · Stamina "+JournalReadout.Exact(a.Vitals.Stamina)+" / "+JournalReadout.Exact(p.MaxStamina)+
                "\n\nADULT CALCULATIONS\nWalk "+JournalReadout.Exact(p.WalkSpeed)+" · sprint "+JournalReadout.Exact(p.SprintSpeed)+" m/s\nTurning "+JournalReadout.Exact(p.TurnRate)+" rad/s · acceleration "+JournalReadout.Exact(p.Acceleration)+
                "\nMaintenance "+JournalReadout.Exact(p.PassiveDrain)+" E/s · movement adds up to "+JournalReadout.Exact(p.MoveCost)+" E/s\nSprinting adds "+JournalReadout.Exact(p.SprintCost)+" E/s · stamina use "+JournalReadout.Exact(p.StaminaCost)+" /s\nStamina recovery "+JournalReadout.Exact(p.StaminaRecovery)+" /s · nutrition factor "+JournalReadout.Exact(p.NutritionFactor)+
                "\nMaturity "+JournalReadout.Exact(p.MaturityAge)+" s · lifespan "+JournalReadout.Exact(p.LifespanSeconds)+" s\nMating threshold "+JournalReadout.Exact(p.MaxEnergy*p.ReproductionEnergyFraction)+" E · birth cost "+JournalReadout.Exact(p.MaxEnergy*GenerationLoop.ParentEnergyCost)+" E\nMating recovery "+JournalReadout.Exact(p.ReproductionCooldown)+" s\n\n"+session.Care.LastTransfer;
        }
        string FullRecord(CreatureLineageRecord entry,CreatureAgent actor)
        {
            bool died=archive.TryDeath(entry.CreatureId,out var death);
            string life=died?"Recorded death: "+LineageChronicle.Cause(death.Cause)+"\nAge at death: "+JournalReadout.Exact(entry.AgeAt(death.Time))+" seconds\nRecorded death time: "+death.Time.ToString("G17",System.Globalization.CultureInfo.InvariantCulture):actor?"Living · exact age "+JournalReadout.Exact(actor.Life.Age)+" seconds":"Actor unavailable; no recorded death.";
            return $"IDENTITY RECORD\nStable ID: {entry.CreatureId.Value}\nGeneration: {entry.Generation}\nRecorded birth order: {session.Names.BirthOrder(entry.CreatureId)}\nBirth time: {entry.BirthTime:G17} seconds\nChildren born: {entry.OffspringCount}\n\n"+life+"\n\nParent A: "+RecordName(entry.FirstParentId)+"\nParent B: "+RecordName(entry.SecondParentId)+"\nLineage founder: "+RecordName(entry.FounderId)+"\n\n"+CreatureProfileText.Family(session,entry.CreatureId);
        }
        string RecordName(CreatureId id)=>id.IsValid?session.Names.PersonalName(id)+" · "+id.Value:"Not recorded (founder)";
    }
}
