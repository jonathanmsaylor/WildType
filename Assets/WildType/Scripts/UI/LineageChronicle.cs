using UnityEngine;
namespace WildType
{
    // Journal-only presentation. No survival, identity, ancestry or control rules are changed here.
    public static class LineageChronicle
    {
        public static string Relationship(LineageArchive archive, CreatureId focus, CreatureLineageRecord record)
        {
            if (record.CreatureId == focus) return "You";
            var current = archive.Get(focus);
            if (current != null && (current.FirstParentId == record.CreatureId || current.SecondParentId == record.CreatureId)) return "Parent";
            if (record.FirstParentId == focus || record.SecondParentId == focus) return "Child";
            return archive.IsDescendant(record.CreatureId, focus) ? "Descendant" : "Ancestor";
        }
        public static CreatureAgent LivingActor(StageSession session, CreatureLineageRecord record)
        {
            if (!record.Alive || session.Generations.Archive.TryDeath(record.CreatureId, out _)) return null;
            foreach (var actor in session.Creatures)
                if (actor && actor.Life.Id == record.CreatureId && !actor.Vitals.Dead && session.Generations.Owns(actor)) return actor;
            return null;
        }
        public static string Cause(CreatureDeathCause cause) => cause == CreatureDeathCause.Starvation ? "starvation" :
            cause == CreatureDeathCause.OldAge ? "old age" : "cause unknown";
        public static string Card(StageSession session, CreatureLineageRecord record, CreatureAgent actor, bool canControl)
        {
            var archive = session.Generations.Archive;
            string relation = Relationship(archive, session.Player.Life.Id, record);
            bool died = archive.TryDeath(record.CreatureId, out var death);
            string state = died ? "Deceased · " + Cause(death.Cause) : actor ? (actor.Life.Adult ? "Living adult" : "Living juvenile") : "Unavailable · no recorded death";
            string time = died ? $"Lived {record.AgeAt(death.Time):0}s" : actor ? $"Age {actor.Life.Age:0}s" : "Birth recorded";
            string parents = record.Generation == 0 ? "Founder" : "Parents " + GenerationLoop.ShortId(record.FirstParentId) + " + " + GenerationLoop.ShortId(record.SecondParentId);
            return $"<size=20>{session.Names.Label(record.CreatureId)} · Gen {record.Generation}</size>\n" +
                relation + " · " + state + "\n" + parents + $" · Children born {record.OffspringCount}\n" +
                time + (canControl ? " · select to control" : " · record only");
        }
    }
}
