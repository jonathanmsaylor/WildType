using System.Text;
using UnityEngine;
namespace WildType
{
    // Read-only adult comparisons. Never rank genomes or use temporary vitals / juvenile scale.
    public static class InheritedChoices
    {
        public static CreatureAgent Parent(StageSession s, CreatureId id)
        { var r=s.Generations.Archive.Get(id);return r==null?null:LineageChronicle.LivingActor(s,r); }
        public static string Name(StageSession s,CreatureId id,bool compact=false)
        { string n=s.Names.PersonalName(id);return compact&&n.Length>14?n.Substring(0,13)+"…":n; }
        public static bool Different(float a,float b)=>Mathf.Abs(a-b)>.0001f&&Mathf.Abs(a-b)/Mathf.Max(.0001f,Mathf.Abs(b))>=.02f;
        public static string Pair(Phenotype c,Phenotype p,string name,int category)
        {
            switch(category){
                case 0:
                    if(Different(c.WalkSpeed,p.WalkSpeed))return (c.WalkSpeed>p.WalkSpeed?"Faster":"Slower")+" in The Meadow than "+name+
                        (Different(c.TurnRate,p.TurnRate)?(c.TurnRate<p.TurnRate?"; wider turns.":"; sharper turns."):".");
                    if(Different(c.TurnRate,p.TurnRate))return (c.TurnRate>p.TurnRate?"Sharper":"Wider")+" turns than "+name+".";
                    return "Similar adult pace and turning to "+name+".";
                case 1:
                    if(Different(c.Vision,p.Vision))return (c.Vision>p.Vision?"Sees farther":"Shorter sight")+" than "+name+
                        (Different(c.PassiveDrain,p.PassiveDrain)?(c.PassiveDrain>p.PassiveDrain?"; more upkeep.":"; less upkeep."):".");
                    if(Different(c.NutritionFactor,p.NutritionFactor))return (c.NutritionFactor>p.NutritionFactor?"More":"Less")+" energy per bite than "+name+".";
                    return "Similar sight and bite energy to "+name+".";
                case 2:
                    if(Different(c.MaxEnergy,p.MaxEnergy))return (c.MaxEnergy>p.MaxEnergy?"Larger":"Smaller")+" reserve than "+name+
                        (Different(c.PassiveDrain,p.PassiveDrain)?(c.PassiveDrain>p.PassiveDrain?"; more upkeep.":"; less upkeep."):".");
                    if(Different(c.PassiveDrain,p.PassiveDrain))return (c.PassiveDrain<p.PassiveDrain?"Less":"More")+" upkeep than "+name+".";
                    return "Similar reserve and upkeep to "+name+".";
                default:
                    if(Different(c.ReproductionCooldown,p.ReproductionCooldown))return (c.ReproductionCooldown<p.ReproductionCooldown?"Shorter":"Longer")+" mating recovery than "+name+".";
                    return "Similar mating recovery to "+name+".";
            }
        }
        public static string Comparisons(StageSession s,CreatureLineageRecord r,CreatureAgent a,int category,bool exact=false)
        {
            if(r.Generation==0)return "Founder · no parents recorded.";
            var b=new StringBuilder();foreach(var id in new[]{r.FirstParentId,r.SecondParentId}){
                if(b.Length>0)b.Append('\n');var p=Parent(s,id);string name=Name(s,id);
                if(!a||!p){b.Append(name).Append(": adult comparison unavailable; retained birth record below.");continue;}
                b.Append(Pair(a.Stats,p.Stats,name,category));
                if(exact)b.Append(" ").Append(Values(a.Stats,category)).Append(" Parent: ").Append(Values(p.Stats,category));
            }return b.ToString();
        }
        public static string Values(Phenotype p,int category)
        {return category switch {
            0=>"Walk "+JournalReadout.Exact(p.WalkSpeed)+" m/s, turn "+JournalReadout.Exact(p.TurnRate)+" rad/s.",
            1=>"Vision "+JournalReadout.Exact(p.Vision)+" m, bite factor "+JournalReadout.Exact(p.NutritionFactor)+".",
            2=>"Reserve "+JournalReadout.Exact(p.MaxEnergy)+", upkeep "+JournalReadout.Exact(p.PassiveDrain)+" E/s.",
            _=>"Mating recovery "+JournalReadout.Exact(p.ReproductionCooldown)+" s; threshold "+JournalReadout.Exact(p.ReproductionEnergyFraction)+"."};}
        // Prefer opposing consequences to a pair that could read as an overall ranking.
        public static string Opposing(Phenotype c,Phenotype p,string name)
        {
            if(Different(c.WalkSpeed,p.WalkSpeed)&&Different(c.TurnRate,p.TurnRate)&&(c.WalkSpeed>p.WalkSpeed)!=(c.TurnRate>p.TurnRate))
                return (c.WalkSpeed>p.WalkSpeed?"Faster":"Slower")+" in The Meadow\nthan "+name+(c.TurnRate<p.TurnRate?" · wider turns":" · sharper turns");
            if(Different(c.Vision,p.Vision)&&Different(c.PassiveDrain,p.PassiveDrain)&&(c.Vision>p.Vision)==(c.PassiveDrain>p.PassiveDrain))
                return (c.Vision>p.Vision?"Sees farther":"Shorter sight")+"\nthan "+name+Upkeep(c,p);
            if(Different(c.MaxEnergy,p.MaxEnergy)&&Different(c.PassiveDrain,p.PassiveDrain)&&(c.MaxEnergy>p.MaxEnergy)==(c.PassiveDrain>p.PassiveDrain))
                return (c.MaxEnergy>p.MaxEnergy?"Larger":"Smaller")+" reserve\nthan "+name+Upkeep(c,p);
            return "";
        }
        // One compact callout fits the fixed-size tree card; both parents remain in the profile.
        public static string Callout(StageSession s,CreatureLineageRecord r,CreatureAgent a)
        {
            if(!a||r.Generation==0)return "";
            foreach(var id in new[]{r.FirstParentId,r.SecondParentId}){
                var p=Parent(s,id);if(!p)continue;string tradeoff=Opposing(a.Stats,p.Stats,Name(s,id,true));if(tradeoff.Length>0)return tradeoff;
            }
            foreach(var id in new[]{r.FirstParentId,r.SecondParentId}){
                var p=Parent(s,id);if(!p)continue;string name=Name(s,id,true);var c=a.Stats;var v=p.Stats;
                if(Different(c.WalkSpeed,v.WalkSpeed))return (c.WalkSpeed>v.WalkSpeed?"Faster":"Slower")+" in The Meadow\nthan "+name+(Different(c.TurnRate,v.TurnRate)?(c.TurnRate<v.TurnRate?" · wider turns":" · sharper turns"):"");
                if(Different(c.Vision,v.Vision))return (c.Vision>v.Vision?"Sees farther":"Shorter sight")+"\nthan "+name+Upkeep(c,v);
                if(Different(c.MaxEnergy,v.MaxEnergy))return (c.MaxEnergy>v.MaxEnergy?"Larger":"Smaller")+" reserve\nthan "+name+Upkeep(c,v);
                if(Different(c.TurnRate,v.TurnRate))return (c.TurnRate>v.TurnRate?"Sharper":"Wider")+" turns than "+name;
            }
            return Parent(s,r.FirstParentId)||Parent(s,r.SecondParentId)?"Similar adult traits\nCompare parents in profile":"Parent traits unavailable\nBirth record in profile";
        }
        static string Upkeep(Phenotype c,Phenotype p)=>Different(c.PassiveDrain,p.PassiveDrain)?(c.PassiveDrain>p.PassiveDrain?" · more upkeep":" · less upkeep"):"";
        public static string Summary(CreatureAgent a,int category)
        {
            if(!a)return "Current traits unavailable. Retained records are below.";
            var p=a.Stats;
            return category switch {
                0=>CreatureProfileText.SpeedRating(p.WalkSpeed)+" open-ground pace · "+GameplayText.Build(a.Genome)+"\nFernwood may limit stride; turning matters.",
                1=>"Scout visible food within "+JournalReadout.Distance(p.Vision)+".\nBigger bites refill energy; longer sight also costs upkeep.",
                2=>"Stores "+JournalReadout.Whole(p.MaxEnergy)+" energy · "+JournalReadout.Whole(p.MaxStamina)+" sprint stamina.\nA larger reserve takes more food to fill.",
                _=>new CreatureAppearance(a.Genome).Description+"\nMate at "+JournalReadout.Required(p.MaxEnergy*p.ReproductionEnergyFraction)+" energy; birth costs "+JournalReadout.Approx(p.MaxEnergy*GenerationLoop.ParentEnergyCost)+"."};
        }
    }
}
