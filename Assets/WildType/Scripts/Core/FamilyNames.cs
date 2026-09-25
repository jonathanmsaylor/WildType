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
        CreatureId pending, parent;
        public bool ShowPrompts { get; set; } = true;
        public CreatureId Pending => pending;
        public bool HasPrompt => pending.IsValid;
        public int Count => names.Count;
        public FamilyNames(StageSession value) { session = value; }
        public void ResetRun() { names.Clear(); pending = parent = default; }
        public void RecordBirth(CreatureAgent child, CreatureAgent first, CreatureAgent second)
        {
            if (!child || names.Count >= LineageArchive.Capacity) return;
            var donor = first.IsPlayer ? first : second.IsPlayer ? second : first;
            int order = session.Generations.Archive.Get(donor.Life.Id).OffspringCount;
            names[child.Life.Id] = new Entry { order = order };
            if (!ShowPrompts || !donor.IsPlayer || donor.Vitals.Dead) return;
            pending = child.Life.Id; parent = donor.Life.Id; session.SetPaused(true);
        }
        public static string Format(string name, int order) => CleanName(name) + " " + System.Math.Max(1, order).ToString(System.Globalization.CultureInfo.InvariantCulture);
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
        public string PersonalName(CreatureId id) => names.TryGetValue(id, out var entry) && entry.name.Length > 0
            ? Format(entry.name, entry.order) : "";
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
            entry.name = CleanName(value); string label = Label(pending);
            Cancel(); session.ShowNotice(entry.name.Length > 0 ? "Named " + label : "Child keeps ID " + label, 5); return true;
        }
        public void Cancel() { pending = parent = default; if (session.Ready) session.SetPaused(false); }
    }
}
