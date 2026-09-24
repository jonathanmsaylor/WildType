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
        public int Count => records.Count;
        public CreatureLineageRecord Get(CreatureId id) => records.TryGetValue(id, out var value) ? value : null;
        public string Changes(CreatureId id) => changes.TryGetValue(id, out var value) ? value : "Founder traits";
        public bool Add(CreatureLineageRecord record, string summary = "Founder traits")
        {
            if (record == null || !record.CreatureId.IsValid || Count >= Capacity || records.ContainsKey(record.CreatureId)) return false;
            if (record.Generation > 0 && (Get(record.FirstParentId) == null || Get(record.SecondParentId) == null)) return false;
            records.Add(record.CreatureId, record); changes.Add(record.CreatureId, summary);
            foreach (var id in new[] { record.FirstParentId, record.SecondParentId })
                if (records.TryGetValue(id, out var parent)) records[id] = parent.WithOffspringCount(parent.OffspringCount + 1);
            return true;
        }
        public void MarkDead(CreatureId id)
        { if (records.TryGetValue(id, out var record) && record.Alive) records[id] = record.WithAlive(false); }
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
}
