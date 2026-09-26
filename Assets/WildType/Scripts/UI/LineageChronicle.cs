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
            if (archive.IsDescendant(record.CreatureId, focus)) return "Descendant";
            if (archive.IsDescendant(focus, record.CreatureId)) return "Ancestor";
            if (current != null && (SharedParent(current.FirstParentId, record) || SharedParent(current.SecondParentId, record))) return "Sibling";
            return "Other family branch"; // Not a descendant/ancestor; parent IDs retain the exact links.
        }
        static bool SharedParent(CreatureId id, CreatureLineageRecord record) => id.IsValid &&
            (id == record.FirstParentId || id == record.SecondParentId);
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
            string place = died ? $"Lived {JournalReadout.Whole(record.AgeAt(death.Time))}s" : actor ? session.World.Zone(actor.transform.position) + " · " +
                JournalReadout.Distance(Vector3.Distance(session.Player.transform.position, actor.transform.position)) + " away" : "No location recorded";
            return $"<b>{session.Names.PersonalName(record.CreatureId)}</b> · Gen {record.Generation}\n" + relation + " · " + state + "\n" + place;
        }
    }
}
