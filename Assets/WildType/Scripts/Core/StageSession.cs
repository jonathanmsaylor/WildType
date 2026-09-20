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
            Fx = new GameObject("Bounded feedback pool").AddComponent<FeedbackPool>(); Fx.transform.SetParent(transform); Fx.Configure(particleMaterial);
            GetComponent<StageHud>().Configure(this);
        }
        void Start() { Begin(); }
        void Begin()
        {
            RuntimeRoot = new GameObject("Runtime ecosystem").transform;
            World.Populate(this, seed);
            var random = new System.Random(seed);
            Player = Spawn(presets[0].RuntimeCopy(), new Vector3(0, Ecosystem.Height(0, 0) + .2f, 0), true);
            for (int i = 0; i < AICount; i++)
            {
                Vector3 point = Ecosystem.RandomGround(random, 12, 64);
                for (int retry = 0; retry < 30 && Physics.CheckSphere(point + Vector3.up, 2, Ecosystem.ObstacleMask); retry++) point = Ecosystem.RandomGround(random, 12, 64);
                var actor = Spawn(Genome.Varied(presets[i % presets.Length].RuntimeCopy(), random), point + Vector3.up * .3f, false);
                actor.gameObject.AddComponent<HerbivoreBrain>().Configure(actor, seed + i * 107);
            }
            orbit.Configure(Player, this);
            GetComponent<PlayerInputBridge>().Configure(this);
            Ready = true; SetPaused(false); ShowNotice("Explore, eat, and survive.", 7);
            Debug.Log("WILDTYPE ready | seed " + seed + " | creatures " + Population + " | food cap " + Ecosystem.FoodCap);
        }
        CreatureAgent Spawn(Genome genome, Vector3 at, bool isPlayer)
        {
            var actor = Instantiate(creaturePrefab, at, Quaternion.identity, RuntimeRoot);
            actor.name = isPlayer ? "Player Creature" : "Herbivore " + creatures.Count;
            actor.Configure(genome, this, isPlayer); creatures.Add(actor); return actor;
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
            if (actor.IsPlayer) { GameOver = true; SetPaused(true); ShowNotice("Your creature has died.", 60); }
        }
        public void Unregister(CreatureAgent actor) { creatures.Remove(actor); }
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
