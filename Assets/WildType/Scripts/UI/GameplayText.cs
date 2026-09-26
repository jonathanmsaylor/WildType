using System.Text;
using UnityEngine;
namespace WildType
{
    // Presentation only. Values come from the same phenotype and transfer rules used by gameplay.
    public static class GameplayText
    {
        public static string Status(float energy, float maximum, float health, bool sprintLocked)
        {
            if (health <= 0) return "Life ended — open the journal.";
            var text = new StringBuilder();
            if (energy <= .001f) text.Append("STARVING — eat now. Health is falling.");
            else if (energy <= maximum * .25f) text.Append("Hungry — find ripe fruit soon.");
            if (health < 60) { if (text.Length > 0) text.Append('\n'); text.Append("Low health — food does not heal damage."); }
            if (sprintLocked) { if (text.Length > 0) text.Append('\n'); text.Append("Tired — walk or rest to recover stamina."); }
            return text.ToString();
        }
        public static string Care(StageSession session, CreatureAgent parent, CreatureAgent child, string key)
        {
            if (!child || session.Care.Reason(parent, child).Length > 0 || !FamilyCareRules.Transfer(parent.Vitals.Energy,
                parent.Stats.MaxEnergy, child.Vitals.Energy, child.Stats.MaxEnergy, out float cost, out float gain)) return "";
            return $"{key} Share with {session.Names.PersonalName(child.Life.Id)}\n" + JournalReadout.Care(cost, gain);
        }
        public static string Build(Genome g) => (g.bodySize > 1.12f ? "Large build" : g.bodySize < .94f ? "Small build" : "Medium build") +
            " · " + (g.legLength > 1.1f ? "long legs" : g.legLength < .9f ? "short legs" : "medium legs");
        public const string Tradeoffs = "All other genes equal:\nLarger bodies store more energy, but eat more and accelerate more slowly.\nLonger legs cover open ground faster, but turn less sharply. Tight woodland can limit their speed.\nCoat and markings are appearance, not bonuses. No body type wins everywhere.";
        public static string Numbers(CreatureAgent a)
        {
            var s = a.Stats;
            return $"ADULT MOVEMENT & RESOURCES\nWalk {s.WalkSpeed:0.00} · sprint {s.SprintSpeed:0.00} m/s\nSteering {s.TurnRate:0.00} rad/s · acceleration {s.Acceleration:0.00}\nEnergy reserve {s.MaxEnergy:0.00} · stamina {s.MaxStamina:0.00}\nMaintenance {s.PassiveDrain:0.000} energy/s\nMovement adds up to {s.MoveCost:0.000} energy/s\nSprinting adds {s.SprintCost:0.000} energy/s\nFood multiplier {s.NutritionFactor:0.00}\nMate at {s.MaxEnergy*s.ReproductionEnergyFraction:0.00} energy\nBirth costs {s.MaxEnergy*GenerationLoop.ParentEnergyCost:0.00} energy\nAdult at {s.MaturityAge:0.0}s · lifespan {s.LifespanSeconds:0.0}s\nCooldown after birth {s.ReproductionCooldown:0.0}s\n\nThese are adult values, not a fitness score.\nGrowth, fatigue and woodland also affect movement.";
        }
        public static string Genes(Genome g)
        {
            var b=new StringBuilder("EXACT GENES\n");
            for(int i=0;i<GenomeGeneCatalog.Count;i++){var gene=GenomeGeneCatalog.At(i);b.Append(GenomeGeneCatalog.Name(gene)).Append("  ").Append(GenomeGeneCatalog.Get(g,gene).ToString("0.000")).Append('\n');}
            return b.ToString();
        }
        public static string Mutations(MutationRecord[] records, int offset = 0, int count = int.MaxValue)
        {
            if(records==null||records.Length==0)return "No notable mutations recorded.\nSmall unrecorded changes may still have occurred.";
            var b=new StringBuilder("RECORDED MUTATIONS (not improvements)\n");
            for(int i=offset;i<records.Length&&i-offset<count;i++){var r=records[i];b.Append(r.Major?"Major: ":"Mutation: ").Append(r.TraitName).Append("  ").Append(r.InheritedValue.ToString("0.000")).Append(" → ").Append(r.ChildValue.ToString("0.000")).Append('\n');}
            return b.ToString();
        }
        public static string Guide(bool pad) => "GETTING STARTED · Simulation paused\n\n" +
            (pad ? "Move: left stick · look: right stick\nSprint: press left stick (uses stamina and extra energy)\n\n" : "Move: W A S D · look: mouse\nSprint: hold Shift (uses stamina and extra energy)\n\n") +
            "1. Find fruit on a plant. Empty stems need time to regrow.\n" + (pad?"South button":"E") + " eats ripe food when close. Food restores ENERGY, not health.\n\n" +
            "2. Eat enough to mate, then approach a healthy adult.\n" + (pad?"West button":"F") + " attempts mating. A failed attempt explains why.\nStay close during the hearts. Each parent pays energy at birth.\n\n" +
            "3. After a birth, name your child or keep its default name.\n" + (pad?"North button":"Tab") + " opens your family journal. Highlight locates; Take control switches creatures.\n\n" +
            "4. Helping is optional. Approach your juvenile child.\n" + (pad?"Right shoulder":"R") + " shares energy: the prompt shows your cost and its gain.\nCare does not heal or guarantee survival. Siblings cannot receive care.\n\n" +
            "Leave depleted patches to find food elsewhere. Rest saves movement energy,\nbut maintenance still costs food. " + (pad?"Start":"Escape") + " opens/closes the paused menu.";
    }
}
