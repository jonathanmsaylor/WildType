using System.Collections.Generic;
using UnityEngine;
namespace WildType
{
    [DefaultExecutionOrder(-20)]
    public sealed class GenerationLoop : MonoBehaviour
    {
        public const int PopulationCap = 24, ObjectCap = 32;
        public const float MateRange = 3.5f, CourtshipSeconds = 2, ParentEnergyCost = .30f;
        sealed class Pair { public CreatureAgent first, second; public float remaining = CourtshipSeconds; }
        readonly List<Pair> pairs = new List<Pair>(PopulationCap / 2);
        readonly List<CreatureAgent> descendants = new List<CreatureAgent>(PopulationCap);
        StageSession session;
        EvolutionSettings settings;
        IRandomSource random;
        int nextId;
        public double Clock { get; private set; }
        public LineageArchive Archive { get; private set; }
        public TurnoverHistory Turnover { get; } = new TurnoverHistory();
        public int Births => Turnover.Births;
        public int Deaths => Turnover.Deaths;
        public int PendingBirths => pairs.Count;
        public int PeakPopulation { get; private set; }
        public void ResetRun(StageSession value)
        {
            session = value; pairs.Clear(); descendants.Clear(); Archive = new LineageArchive();
            settings = new EvolutionSettings().ValidatedCopy(); random = new SeededRandomSource(session.seed);
            Clock = 1000; nextId = 0; Turnover.Clear(); PeakPopulation = 0;
        }
        CreatureId NextId() => CreatureId.From(session.seed + ":" + (++nextId).ToString("D3"));
        public void RegisterFounder(CreatureAgent actor)
        {
            var record = CreatureLineageRecord.Founder(NextId(), Clock - actor.Stats.MaturityAge - 15);
            Archive.Add(record); actor.AttachLife(record);
        }
        public bool Reserved(CreatureAgent actor)
        { foreach (var pair in pairs) if (pair.first == actor || pair.second == actor) return true; return false; }
        bool Owns(CreatureAgent actor)
        {
            if (!actor || actor.Session != session || !actor.Life || Archive == null || Archive.Get(actor.Life.Id) == null) return false;
            foreach (var member in session.Creatures) if (member == actor) return true;
            return false;
        }
        public string IndividualReason(CreatureAgent actor)
        {
            if (!Owns(actor)) return "Creature unavailable";
            var reason = ReproductionEligibility.CandidateReason(actor.Life.Candidate(Reserved(actor)));
            switch (reason)
            {
                case ReproductionRejection.None: return "";
                case ReproductionRejection.Juvenile: return $"Adult in {Mathf.CeilToInt(actor.Stats.MaturityAge - actor.Life.Age)}s";
                case ReproductionRejection.InsufficientEnergy: return $"Need {actor.Stats.MaxEnergy * actor.Stats.ReproductionEnergyFraction:0} energy";
                case ReproductionRejection.Cooldown: return $"Mating cooldown {Mathf.CeilToInt(actor.Life.Cooldown)}s";
                case ReproductionRejection.LowHealth: return "Need at least 60 health";
                case ReproductionRejection.Reserved: return "Courting — stay nearby";
                case ReproductionRejection.Dead: return "Creature has died";
                default: return "Invalid creature data";
            }
        }
        string CapacityReason(bool includeReservations)
        {
            int pending = includeReservations ? pairs.Count : 0;
            if (session.Population + pending >= PopulationCap) return "Population full (24) — wait for space";
            if (session.Creatures.Count + pending >= ObjectCap) return "Waiting for corpses to clear";
            if (Archive.Count + pending >= LineageArchive.Capacity) return "Lineage archive full — restart or reseed";
            return "";
        }
        string PairReason(CreatureAgent first, CreatureAgent second, bool checkReserved, bool checkRange)
        {
            if (!Owns(first) || !Owns(second) || first.Vitals.Dead || second.Vitals.Dead) return "Partner unavailable";
            if (first.Life.Age >= first.Stats.LifespanSeconds || second.Life.Age >= second.Stats.LifespanSeconds) return "Partner at end of lifespan";
            var result = ReproductionEligibility.Evaluate(first.Life.Candidate(checkReserved && Reserved(first)),
                second.Life.Candidate(checkReserved && Reserved(second)), GeneticDistance.Between(first.Genome, second.Genome),
                settings.compatibilityThreshold, session.Population, PopulationCap);
            if (!result.Eligible) return result.Rejection == ReproductionRejection.Incompatible ? "Genomes too different" : "Partner not ready: " + result.Rejection;
            if (checkRange && Vector3.Distance(first.transform.position, second.transform.position) > MateRange) return "Move within 3.5 m of partner";
            return "";
        }
        public CreatureAgent FindPartner(CreatureAgent actor, float range, bool eligibleOnly)
        {
            CreatureAgent best = null; float distance = range * range;
            foreach (var other in session.Creatures)
            {
                if (other == actor || !Owns(other) || other.Vitals.Dead) continue;
                if (eligibleOnly && PairReason(actor, other, true, false).Length > 0) continue;
                float d = (other.transform.position - actor.transform.position).sqrMagnitude;
                if (d < distance) { best = other; distance = d; }
            }
            return best;
        }
        public string PlayerHint()
        {
            if (!enabled) return "Generations disabled";
            var player = session.Player; string reason = IndividualReason(player);
            if (reason.Length > 0) return reason;
            reason = CapacityReason(true); if (reason.Length > 0) return reason;
            var partner = FindPartner(player, MateRange, true);
            if (partner) return "M / X: mate with " + ShortId(partner.Life.Id) + " (cost 30% energy)";
            partner = FindPartner(player, MateRange, false);
            if (partner) return "Nearby: " + PairReason(player, partner, true, true);
            partner = FindPartner(player, player.Stats.Vision, true);
            return partner ? $"Partner {ShortId(partner.Life.Id)} — {Vector3.Distance(player.transform.position, partner.transform.position):0} m away" : "Find a healthy, well-fed adult partner";
        }
        public bool TryPlayerMate()
        {
            var partner = FindPartner(session.Player, MateRange, true);
            if (!partner) { session.ShowNotice(PlayerHint(), 4); return false; }
            bool result = TryMate(session.Player, partner, out string reason);
            session.ShowNotice(result ? "Courtship — stay close for two seconds" : reason, 4); return result;
        }
        public bool TryMate(CreatureAgent first, CreatureAgent second, out string reason)
        {
            reason = !enabled || !session.Ready || session.Paused ? "Simulation paused or unavailable" : CapacityReason(true);
            if (reason.Length == 0) reason = PairReason(first, second, true, true);
            if (reason.Length > 0) return false;
            pairs.Add(new Pair { first = first, second = second });
            first.DesiredDirection = second.DesiredDirection = Vector3.zero;
            first.WantsSprint = second.WantsSprint = false; return true;
        }
        void FixedUpdate()
        {
            if (!session || !session.Ready || session.Paused) return;
            Clock += Time.fixedDeltaTime;
            for (int i = pairs.Count - 1; i >= 0; i--)
            {
                var pair = pairs[i]; string reason = PairReason(pair.first, pair.second, false, true);
                if (reason.Length > 0)
                { Cancel(pair, reason); pairs.RemoveAt(i); continue; }
                pair.remaining -= Time.fixedDeltaTime;
                if (pair.remaining > 0) continue;
                pairs.RemoveAt(i);
                if (!Birth(pair, out reason)) Cancel(pair, reason);
            }
            PeakPopulation = Mathf.Max(PeakPopulation, session.Population);
        }
        void Cancel(Pair pair, string reason)
        {
            if ((pair.first && pair.first.IsPlayer) || (pair.second && pair.second.IsPlayer))
                session.ShowNotice("Courtship cancelled: " + reason, 4);
        }
        bool Birth(Pair pair, out string reason)
        {
            reason = CapacityReason(false); if (reason.Length > 0) return false;
            var first = pair.first; var second = pair.second;
            // Reserve a collision-free site before sampling inheritance or spending resources.
            if (!BirthSite(first.transform.position, out var point)) { reason = "No safe nearby birth site"; return false; }
            var result = GenomeInheritance.CreateChild(first.Genome, second.Genome, settings, random);
            var mutations = new MutationRecord[result.ImportantMutations.Count];
            for (int i = 0; i < mutations.Length; i++) mutations[i] = result.ImportantMutations[i];
            var record = CreatureLineageRecord.Child(NextId(), Archive.Get(first.Life.Id), Archive.Get(second.Life.Id), Clock, mutations);
            // Single-threaded commit: both candidates were revalidated this tick; no delayed target dereferences.
            first.Vitals.SpendEnergy(first.Stats.MaxEnergy * ParentEnergyCost);
            second.Vitals.SpendEnergy(second.Stats.MaxEnergy * ParentEnergyCost);
            first.Life.BeginCooldown(); second.Life.BeginCooldown();
            Archive.Add(record, DescribeChanges(result.Genome, first.Genome, second.Genome));
            var child = session.SpawnChild(result.Genome, point, record);
            child.Vitals.SpendEnergy(child.Stats.MaxEnergy * .45f);
            Turnover.RecordBirth(record);
            if (first.IsPlayer || second.IsPlayer)
                session.ShowNotice($"Born: {ShortId(record.CreatureId)} · generation {record.Generation}" +
                    (result.MajorMutationCount > 0 ? " · notable mutation" : "") + " · F opens family", 7);
            if (Vector3.Distance(point, session.Player.transform.position) < 30)
                session.Fx.Burst(point + Vector3.up, child.Genome.camouflage, 12, .8f);
            return true;
        }
        bool BirthSite(Vector3 center, out Vector3 point)
        {
            for (int i = 0; i < 16; i++)
            {
                float angle = i * Mathf.PI * .5f + i * .17f;
                point = center + new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * (2 + i / 4);
                if (new Vector2(point.x, point.z).magnitude > Ecosystem.PlayRadius - 3) continue;
                if (!Physics.Raycast(point + Vector3.up * 12, Vector3.down, out var hit, 24, Ecosystem.GroundMask) || hit.normal.y < .75f) continue;
                point = hit.point + Vector3.up * .15f;
                if (Physics.CheckCapsule(point + Vector3.up * .5f, point + Vector3.up * 2, .9f, Ecosystem.ObstacleMask)) continue;
                bool occupied = false;
                foreach (var creature in session.Creatures)
                    if (creature && (creature.transform.position - point).sqrMagnitude < 2.25f) { occupied = true; break; }
                if (!occupied) return true;
            }
            point = default; return false;
        }
        public void MarkDead(CreatureAgent actor)
        {
            if (!Owns(actor)) return;
            var record = Archive.Get(actor.Life.Id);
            if (!record.Alive) return; // Death notification and subsequent corpse cleanup count only once.
            Archive.MarkDead(actor.Life.Id);
            // Destruction/unregister alone is not evidence of gameplay death. Restarts are not deaths either.
            if (session.Ready && actor.Vitals.Dead) Turnover.RecordDeath(record, actor.Vitals.DeathCause);
        }
        public IReadOnlyList<CreatureAgent> LivingDescendants(CreatureId ancestor)
        {
            descendants.Clear();
            foreach (var actor in session.Creatures)
                if (Owns(actor) && !actor.Vitals.Dead && Archive.IsDescendant(actor.Life.Id, ancestor)) descendants.Add(actor);
            return descendants;
        }
        public int LivingChildren(CreatureId parent)
        {
            int count = 0;
            foreach (var actor in session.Creatures)
            {
                if (!Owns(actor) || actor.Vitals.Dead) continue;
                var record = Archive.Get(actor.Life.Id);
                if (record.FirstParentId == parent || record.SecondParentId == parent) count++;
            }
            return count;
        }
        public bool IsLivingDescendant(CreatureAgent actor, CreatureId ancestor) => Owns(actor) && !actor.Vitals.Dead && Archive.IsDescendant(actor.Life.Id, ancestor);
        public static string ShortId(CreatureId id)
        { int colon = id.Value.LastIndexOf(':'); return id.IsValid ? "#" + id.Value.Substring(colon + 1) : "—"; }
        static string DescribeChanges(Genome child, Genome first, Genome second)
        {
            GenomeGene best = GenomeGene.BodySize; float largest = -1;
            for (int i = 0; i < GenomeGeneCatalog.Count; i++)
            {
                var gene = GenomeGeneCatalog.At(i);
                float mid = (GenomeGeneCatalog.Get(first, gene) + GenomeGeneCatalog.Get(second, gene)) * .5f;
                float delta = Mathf.Abs(GenomeGeneCatalog.Get(child, gene) - mid) / GenomeGeneCatalog.Span(gene);
                if (delta > largest) { largest = delta; best = gene; }
            }
            float average = (GenomeGeneCatalog.Get(first, best) + GenomeGeneCatalog.Get(second, best)) * .5f;
            return $"{GenomeGeneCatalog.Name(best)} {GenomeGeneCatalog.Get(child, best):0.00} (parents avg {average:0.00})";
        }
    }
}
