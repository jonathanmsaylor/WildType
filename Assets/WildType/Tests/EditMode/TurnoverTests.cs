using NUnit.Framework;
using UnityEngine;
namespace WildType.Tests
{
    public sealed class TurnoverTests
    {
        [Test] public void ReplacementBirthRetainsDeathIdentityAndLineage()
        {
            var history = new TurnoverHistory();
            var a = CreatureLineageRecord.Founder(CreatureId.From("seed:001"), 0);
            var b = CreatureLineageRecord.Founder(CreatureId.From("seed:002"), 0);
            var child = CreatureLineageRecord.Child(CreatureId.From("seed:003"), a, b, 1, null);
            history.RecordDeath(a, CreatureDeathCause.Starvation); history.RecordBirth(child);
            Assert.AreEqual(1, history.Deaths); Assert.AreEqual(1, history.Births);
            Assert.AreEqual(TurnoverKind.Birth, history.Recent[0].Kind);
            Assert.AreEqual(child.CreatureId, history.Recent[0].CreatureId);
            Assert.AreEqual(1, history.Recent[0].Generation);
            Assert.AreEqual(a.CreatureId, history.Recent[0].FirstParentId);
            Assert.AreEqual(b.CreatureId, history.Recent[0].SecondParentId);
            Assert.AreEqual(a.FounderId, history.Recent[0].FounderId);
            Assert.AreEqual(a.CreatureId, history.Recent[1].CreatureId);
            Assert.AreEqual(CreatureDeathCause.Starvation, history.Recent[1].Cause);
        }
        [Test] public void HistoryIsBoundedWhileTotalsPersistAndClearResetsEverything()
        {
            var history = new TurnoverHistory();
            history.RecordBirth(null); history.RecordDeath(null, CreatureDeathCause.Unknown);
            Assert.AreEqual(0, history.Recent.Count);
            for (int i = 0; i < 40; i++)
            {
                var record = CreatureLineageRecord.Founder(CreatureId.From(i.ToString()), 0);
                history.RecordBirth(record); history.RecordDeath(record, CreatureDeathCause.Unknown);
                Assert.LessOrEqual(history.Recent.Count, TurnoverHistory.Capacity);
            }
            Assert.AreEqual(40, history.Births); Assert.AreEqual(40, history.Deaths);
            Assert.AreEqual("39", history.Recent[0].CreatureId.Value);
            Assert.AreEqual("38", history.Recent[3].CreatureId.Value);
            history.Clear(); Assert.AreEqual(0, history.Births); Assert.AreEqual(0, history.Deaths);
            Assert.AreEqual(0, history.Recent.Count);
        }
        [Test] public void CauseComesFromFatalDamageAndIsAvailableInsideDeathCallback()
        {
            var obj = new GameObject("Cause test");
            try
            {
                var vitals = obj.AddComponent<CreatureVitals>(); var stats = new Phenotype(new Genome());
                CreatureDeathCause observed = CreatureDeathCause.Unknown; int notifications = 0;
                vitals.Died += () => { observed = vitals.DeathCause; notifications++; };
                vitals.Configure(stats); vitals.SpendEnergy(vitals.Energy); vitals.Tick(1, 0, false);
                Assert.False(vitals.Dead); Assert.AreEqual(CreatureDeathCause.Unknown, vitals.DeathCause);
                vitals.Damage(100); // Low energy/nonfatal starvation must not cause a guessed fatal label.
                Assert.AreEqual(CreatureDeathCause.Unknown, observed);
                vitals.Damage(100, CreatureDeathCause.OldAge); Assert.AreEqual(1, notifications);
                Assert.AreEqual(CreatureDeathCause.Unknown, vitals.DeathCause);
                vitals.Configure(stats); vitals.Tick(10000, 0, false);
                Assert.AreEqual(CreatureDeathCause.Starvation, observed);
                vitals.Configure(stats); vitals.Damage(100, CreatureDeathCause.OldAge);
                Assert.AreEqual(CreatureDeathCause.OldAge, observed); Assert.AreEqual(3, notifications);
                vitals.Configure(stats); Assert.AreEqual(CreatureDeathCause.Unknown, vitals.DeathCause);
            }
            finally { Object.DestroyImmediate(obj); }
        }
    }
}
