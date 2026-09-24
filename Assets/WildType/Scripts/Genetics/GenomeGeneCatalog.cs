using System;
using UnityEngine;

namespace WildType
{
    public enum GenomeGene
    {
        BodySize,
        LegLength,
        MovementSpeed,
        TurnAgility,
        Vision,
        Metabolism,
        EnergyEfficiency,
        MaximumEnergy,
        Stamina,
        Fertility,
        ReproductionThreshold,
        OffspringTendency,
        Lifespan,
        CamouflageRed,
        CamouflageGreen,
        CamouflageBlue
    }

    public static class GenomeGeneCatalog
    {
        static readonly GenomeGene[] genes = (GenomeGene[])Enum.GetValues(typeof(GenomeGene));
        public static int Count => genes.Length;
        public static GenomeGene At(int index) => genes[index];

        public static string Name(GenomeGene gene)
        {
            switch (gene)
            {
                case GenomeGene.BodySize: return "Body size";
                case GenomeGene.LegLength: return "Leg length";
                case GenomeGene.MovementSpeed: return "Movement speed";
                case GenomeGene.TurnAgility: return "Turn agility";
                case GenomeGene.Vision: return "Vision";
                case GenomeGene.Metabolism: return "Metabolism";
                case GenomeGene.EnergyEfficiency: return "Energy efficiency";
                case GenomeGene.MaximumEnergy: return "Maximum energy";
                case GenomeGene.Stamina: return "Stamina";
                case GenomeGene.Fertility: return "Fertility";
                case GenomeGene.ReproductionThreshold: return "Reproduction threshold";
                case GenomeGene.OffspringTendency: return "Offspring tendency";
                case GenomeGene.Lifespan: return "Lifespan";
                case GenomeGene.CamouflageRed: return "Camouflage red";
                case GenomeGene.CamouflageGreen: return "Camouflage green";
                case GenomeGene.CamouflageBlue: return "Camouflage blue";
                default: throw new ArgumentOutOfRangeException(nameof(gene), gene, null);
            }
        }

        public static float Minimum(GenomeGene gene)
        {
            switch (gene)
            {
                case GenomeGene.BodySize: return .65f;
                case GenomeGene.LegLength: return .6f;
                case GenomeGene.MovementSpeed: return 3f;
                case GenomeGene.TurnAgility: return 2f;
                case GenomeGene.Vision: return 10f;
                case GenomeGene.Metabolism: return .5f;
                case GenomeGene.EnergyEfficiency: return .6f;
                case GenomeGene.MaximumEnergy: return 60f;
                case GenomeGene.Stamina: return 60f;
                case GenomeGene.Fertility: return .5f;
                case GenomeGene.ReproductionThreshold: return .5f;
                case GenomeGene.OffspringTendency: return .4f;
                case GenomeGene.Lifespan: return 240f;
                case GenomeGene.CamouflageRed:
                case GenomeGene.CamouflageGreen:
                case GenomeGene.CamouflageBlue: return .08f;
                default: throw new ArgumentOutOfRangeException(nameof(gene), gene, null);
            }
        }

        public static float Maximum(GenomeGene gene)
        {
            switch (gene)
            {
                case GenomeGene.BodySize: return 1.7f;
                case GenomeGene.LegLength: return 1.6f;
                case GenomeGene.MovementSpeed: return 10f;
                case GenomeGene.TurnAgility: return 12f;
                case GenomeGene.Vision: return 45f;
                case GenomeGene.Metabolism: return 1.8f;
                case GenomeGene.EnergyEfficiency: return 1.5f;
                case GenomeGene.MaximumEnergy: return 150f;
                case GenomeGene.Stamina: return 150f;
                case GenomeGene.Fertility: return 1.5f;
                case GenomeGene.ReproductionThreshold: return .9f;
                case GenomeGene.OffspringTendency: return 1.6f;
                case GenomeGene.Lifespan: return 900f;
                case GenomeGene.CamouflageRed:
                case GenomeGene.CamouflageGreen:
                case GenomeGene.CamouflageBlue: return .9f;
                default: throw new ArgumentOutOfRangeException(nameof(gene), gene, null);
            }
        }

        public static float Span(GenomeGene gene) => Maximum(gene) - Minimum(gene);
        public static float Clamp(GenomeGene gene, float value) => Genome.Safe(value, (Minimum(gene) + Maximum(gene)) * .5f, Minimum(gene), Maximum(gene));
        public static float Normalize(GenomeGene gene, float value) => Mathf.InverseLerp(Minimum(gene), Maximum(gene), Clamp(gene, value));

        public static float Get(Genome genome, GenomeGene gene)
        {
            if (genome == null) throw new ArgumentNullException(nameof(genome));
            switch (gene)
            {
                case GenomeGene.BodySize: return genome.bodySize;
                case GenomeGene.LegLength: return genome.legLength;
                case GenomeGene.MovementSpeed: return genome.movementSpeed;
                case GenomeGene.TurnAgility: return genome.turnAgility;
                case GenomeGene.Vision: return genome.vision;
                case GenomeGene.Metabolism: return genome.metabolism;
                case GenomeGene.EnergyEfficiency: return genome.energyEfficiency;
                case GenomeGene.MaximumEnergy: return genome.maximumEnergy;
                case GenomeGene.Stamina: return genome.stamina;
                case GenomeGene.Fertility: return genome.fertility;
                case GenomeGene.ReproductionThreshold: return genome.reproductionThreshold;
                case GenomeGene.OffspringTendency: return genome.offspringTendency;
                case GenomeGene.Lifespan: return genome.lifespan;
                case GenomeGene.CamouflageRed: return genome.camouflage.r;
                case GenomeGene.CamouflageGreen: return genome.camouflage.g;
                case GenomeGene.CamouflageBlue: return genome.camouflage.b;
                default: throw new ArgumentOutOfRangeException(nameof(gene), gene, null);
            }
        }

        public static void Set(Genome genome, GenomeGene gene, float value)
        {
            if (genome == null) throw new ArgumentNullException(nameof(genome));
            value = Clamp(gene, value);
            switch (gene)
            {
                case GenomeGene.BodySize: genome.bodySize = value; break;
                case GenomeGene.LegLength: genome.legLength = value; break;
                case GenomeGene.MovementSpeed: genome.movementSpeed = value; break;
                case GenomeGene.TurnAgility: genome.turnAgility = value; break;
                case GenomeGene.Vision: genome.vision = value; break;
                case GenomeGene.Metabolism: genome.metabolism = value; break;
                case GenomeGene.EnergyEfficiency: genome.energyEfficiency = value; break;
                case GenomeGene.MaximumEnergy: genome.maximumEnergy = value; break;
                case GenomeGene.Stamina: genome.stamina = value; break;
                case GenomeGene.Fertility: genome.fertility = value; break;
                case GenomeGene.ReproductionThreshold: genome.reproductionThreshold = value; break;
                case GenomeGene.OffspringTendency: genome.offspringTendency = value; break;
                case GenomeGene.Lifespan: genome.lifespan = value; break;
                case GenomeGene.CamouflageRed: genome.camouflage = new Color(value, genome.camouflage.g, genome.camouflage.b, 1); break;
                case GenomeGene.CamouflageGreen: genome.camouflage = new Color(genome.camouflage.r, value, genome.camouflage.b, 1); break;
                case GenomeGene.CamouflageBlue: genome.camouflage = new Color(genome.camouflage.r, genome.camouflage.g, value, 1); break;
                default: throw new ArgumentOutOfRangeException(nameof(gene), gene, null);
            }
        }
    }
}
