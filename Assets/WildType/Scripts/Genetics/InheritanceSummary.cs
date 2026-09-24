using System.Globalization;
namespace WildType
{
    // Text snapshots stored once at birth, so both parents remain comparable after they die.
    public static class InheritanceSummary
    {
        static string N(float value, string format = "0.00") => value.ToString(format, CultureInfo.InvariantCulture);
        static string Row(string label, float value, float a, float b, string unit = "") =>
            label + " " + N(a) + " / " + N(b) + " -> " + N(value) + unit;
        const string Tradeoffs = "\nLong legs: +stride speed / -steering\nLarge body: +reserves / +food cost";
        public static string Founder(Genome genome)
        {
            var appearance = new CreatureAppearance(genome); var stats = new Phenotype(genome);
            return appearance.Description + "\nAdult size " + N(genome.bodySize) + " · legs " + N(genome.legLength) +
                "\nSprint " + N(stats.SprintSpeed) + " m/s · steer " + N(stats.TurnRate) + " rad/s" +
                "\nReserve " + N(stats.MaxEnergy, "0") + " E · idle " + N(stats.PassiveDrain) + " E/s" + Tradeoffs;
        }
        public static string Child(Genome child, Genome first, Genome second)
        {
            var c = new Phenotype(child); var a = new Phenotype(first); var b = new Phenotype(second);
            return new CreatureAppearance(child).Description + "\nADULT: parent A / B -> child\n" +
                "Coat " + CreatureAppearance.CoatName(first.camouflage) + " / " + CreatureAppearance.CoatName(second.camouflage) + " -> " + CreatureAppearance.CoatName(child.camouflage) + "\n" +
                Row("Size", child.bodySize, first.bodySize, second.bodySize) + "\n" +
                Row("Legs", child.legLength, first.legLength, second.legLength) + "\n" +
                Row("Sprint", c.SprintSpeed, a.SprintSpeed, b.SprintSpeed, " m/s") + "\n" +
                Row("Steer", c.TurnRate, a.TurnRate, b.TurnRate, " rad/s") + "\n" +
                Row("Reserve", c.MaxEnergy, a.MaxEnergy, b.MaxEnergy, " E") + "\n" +
                Row("Idle", c.PassiveDrain, a.PassiveDrain, b.PassiveDrain, " E/s") + Tradeoffs;
        }
        public static string NotableMutation(MutationRecord[] records)
        {
            if (records == null || records.Length == 0) return "";
            // Prefer an appearance mutation over an unrelated record when one was recorded.
            var chosen = records[0];
            foreach (var item in records)
                if (item.Gene == GenomeGene.BodySize || item.Gene == GenomeGene.LegLength || (int)item.Gene >= (int)GenomeGene.CamouflageRed)
                { chosen = item; break; }
            return "\n" + (chosen.Major ? "Major mutation: " : "Mutation: ") + chosen.TraitName +
                "\n" + N(chosen.InheritedValue) + " -> " + N(chosen.ChildValue) + " (not a guaranteed benefit)";
        }
    }
}
