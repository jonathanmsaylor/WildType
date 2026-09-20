using UnityEngine;
namespace WildType
{
    [RequireComponent(typeof(CreatureMotor), typeof(CreatureVitals), typeof(CreatureInteraction))]
    public sealed class CreatureAgent : MonoBehaviour
    {
        public Genome Genome { get; private set; }
        public Phenotype Stats { get; private set; }
        public CreatureVitals Vitals { get; private set; }
        public CreatureMotor Motor { get; private set; }
        public CreatureInteraction Interaction { get; private set; }
        public CreatureVisual Visual { get; private set; }
        public StageSession Session { get; private set; }
        public bool IsPlayer { get; private set; }
        public Vector3 DesiredDirection { get; set; }
        public bool WantsSprint { get; set; }
        public string Intent { get; set; } = "Exploring";
        public string State => Vitals.Dead ? "Dead" : Vitals.Energy <= 0 ? "Starving" : Vitals.SprintLocked ? "Recovering stamina" : Motor.Sprinting ? "Sprinting" : Motor.Speed > .15f ? Intent : "Resting";
        float deathTimer;
        public void Configure(Genome genome, StageSession session, bool player)
        {
            Genome = genome.Copy(); Genome.Validate(); Stats = new Phenotype(Genome);
            Session = session; IsPlayer = player;
            Vitals = GetComponent<CreatureVitals>(); Vitals.Configure(Stats);
            Motor = GetComponent<CreatureMotor>(); Motor.Configure(Stats);
            Interaction = GetComponent<CreatureInteraction>(); Interaction.Configure(this);
            Visual = GetComponentInChildren<CreatureVisual>(); Visual.Build(this);
            Vitals.Died += OnDeath;
        }
        void FixedUpdate()
        {
            if (Session == null || Session.Paused || Genome == null) return;
            Motor.Tick(DesiredDirection, WantsSprint, Vitals, Time.fixedDeltaTime);
            Vitals.Tick(Time.fixedDeltaTime, Motor.Speed, Motor.Sprinting);
            if (Vitals.Dead && !IsPlayer)
            {
                deathTimer += Time.fixedDeltaTime;
                if (deathTimer > 4) Destroy(gameObject);
            }
        }
        void OnDeath()
        {
            DesiredDirection = Vector3.zero; WantsSprint = false;
            Session.Fx.Burst(transform.position + Vector3.up, Genome.camouflage, 28, 2);
            Session.NotifyDeath(this);
        }
        void OnDestroy() { if (Vitals) Vitals.Died -= OnDeath; if (Session) Session.Unregister(this); }
    }
}
