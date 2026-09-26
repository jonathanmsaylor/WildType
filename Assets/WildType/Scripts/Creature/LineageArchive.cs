using System.Collections.Generic;
namespace WildType
{
    // Records outlive GameObjects. A hard archive limit retains every parent reference;
    // it never silently prunes ancestors or grows without bound.
    public sealed class LineageArchive
    {
        public const int Capacity = 512;
        readonly Dictionary<CreatureId, CreatureLineageRecord> records = new Dictionary<CreatureId, CreatureLineageRecord>();
        readonly Dictionary<CreatureId, string> changes = new Dictionary<CreatureId, string>();
        readonly Dictionary<CreatureId, LineageDeath> deaths = new Dictionary<CreatureId, LineageDeath>();
        readonly List<CreatureLineageRecord> birthOrder = new List<CreatureLineageRecord>(Capacity);
        public int Revision { get; private set; }
        public int Count => records.Count;
        public CreatureLineageRecord Get(CreatureId id) => records.TryGetValue(id, out var value) ? value : null;
        public string Changes(CreatureId id) => changes.TryGetValue(id, out var value) ? value : "Founder traits";
        public bool Add(CreatureLineageRecord record, string summary = "Founder traits")
        {
            if (record == null || !record.CreatureId.IsValid || Count >= Capacity || records.ContainsKey(record.CreatureId)) return false;
            if (record.Generation > 0 && (Get(record.FirstParentId) == null || Get(record.SecondParentId) == null)) return false;
            records.Add(record.CreatureId, record); changes.Add(record.CreatureId, summary);
            birthOrder.Add(record); Revision++;
            foreach (var id in new[] { record.FirstParentId, record.SecondParentId })
                if (records.TryGetValue(id, out var parent)) records[id] = parent.WithOffspringCount(parent.OffspringCount + 1);
            return true;
        }
        public void MarkDead(CreatureId id)
        { if (records.TryGetValue(id, out var record) && record.Alive) { records[id] = record.WithAlive(false); Revision++; } }
        // Only the actual fatal-damage notification calls this. Object disappearance is not death evidence.
        public bool RecordDeath(CreatureId id, CreatureDeathCause cause, double time)
        {
            var record = Get(id);
            if (record == null || deaths.ContainsKey(id) || double.IsNaN(time) || double.IsInfinity(time) || time < record.BirthTime) return false;
            deaths.Add(id, new LineageDeath(time, cause)); MarkDead(id); Revision++; return true;
        }
        public bool TryDeath(CreatureId id, out LineageDeath death) => deaths.TryGetValue(id, out death);
        // Read-only tree projection. Return current records, not stale birth-time alive/count snapshots.
        public void CollectChildren(CreatureId parent, List<CreatureLineageRecord> destination)
        {
            destination.Clear(); if (!parent.IsValid || Get(parent) == null) return;
            foreach (var original in birthOrder)
            {
                var record = Get(original.CreatureId);
                if (record.FirstParentId == parent || record.SecondParentId == parent) destination.Add(record);
            }
        }
        // Parents must already exist when Add succeeds, so this bounded forward pass finds all descendants.
        // Include actual ancestors, not unrelated creatures sharing the canonical founder ID.
        public void CollectFamily(CreatureId focus, List<CreatureLineageRecord> destination, IEnumerable<CreatureId> earlierControls = null)
        {
            destination.Clear(); if (Get(focus) == null) return;
            var ancestors = new HashSet<CreatureId>(); var pending = new Stack<CreatureId>(); pending.Push(focus);
            while (pending.Count > 0)
            {
                var id = pending.Pop(); if (!ancestors.Add(id)) continue;
                var record = Get(id); if (record == null) continue;
                if (record.FirstParentId.IsValid) pending.Push(record.FirstParentId);
                if (record.SecondParentId.IsValid) pending.Push(record.SecondParentId);
            }
            var descendants = new HashSet<CreatureId> { focus };
            if (earlierControls != null) foreach (var id in earlierControls) if (Get(id) != null) descendants.Add(id);
            foreach (var original in birthOrder)
            {
                var record = Get(original.CreatureId);
                if (descendants.Contains(record.FirstParentId) || descendants.Contains(record.SecondParentId)) descendants.Add(record.CreatureId);
                if (ancestors.Contains(record.CreatureId) || descendants.Contains(record.CreatureId)) destination.Add(record);
            }
        }
        public bool InFamilyHistory(CreatureId id, CreatureId focus, IEnumerable<CreatureId> earlierControls)
        {
            if (Get(id) == null || Get(focus) == null) return false;
            if (id == focus || IsDescendant(focus, id) || IsDescendant(id, focus)) return true;
            if (earlierControls != null) foreach (var previous in earlierControls)
                if (Get(previous) != null && (id == previous || IsDescendant(id, previous))) return true;
            return false;
        }
        public bool IsDescendant(CreatureId child, CreatureId ancestor)
        {
            if (!child.IsValid || !ancestor.IsValid || child == ancestor) return false;
            var visited = new HashSet<CreatureId>(); var pending = new Stack<CreatureId>(); pending.Push(child);
            while (pending.Count > 0)
            {
                var id = pending.Pop(); if (!visited.Add(id)) continue;
                var record = Get(id); if (record == null) continue;
                if (record.FirstParentId == ancestor || record.SecondParentId == ancestor) return true;
                if (record.FirstParentId.IsValid) pending.Push(record.FirstParentId);
                if (record.SecondParentId.IsValid) pending.Push(record.SecondParentId);
            }
            return false;
        }
    }
    public readonly struct LineageDeath
    {
        public double Time { get; }
        public CreatureDeathCause Cause { get; }
        public LineageDeath(double time, CreatureDeathCause cause)
        {
            Time = time;
            Cause = cause == CreatureDeathCause.Starvation || cause == CreatureDeathCause.OldAge ? cause : CreatureDeathCause.Unknown;
        }
    }
}
