using System;
using Animas.Core;
using Animas.Items;
using Animas.Levels;
using Animas.Player;
using Animas.Save;
using UnityEngine;

namespace Animas.Entities
{
    // La Llorona (GDD §13 and §21): Hidden → Stalk (drifts along the canals) → (the Padre comes close)
    // → Chase → (reaches him) → Grab (drains life) → Retreat (sinks into the water) → Hidden → reappears
    // by the water. Salt lines and the prayer circle push her back, a veladora's light scares her off and
    // holy water stuns her: all of them send her under the water for a while.
    // She can't be killed with weapons: she's beaten by finishing the ritual (Completed → Dissolve).
    // She's a ghost: she glides in a straight line over water and land, so she uses no NavMesh.
    [RequireComponent(typeof(SpiritBody), typeof(Health))]
    public class LloronaEntity : MonoBehaviour
    {
        public enum State { Hidden, Stalk, Chase, Grab, Retreat, Dissolved }

        [SerializeField] LloronaTuning tuning;
        [Tooltip("Points over the water of the canals. She drifts between them and reappears at them.")]
        [SerializeField] Transform[] canalPoints;
        [Tooltip("The ritual she defends. When it completes, she dissolves.")]
        [SerializeField] RitualController ritual;
        [Tooltip("Shown when she dissolves (the note with the silver cross recipe). Starts hidden.")]
        [SerializeField] GameObject rewardNote;
        [Tooltip("The model and its lights: hidden while she's under the water.")]
        [SerializeField] GameObject visuals;
        [SerializeField] Animator animator;
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip screamClip;
        [Tooltip("Seconds the dissolve takes (she sinks and fades).")]
        [SerializeField, Min(0.1f)] float dissolveSeconds = 2.5f;

        public State Current { get; private set; } = State.Hidden;
        public event Action<State> StateChanged;

        SpiritBody body;
        Health health;
        Collider[] colliders;
        Transform player;
        Health playerHealth;
        PlayerController playerMovement;
        PlayerRespawn playerRespawn;
        Inventory messages;

        int pointIndex;
        float stateUntil;
        float baseY;
        bool hasSpeedParam;

        static readonly int SpeedParam = Animator.StringToHash("Speed");

        void Awake()
        {
            body = GetComponent<SpiritBody>();
            health = GetComponent<Health>();
            colliders = GetComponentsInChildren<Collider>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                playerHealth = p.GetComponent<Health>();
                playerMovement = p.GetComponent<PlayerController>();
                playerRespawn = p.GetComponent<PlayerRespawn>();
                messages = p.GetComponent<Inventory>();
            }
            baseY = transform.position.y;
            if (rewardNote != null) rewardNote.SetActive(false);
            // The placeholder model may come without an Animator Controller: don't spam warnings then.
            hasSpeedParam = animator != null && animator.runtimeAnimatorController != null
                && Array.Exists(animator.parameters, param => param.nameHash == SpeedParam);
            if (canalPoints == null || canalPoints.Length == 0)
            {
                Debug.LogError("[LloronaEntity] She needs canal points to move between.", this);
                enabled = false;
            }
        }

        void OnEnable()
        {
            body.Repelled += Scare;
            body.Stunned += OnStunned;
            body.Revealed += Scare;
            health.Damaged += OnHurt;
            if (playerRespawn != null) playerRespawn.Respawned += OnPlayerRespawned;
            if (ritual != null) ritual.Completed += Dissolve;
        }

        void OnDisable()
        {
            body.Repelled -= Scare;
            body.Stunned -= OnStunned;
            body.Revealed -= Scare;
            health.Damaged -= OnHurt;
            if (playerRespawn != null) playerRespawn.Respawned -= OnPlayerRespawned;
            if (ritual != null) ritual.Completed -= Dissolve;
        }

        void Start() => Enter(State.Hidden, tuning.hiddenSeconds * 0.5f);

        void Update()
        {
            if (player == null || Current == State.Dissolved) return;

            switch (Current)
            {
                case State.Hidden: UpdateHidden(); break;
                case State.Stalk: UpdateStalk(); break;
                case State.Chase: UpdateChase(); break;
                case State.Grab: UpdateGrab(); break;
                case State.Retreat: UpdateRetreat(); break;
            }
        }

        void UpdateHidden()
        {
            if (Time.time < stateUntil) return;
            // She never surfaces next to the Padre, or while he's safe in the choza.
            if (!PlayerHuntable()) return;
            Transform point = ReappearPoint();
            if (point == null) return;
            MoveTo(point.position);
            Enter(State.Stalk, Time.time + tuning.waitAtPoint);
        }

        void UpdateStalk()
        {
            if (PlayerHuntable() && (DistanceToPlayer() <= tuning.detectRadius || RitualUnderway()))
            {
                Enter(State.Chase, 0f);
                return;
            }
            Transform target = canalPoints[pointIndex];
            if (Glide(target.position, tuning.stalkSpeed)) return;
            // At the point: wait a moment, then drift to the canal point closest to the Padre
            // (if she's already there, she stays watching him from the water).
            if (Time.time < stateUntil) return;
            pointIndex = ClosestPoint(player.position);
            stateUntil = Time.time + tuning.waitAtPoint;
        }

        void UpdateChase()
        {
            // During the ritual she always knows where the Padre is (GDD §13: "mientras ella ataca").
            bool lost = DistanceToPlayer() > tuning.loseRadius && !RitualUnderway();
            if (!PlayerHuntable() || lost)
            {
                Enter(State.Retreat, 0f);
                return;
            }
            if (body.IsStunned) return;
            Glide(player.position, tuning.chaseSpeed);
            if (DistanceToPlayer() <= tuning.grabRange) StartGrab();
        }

        void StartGrab()
        {
            Enter(State.Grab, Time.time + tuning.grabSeconds);
            if (audioSource != null && screamClip != null) audioSource.PlayOneShot(screamClip);
            if (messages != null) messages.Notify("¡La Llorona te atrapó!");
            // One hit of 15: the ritual takes it as a strong hit and pushes the bar back (GDD P3).
            playerHealth.TakeDamage(tuning.grabDamage);
        }

        void UpdateGrab()
        {
            if (playerHealth.IsDead)
            {
                Release();
                return;
            }
            // The Padre can't walk away while she holds him; the drain is small and continuous.
            if (playerMovement != null) playerMovement.SpeedMultiplier = 0f;
            playerHealth.TakeDamage(tuning.drainPerSecond * Time.deltaTime);
            if (Time.time >= stateUntil) Release();
        }

        void Release()
        {
            if (playerMovement != null) playerMovement.SpeedMultiplier = 1f;
            Enter(State.Retreat, 0f);
        }

        void UpdateRetreat()
        {
            // She glides back to the nearest canal and sinks there.
            Transform point = canalPoints[ClosestPoint(transform.position)];
            if (Glide(point.position, tuning.chaseSpeed)) return;
            Enter(State.Hidden, Time.time + tuning.hiddenSeconds);
        }

        // Salt line, prayer circle, veladora light: she screams and goes under the water.
        void Scare()
        {
            if (Current == State.Hidden || Current == State.Retreat || Current == State.Dissolved) return;
            if (Current == State.Grab && playerMovement != null) playerMovement.SpeedMultiplier = 1f;
            if (audioSource != null && screamClip != null) audioSource.PlayOneShot(screamClip);
            // Straight under: running back to the canal through the ward would look odd.
            Enter(State.Hidden, Time.time + tuning.hiddenSeconds);
        }

        // Holy water interrupts her attack (GDD §18): the grab breaks and she flees.
        void OnStunned(float seconds) => Scare();

        // Weapons don't kill her (GDD §13: "se vence con crafteo y rezo"), but a blessed hit scares her.
        void OnHurt(float amount)
        {
            health.Heal(amount);
            Scare();
        }

        void OnPlayerRespawned()
        {
            if (Current == State.Dissolved) return;
            if (playerMovement != null) playerMovement.SpeedMultiplier = 1f;
            Enter(State.Hidden, Time.time + tuning.hiddenSeconds);
        }

        // The ritual is complete: she sinks and fades for good, and leaves her note on the water's edge.
        void Dissolve()
        {
            if (Current == State.Grab && playerMovement != null) playerMovement.SpeedMultiplier = 1f;
            Enter(State.Dissolved, 0f);
            if (audioSource != null && screamClip != null) audioSource.PlayOneShot(screamClip);
            if (rewardNote != null) rewardNote.SetActive(true);
            if (messages != null) messages.Notify("La Llorona se disuelve en el agua...");
            StartCoroutine(SinkAndFade());
        }

        System.Collections.IEnumerator SinkAndFade()
        {
            Vector3 start = transform.position;
            Vector3 startScale = visuals != null ? visuals.transform.localScale : Vector3.one;
            for (float t = 0f; t < 1f; t += Time.deltaTime / dissolveSeconds)
            {
                transform.position = start + Vector3.down * (2f * t);
                if (visuals != null) visuals.transform.localScale = startScale * (1f - 0.7f * t);
                yield return null;
            }
            SetVisible(false);
        }

        void Enter(State state, float until)
        {
            Current = state;
            stateUntil = until;
            // Hidden = under the water: no model, no collider, so holy water or bullets can't find her.
            bool visible = state != State.Hidden;
            SetVisible(visible);
            foreach (Collider c in colliders) c.enabled = visible && state != State.Dissolved;
            SetSpeed(0f);
            StateChanged?.Invoke(state);
        }

        void SetVisible(bool visible)
        {
            if (visuals != null) visuals.SetActive(visible);
            if (audioSource != null) audioSource.mute = !visible;
        }

        // Moves toward a point on the XZ plane at her floating height. True while still on the way.
        bool Glide(Vector3 target, float speed)
        {
            target.y = baseY;
            Vector3 to = target - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude < 0.04f) return false;
            Vector3 step = Vector3.ClampMagnitude(to, speed * Time.deltaTime);
            transform.position += step;
            transform.rotation = Quaternion.LookRotation(to);
            SetSpeed(speed);
            return true;
        }

        void SetSpeed(float speed)
        {
            if (hasSpeedParam) animator.SetFloat(SpeedParam, speed);
        }

        void MoveTo(Vector3 point)
        {
            point.y = baseY;
            transform.position = point;
            pointIndex = ClosestPoint(point);
        }

        // The canal point nearest the Padre that is still at least reappearMinDistance from him.
        Transform ReappearPoint()
        {
            Transform best = null;
            float bestDistance = float.MaxValue;
            foreach (Transform point in canalPoints)
            {
                float d = FlatDistance(point.position, player.position);
                if (d < tuning.reappearMinDistance || d >= bestDistance) continue;
                best = point;
                bestDistance = d;
            }
            return best;
        }

        int ClosestPoint(Vector3 position)
        {
            int best = 0;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < canalPoints.Length; i++)
            {
                float d = FlatDistance(canalPoints[i].position, position);
                if (d < bestDistance) { best = i; bestDistance = d; }
            }
            return best;
        }

        bool PlayerHuntable() => !playerHealth.IsDead && !SafeRoom.PlayerInside;
        bool RitualUnderway() => ritual != null && ritual.Current == RitualController.Stage.Praying;
        float DistanceToPlayer() => FlatDistance(transform.position, player.position);

        static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
