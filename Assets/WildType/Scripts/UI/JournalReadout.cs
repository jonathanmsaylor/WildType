using System;
using System.Globalization;
namespace WildType
{
    // Display only: never feeds a rounded value back into simulation.
    public static class JournalReadout
    {
        public static string Whole(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return "Unavailable";
            if (value > 0 && value < 1) return "less than 1";
            if (value < 0 && value > -1) return "more than -1";
            return Math.Round(value, 0, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture);
        }
        public static string Approx(float value) => value == Math.Truncate(value) || Math.Abs(value) < 1
            ? Whole(value) : "about " + Whole(value);
        public static string Exact(float value) => value.ToString("G9", CultureInfo.InvariantCulture);
        // Thresholds round upwards so a displayed target is sufficient, never a false eligibility promise.
        public static string Required(float value) => Math.Ceiling(value).ToString("0", CultureInfo.InvariantCulture);
        public static string Care(float cost, float gain) => $"You spend {Approx(cost)} energy · child gains {Approx(gain)}";
        public static string Distance(float distance) => Whole(distance) + " m";
    }
}
