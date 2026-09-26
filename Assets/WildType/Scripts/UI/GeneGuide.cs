namespace WildType
{
    public static class GeneGuide
    {
        public static int Category(GenomeGene gene)
        {
            switch (gene)
            {
                case GenomeGene.BodySize: case GenomeGene.LegLength: case GenomeGene.MovementSpeed: case GenomeGene.TurnAgility: return 0;
                case GenomeGene.Vision: case GenomeGene.Metabolism: case GenomeGene.EnergyEfficiency: return 1;
                case GenomeGene.MaximumEnergy: case GenomeGene.Stamina: case GenomeGene.Lifespan: return 2;
                default: return 3;
            }
        }
        public static string Title(int category) => new[] { "Movement", "Senses & Foraging", "Survival", "Inheritance & Coat" }[category];
        public static string Summary(int category) => new[] { "Stride, steering and body shape", "Finding food and using its energy", "Reserves, endurance and time to grow", "Family timing and visible color" }[category];
        public static string Explain(GenomeGene gene)
        {
            switch (gene)
            {
                case GenomeGene.BodySize: return "Body scale. Higher means a larger body and energy reserve, but more food use and slower acceleration. Lower reverses these effects. Other genes still matter.";
                case GenomeGene.LegLength: return "Leg proportion. Higher gives longer legs and faster open-ground travel, but slower turning. Lower turns more sharply; Fernwood limits speed by turning ability.";
                case GenomeGene.MovementSpeed: return "Base travel pace, before other traits. Higher raises walking and sprint speed, but increases movement energy and sprint stamina use. Lower is slower and cheaper.";
                case GenomeGene.TurnAgility: return "Steering ability. Higher turns faster and may retain more speed in Fernwood; lower turns more slowly. No separate energy penalty is applied to this gene.";
                case GenomeGene.Vision: return "Local search distance in meters for autonomous food and partner decisions. Higher searches farther but adds maintenance cost. Lower sees less and costs less. It does not zoom the player's camera.";
                case GenomeGene.Metabolism: return "How quickly resources are processed. Higher recovers stamina faster and gets more energy per bite, but burns more maintenance energy. Lower does each more slowly.";
                case GenomeGene.EnergyEfficiency: return "Energy economy. Higher reduces living, movement and sprint costs, but lowers burst speed. Lower trades more energy use for faster bursts.";
                case GenomeGene.MaximumEnergy: return "Base energy capacity. Higher stores more food; lower fills sooner. Body size also changes the final reserve. Birth cost and mating readiness scale with that reserve.";
                case GenomeGene.Stamina: return "Sprint endurance capacity. Higher allows a longer sprint reserve; lower empties sooner. Recovery rate comes from metabolism. This is not food or health.";
                case GenomeGene.Lifespan: return "Maximum age in seconds. Higher allows a longer life but delays adulthood and adds maintenance cost. Lower matures sooner but reaches old age sooner. Starvation may end either life earlier.";
                case GenomeGene.Fertility: return "Mating recovery rate. Higher shortens the wait after a birth, but increases maintenance cost. Lower waits longer and costs less. It does not guarantee a birth.";
                case GenomeGene.ReproductionThreshold: return "Fraction of the full energy reserve required to mate. Higher requires more food first; lower permits mating sooner with less reserve. It does not change the energy charge at birth.";
                case GenomeGene.OffspringTendency: return "Autonomous interest in seeking a mate. Higher increases the chance of checking for partners and adds maintenance cost. Lower reduces both. Player F still follows the same eligibility rules.";
                case GenomeGene.CamouflageRed: return "Red part of the inherited coat color. Higher adds red; lower removes red. Color also contributes to visible markings. Despite the gene's name, this stage gives no hiding or survival bonus.";
                case GenomeGene.CamouflageGreen: return "Green part of the inherited coat color. Higher adds green; lower removes green. Color also contributes to visible markings. There is no camouflage survival bonus in this stage.";
                default: return "Blue part of the inherited coat color. Higher adds blue; lower removes blue. Color also contributes to visible markings. There is no camouflage survival bonus in this stage.";
            }
        }
        public static string Help(Genome genome, GenomeGene gene) => GenomeGeneCatalog.Name(gene) + " · exact " +
            JournalReadout.Exact(GenomeGeneCatalog.Get(genome, gene)) + "\n" + Explain(gene);
    }
}
