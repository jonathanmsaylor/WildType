using System.Collections.Generic;
using System.Text;
namespace WildType
{
    // Run-only presentation names. IDs, ancestry, genetic random streams and simulation rules are unchanged.
    public sealed class FamilyNames
    {
        sealed class Entry { public int order; public string name = ""; }
        readonly Dictionary<CreatureId, Entry> names = new Dictionary<CreatureId, Entry>();
        readonly StageSession session;
        static readonly string[] GivenNames = { "Mira", "Tavi", "Neri", "Luma", "Kori", "Sela", "Rumi", "Tala", "Bram", "Nilo", "Vela", "Sora", "Milo", "Kira", "Oren", "Fenna", "Liri", "Timo", "Asha", "Runa", "Pavo", "Nola", "Ivo", "Mena" };
        CreatureId pending, parent;
        public bool ShowPrompts { get; set; } = true;
        public CreatureId Pending => pending;
        public bool HasPrompt => pending.IsValid;
        public bool IsFounderPrompt { get; private set; }
        public int Count => names.Count;
        public FamilyNames(StageSession value) { session = value; }
        public void ResetRun() { names.Clear(); pending = parent = default; IsFounderPrompt = false; }
        public void BeginFounderName()
        {
            if (!session.Ready || !session.Player || !names.TryGetValue(session.Player.Life.Id, out var entry)) return;
            entry.name = "Eddy";
            pending = parent = session.Player.Life.Id; IsFounderPrompt = true;
            session.SetPaused(true);
        }
        public void RecordFounder(CreatureId id)
        {
            var record = session.Generations.Archive.Get(id);
            if (record == null || record.Generation != 0 || names.Count >= LineageArchive.Capacity || names.ContainsKey(id)) return;
            names.Add(id, new Entry { name = DefaultName(id) }); // No child-order suffix for a founder.
        }
        public void RecordBirth(CreatureAgent child, CreatureAgent first, CreatureAgent second)
        {
            if (!child || names.Count >= LineageArchive.Capacity || names.ContainsKey(child.Life.Id)) return;
            var donor = first.IsPlayer ? first : second.IsPlayer ? second : first;
            int order = session.Generations.Archive.Get(donor.Life.Id).OffspringCount;
            names[child.Life.Id] = new Entry { order = order, name = DefaultName(child.Life.Id) };
            if (!ShowPrompts || !donor.IsPlayer || donor.Vitals.Dead) return;
            pending = child.Life.Id; parent = donor.Life.Id; session.SetPaused(true);
        }
        public static string Format(string name, int order) => CleanName(name) + " " + System.Math.Max(1, order).ToString(System.Globalization.CultureInfo.InvariantCulture);
        public static string DefaultName(CreatureId id)
        {
            if (!id.IsValid) return "";
            // Stable ID includes run seed + ordinal. Fixed unsigned hash/avalanche makes an isolated
            // reproducible cosmetic choice: no Unity Random, System.Random, or genetics RNG consumption.
            unchecked
            {
                uint hash = 2166136261;
                foreach (char c in id.Value) { hash ^= c; hash *= 16777619; }
                hash ^= hash >> 16; hash *= 0x7feb352d; hash ^= hash >> 15; hash *= 0x846ca68b; hash ^= hash >> 16;
                return GivenNames[hash % (uint)GivenNames.Length];
            }
        }
        public static string CleanName(string value)
        {
            var result = new StringBuilder(16);
            foreach (char c in value ?? "")
            {
                if (result.Length >= 16) break;
                if (char.IsLetterOrDigit(c) || c == '-' || c == '\'') result.Append(c);
                else if (c == ' ' && result.Length > 0 && result[result.Length - 1] != ' ') result.Append(c);
            }
            return result.ToString().Trim();
        }
        public int BirthOrder(CreatureId id) => names.TryGetValue(id, out var entry) ? entry.order : 0;
        // Stored base name, not string trimming: a player-entered digit remains part of their name.
        public string WorldName(CreatureId id) => names.TryGetValue(id, out var entry) ? entry.name : "Name unavailable";
        public string PersonalName(CreatureId id) => names.TryGetValue(id, out var entry) && entry.name.Length > 0
            ? (entry.order == 0 ? entry.name : Format(entry.name, entry.order)) : "";
        public string Label(CreatureId id)
        {
            string personal = PersonalName(id);
            return GenerationLoop.ShortId(id) + (personal.Length > 0 ? " · " + personal : "");
        }
        public void ValidatePrompt()
        {
            if (!HasPrompt) return;
            var record = session.Generations.Archive.Get(pending);
            if (!session.Ready || !session.Player || session.Player.Life.Id != parent || session.Player.Vitals.Dead ||
                record == null || !record.Alive) Cancel();
        }
        public bool Submit(string value)
        {
            ValidatePrompt(); if (!HasPrompt || !names.TryGetValue(pending, out var entry)) return false;
            string clean = CleanName(value); if (clean.Length > 0) entry.name = clean;
            string label = PersonalName(pending);
            Cancel(); session.ShowNotice("Named " + label, 5); return true;
        }
        public void Cancel() { pending = parent = default; IsFounderPrompt = false; if (session.Ready) session.SetPaused(false); }
    }
}
