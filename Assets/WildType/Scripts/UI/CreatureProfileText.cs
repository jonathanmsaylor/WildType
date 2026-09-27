using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
namespace WildType
{
    // Read-only presentation. Adult inherited values are compared in the same open-ground conditions.
    public static class CreatureProfileText
    {
        public static string N(float value)=>value.ToString("0.#",CultureInfo.InvariantCulture);
        public static string SpeedRating(float speed)=>speed<3?"Slow":speed<4.5f?"Moderate":"Fast";
        public static string Compare(float value,float parent,string name)
        {
            if(Mathf.Abs(value-parent)<.0001f)return "same as "+name;
            float percent=100*Mathf.Abs(value-parent)/Mathf.Max(.0001f,parent);
            string amount=percent<.1f?"less than 0.1":percent<1?percent.ToString("0.0",CultureInfo.InvariantCulture):Mathf.Round(percent).ToString(CultureInfo.InvariantCulture);
            return amount+"% "+(value>parent?"faster":"slower")+" than "+name;
        }
        public static string Movement(StageSession s,CreatureLineageRecord r,CreatureAgent actor,bool exact)
        {
            if(!actor)return "Current adult traits unavailable. Recorded birth comparisons remain in Details.";
            float speed=actor.Stats.WalkSpeed;
            string result=exact?"Adult open-ground walk: "+JournalReadout.Exact(speed)+" m/s. Growth, hunger and biome effects excluded.":SpeedRating(speed)+" · adult open-ground walking";
            foreach(var id in new[]{r.FirstParentId,r.SecondParentId}){
                if(!id.IsValid)continue;var parent=InheritedChoices.Parent(s,id);
                result+="\n"+(parent?(exact?s.Names.PersonalName(id)+": "+JournalReadout.Exact(parent.Stats.WalkSpeed)+" m/s":Compare(speed,parent.Stats.WalkSpeed,s.Names.PersonalName(id))):s.Names.PersonalName(id)+": comparison unavailable; see birth record");}
            if(exact)result+="\nSlow <3; Moderate 3–4.5; Fast ≥4.5 m/s. Speed trades off with turning and energy.";
            return result;
        }
        public static string Family(StageSession s,CreatureId id)
        {
            var children=new List<CreatureLineageRecord>();s.Generations.Archive.CollectChildren(id,children);int living=0,dead=0,unknown=0;
            foreach(var r in children)if(s.Generations.Archive.TryDeath(r.CreatureId,out _))dead++;else if(LineageChronicle.LivingActor(s,r))living++;else unknown++;
            return children.Count+" recorded children · "+living+" living · "+dead+" deceased"+(unknown>0?" · "+unknown+" unavailable":"");
        }
    }
}
