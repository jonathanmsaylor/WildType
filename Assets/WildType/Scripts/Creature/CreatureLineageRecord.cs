using System;
using UnityEngine;

namespace WildType
{
    [Serializable]
    public struct CreatureId : IEquatable<CreatureId>, IComparable<CreatureId>
    {
        [SerializeField] string value;
        public string Value => value ?? string.Empty;
        public bool IsValid => !string.IsNullOrWhiteSpace(value);
        public static CreatureId None => default;

        public static CreatureId From(string value)
        {
            return new CreatureId { value = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim() };
        }

        public bool Equals(CreatureId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is CreatureId other && Equals(other);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);
        public int CompareTo(CreatureId other) => string.Compare(Value, other.Value, StringComparison.Ordinal);
        public override string ToString() => Value;
        public static bool operator ==(CreatureId left, CreatureId right) => left.Equals(right);
        public static bool operator !=(CreatureId left, CreatureId right) => !left.Equals(right);
    }

    [Serializable]
    public sealed class CreatureLineageRecord
    {
        public const int MutationRecordCap = 8;
        [SerializeField] CreatureId creatureId;
        [SerializeField] double birthTime;
        [SerializeField] int generation;
        [SerializeField] CreatureId firstParentId;
        [SerializeField] CreatureId secondParentId;
        [SerializeField] CreatureId founderId;
        [SerializeField] int offspringCount;
        [SerializeField] bool alive;
        [SerializeField] MutationRecord[] importantMutations;

        public CreatureId CreatureId => creatureId;
        public double BirthTime => birthTime;
        public int Generation => generation;
        public CreatureId FirstParentId => firstParentId;
        public CreatureId SecondParentId => secondParentId;
        public CreatureId FounderId => founderId;
        public int OffspringCount => offspringCount;
        public bool Alive => alive;
        public MutationRecord[] ImportantMutations => importantMutations == null ? Array.Empty<MutationRecord>() : (MutationRecord[])importantMutations.Clone();

        CreatureLineageRecord(CreatureId creatureId, double birthTime, int generation, CreatureId firstParentId,
            CreatureId secondParentId, CreatureId founderId, int offspringCount, bool alive, MutationRecord[] mutations)
        {
            this.creatureId = creatureId;
            this.birthTime = FiniteTime(birthTime);
            this.generation = Mathf.Max(0, generation);
            this.firstParentId = firstParentId;
            this.secondParentId = secondParentId;
            this.founderId = founderId;
            this.offspringCount = Mathf.Max(0, offspringCount);
            this.alive = alive;
            importantMutations = BoundedCopy(mutations);
        }

        public static CreatureLineageRecord Founder(CreatureId id, double birthTime)
        {
            RequireValid(id, nameof(id));
            return new CreatureLineageRecord(id, birthTime, 0, CreatureId.None, CreatureId.None, id, 0, true, null);
        }

        public static CreatureLineageRecord Child(CreatureId id, CreatureLineageRecord firstParent,
            CreatureLineageRecord secondParent, double birthTime, MutationRecord[] mutations)
        {
            RequireValid(id, nameof(id));
            if (firstParent == null) throw new ArgumentNullException(nameof(firstParent));
            if (secondParent == null) throw new ArgumentNullException(nameof(secondParent));
            if (firstParent.creatureId == secondParent.creatureId) throw new ArgumentException("A child requires two distinct parents.");
            if (id == firstParent.creatureId || id == secondParent.creatureId) throw new ArgumentException("A child ID must differ from both parent IDs.");
            CreatureId root = firstParent.founderId.CompareTo(secondParent.founderId) <= 0 ? firstParent.founderId : secondParent.founderId;
            int childGeneration = Mathf.Max(firstParent.generation, secondParent.generation) + 1;
            return new CreatureLineageRecord(id, birthTime, childGeneration, firstParent.creatureId,
                secondParent.creatureId, root, 0, true, mutations);
        }

        public CreatureLineageRecord WithOffspringCount(int count) =>
            new CreatureLineageRecord(creatureId, birthTime, generation, firstParentId, secondParentId,
                founderId, count, alive, importantMutations);

        public CreatureLineageRecord WithAlive(bool isAlive) =>
            new CreatureLineageRecord(creatureId, birthTime, generation, firstParentId, secondParentId,
                founderId, offspringCount, isAlive, importantMutations);

        public float AgeAt(double currentTime)
        {
            if (double.IsNaN(currentTime) || double.IsInfinity(currentTime)) return 0;
            return (float)Math.Max(0, currentTime - birthTime);
        }

        static void RequireValid(CreatureId id, string parameter)
        {
            if (!id.IsValid) throw new ArgumentException("Creature IDs cannot be empty.", parameter);
        }

        static double FiniteTime(double value) => double.IsNaN(value) || double.IsInfinity(value) ? 0 : Math.Max(0, value);

        static MutationRecord[] BoundedCopy(MutationRecord[] source)
        {
            if (source == null || source.Length == 0) return Array.Empty<MutationRecord>();
            int count = Mathf.Min(source.Length, MutationRecordCap);
            var copy = new MutationRecord[count];
            Array.Copy(source, copy, count);
            return copy;
        }
    }
}
