using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace WildType
{
    public sealed class StageSession : MonoBehaviour
    {
        public GenomePreset[] presets;
        public CreatureAgent creaturePrefab;
        public FoodPlant foodPrefab;
        public Material prototypeMaterial, particleMaterial;
        public TMP_FontAsset hudFont;
        public OrbitCamera orbit;
        public int seed = 917430;
        public bool autoPauseOnFocusLoss = true;
        public const int AICount = 12;
        readonly List<CreatureAgent> creatures = new List<CreatureAgent>(AICount + 1);
        public IReadOnlyList<CreatureAgent> Creatures => creatures;
        public CreatureAgent Player { get; private set; }
        public Ecosystem World { get; private set; }
        public GenerationLoop Generations { get; private set; }
        public FamilyCare Care { get; private set; }
        public FamilyNames Names { get; private set; }
        public FeedbackPool Fx { get; private set; }
        public Transform RuntimeRoot { get; private set; }
        public bool Paused { get; private set; }
        public bool GameOver { get; private set; }
        public bool Ready { get; private set; }
        public string Notice { get; private set; }
        float noticeUntil;
        bool restarting;
        public int Population { get { int n = 0; foreach (var a in creatures) if (a && !a.Vitals.Dead) n++; return n; } }
        void Awake()
        {
            Time.timeScale = 1;
            World = GetComponent<Ecosystem>();
            Generations = gameObject.AddComponent<GenerationLoop>();
            Care = gameObject.AddComponent<FamilyCare>();
            Names = new FamilyNames(this);
            Fx = new GameObject("Bounded feedback pool").AddComponent<FeedbackPool>(); Fx.transform.SetParent(transform); Fx.Configure(particleMaterial);
            GetComponent<StageHud>().Configure(this);
        }
        void Start() { Begin(); }
        void Begin()
        {
            RuntimeRoot = new GameObject("Runtime ecosystem").transform;
            Generations.ResetRun(this);
            Care.ResetRun(this);
            Names.ResetRun();
            World.Populate(this, seed);
            var random = new System.Random(seed);
            Player = Spawn(presets[0].RuntimeCopy(), new Vector3(0, Ecosystem.Height(0, 0) + .2f, 0), true);
            for (int i = 0; i < AICount; i++)
            {
                Vector3 point = Ecosystem.RandomGround(random, 12, 64);
                if (i == 0) point = new Vector3(4, Ecosystem.Height(4, 4), 4);
                for (int retry = 0; retry < 30 && Physics.CheckSphere(point + Vector3.up, 2, Ecosystem.ObstacleMask); retry++) point = Ecosystem.RandomGround(random, 12, 64);
                var actor = Spawn(Genome.Varied(presets[i % presets.Length].RuntimeCopy(), random), point + Vector3.up * .3f, false);
                actor.gameObject.AddComponent<HerbivoreBrain>().Configure(actor, seed + i * 107);
            }
            orbit.Configure(Player, this);
            GetComponent<PlayerInputBridge>().Configure(this);
            Ready = true; SetPaused(false); ShowNotice("Explore, forage and grow a lineage.", 5);
            Debug.Log("WILDTYPE ready | seed " + seed + " | creatures " + Population + " | food cap " + Ecosystem.FoodCap);
        }
        CreatureAgent Spawn(Genome genome, Vector3 at, bool isPlayer)
        {
            var actor = Instantiate(creaturePrefab, at, Quaternion.identity, RuntimeRoot);
            actor.name = isPlayer ? "Player Creature" : "Herbivore " + creatures.Count;
            actor.Configure(genome, this, isPlayer); creatures.Add(actor); Generations.RegisterFounder(actor); return actor;
        }
        internal CreatureAgent SpawnChild(Genome genome, Vector3 at, CreatureLineageRecord record)
        {
            var actor = Instantiate(creaturePrefab, at, Quaternion.identity, RuntimeRoot);
            actor.name = "Descendant " + GenerationLoop.ShortId(record.CreatureId);
            actor.Configure(genome, this, false); actor.AttachLife(record); creatures.Add(actor);
            actor.gameObject.AddComponent<HerbivoreBrain>().Configure(actor, seed + Generations.Births * 107 + 31);
            return actor;
        }
        public bool TakeControl(CreatureAgent descendant)
        {
            if (!Ready || !Player || !descendant || descendant.Session != this || descendant.Vitals.Dead || !descendant.Life ||
                !Generations.IsLivingDescendant(descendant, Player.Life.Id)) return false;
            Player.SetPlayer(false); descendant.SetPlayer(true); Player = descendant;
            Care.ClearTracking();
            Names.Cancel();
            GameOver = false; SetPaused(false); orbit.Configure(Player, this);
            ShowNotice("Now controlling " + GenerationLoop.ShortId(Player.Life.Id) + " · resources and age preserved", 6);
            return true;
        }
        public void SetPaused(bool paused)
        {
            if (GameOver && !paused) return;
            Paused = paused; Time.timeScale = paused ? 0 : 1;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked; Cursor.visible = paused;
            if (Player) { Player.DesiredDirection = Vector3.zero; Player.WantsSprint = false; }
        }
        public void NotifyDeath(CreatureAgent actor)
        {
            Generations.MarkDead(actor);
            if (actor.IsPlayer) { GameOver = true; SetPaused(true); ShowNotice("Your creature has died.", 60); }
        }
        public void Unregister(CreatureAgent actor) { if (Generations) Generations.MarkDead(actor); creatures.Remove(actor); }
        public void ShowNotice(string text, float duration) { Notice = text; noticeUntil = Time.unscaledTime + duration; }
        public string CurrentNotice => Time.unscaledTime < noticeUntil ? Notice : "";
        public void Restart(bool reseed)
        {
            if (restarting) return;
            StartCoroutine(Rebuild(reseed));
        }
        IEnumerator Rebuild(bool reseed)
        {
            restarting = true; Ready = false; GameOver = false; Paused = false; Time.timeScale = 1;
            if (RuntimeRoot) Destroy(RuntimeRoot.gameObject);
            creatures.Clear(); Player = null; Fx.Clear();
            if (reseed) seed = unchecked(seed + 7919);
            yield return null; // Destruction completes before spawning the next bounded population.
            Begin(); restarting = false;
        }
        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        void OnApplicationFocus(bool focus) { if (!focus && autoPauseOnFocusLoss && Ready) SetPaused(true); }
        void OnDestroy()
        {
            Time.timeScale = 1; Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            if (RuntimeRoot) Destroy(RuntimeRoot.gameObject);
        }
    }
}
