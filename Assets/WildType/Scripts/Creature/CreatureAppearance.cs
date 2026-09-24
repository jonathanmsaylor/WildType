using UnityEngine;
namespace WildType
{
    // Adult presentation inputs only. No random state, generation number, or simulation bonuses.
    // Juveniles scale this exact adult shape; marks and proportions never get rerolled on growth.
    public readonly struct CreatureAppearance
    {
        public readonly float Size, LegLength, BodyWidth, BodyDepth, BodyLength, BodyHeight, LegThickness, BandWidth;
        public readonly Color Coat, Mark, Accent;
        public Vector3 BodyScale => new Vector3(BodyWidth, BodyDepth, BodyLength);
        public CreatureAppearance(Genome source)
        {
            var g = (source ?? new Genome()).Copy(); g.Validate();
            Size = g.bodySize; LegLength = Size * g.legLength;
            float bulk = Mathf.InverseLerp(.65f, 1.7f, Size);
            float legs = Mathf.InverseLerp(.6f, 1.6f, g.legLength);
            BodyWidth = Size * Mathf.Lerp(1.08f, 1.60f, bulk);
            BodyDepth = Size * Mathf.Lerp(.82f, 1.18f, bulk);
            BodyLength = Size * Mathf.Lerp(1.95f, 2.30f, legs);
            BodyHeight = LegLength + BodyDepth * .32f;
            LegThickness = Size * Mathf.Lerp(.30f, .18f, legs);
            // The current leg gene also expresses a continuous coat-band width: short = broad saddles,
            // long = narrow bands. Not an independent pattern gene and not a separate gameplay modifier.
            BandWidth = Mathf.Lerp(.24f, .10f, legs);
            Coat = g.camouflage;
            Mark = Color.Lerp(Coat, new Color(.97f, .94f, .78f), .82f);
            Accent = Color.Lerp(Coat, Mark, .48f);
        }
        public static string CoatName(Color color)
        {
            Color.RGBToHSV(color, out float hue, out float saturation, out _);
            if (saturation < .16f) return "grey";
            if (hue < .06f || hue >= .94f) return "red";
            if (hue < .14f) return "amber";
            if (hue < .25f) return "olive";
            if (hue < .43f) return "green";
            if (hue < .56f) return "teal";
            if (hue < .72f) return "blue";
            return "violet";
        }
        public string Description => CoatName(Coat) + " · " + (Size > 1.2f ? "broad" : Size < .9f ? "slim" : "medium build") +
            " · " + (BandWidth > .19f ? "wide bands" : BandWidth < .14f ? "narrow bands" : "medium bands");
    }
}
