using System.Collections.Generic;
namespace WildType
{
    public enum TurnoverKind { Birth, Death }

    // Value-only snapshots survive corpse cleanup and changes of player control.
    public readonly struct TurnoverEvent
    {
        public TurnoverKind Kind { get; }
        public CreatureId CreatureId { get; }
        public int Generation { get; }
        public CreatureId FirstParentId { get; }
        public CreatureId SecondParentId { get; }
        public CreatureId FounderId { get; }
        public CreatureDeathCause Cause { get; }
        public TurnoverEvent(TurnoverKind kind, CreatureLineageRecord record, CreatureDeathCause cause)
        {
            Kind = kind; CreatureId = record.CreatureId; Generation = record.Generation;
            FirstParentId = record.FirstParentId; SecondParentId = record.SecondParentId;
            FounderId = record.FounderId; Cause = cause;
        }
    }

    public sealed class TurnoverHistory
    {
        public const int Capacity = 4;
        readonly List<TurnoverEvent> events = new List<TurnoverEvent>(Capacity);
        public IReadOnlyList<TurnoverEvent> Recent { get; }
        public int Births { get; private set; }
        public int Deaths { get; private set; }
        public TurnoverHistory() { Recent = events.AsReadOnly(); }
        public void Clear() { events.Clear(); Births = Deaths = 0; }
        public void RecordBirth(CreatureLineageRecord record)
        {
            if (record == null) return;
            Births++; Add(new TurnoverEvent(TurnoverKind.Birth, record, CreatureDeathCause.Unknown));
        }
        public void RecordDeath(CreatureLineageRecord record, CreatureDeathCause cause)
        {
            if (record == null) return;
            Deaths++; Add(new TurnoverEvent(TurnoverKind.Death, record, cause));
        }
        void Add(TurnoverEvent entry)
        {
            if (events.Count == Capacity) events.RemoveAt(events.Count - 1);
            events.Insert(0, entry);
        }
    }
}
