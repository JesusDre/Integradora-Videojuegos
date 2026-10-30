using System;
using Animas.Core;
using Animas.Items;
using Animas.Levels;
using Animas.Player;
using Animas.Save;
using UnityEngine;
using UnityEngine.AI;

namespace Animas.Entities
{
    // El Charro Negro (GDD §13 and §21): Chase (trots after the Padre) → (close) → Charge (a fast
    // straight run past him) → Recover → Chase. He can't enter the chapel (a SpiritBarrier): he waits
    // at its door. When the 3 coins are on the chapel pedestal, the final phase opens: the canaletas
    // accept holy water, and charging across a full one weakens him for a few seconds. Three charged
    // silver-cross hits while weakened dissolve him, and the gate opens for the escape on foot.
    // Normal weapons don't hurt him: there's no life bar to empty, only the 3 hits.
    [RequireComponent(typeof(NavMeshAgent), typeof(SpiritBody), typeof(Health))]
    public class CharroNegroEntity : MonoBehaviour
    {
        public enum State { Idle, Chase, Charge, Recover, Weakened, Dissolved }

        [SerializeField] CharroTuning tuning;
        [Tooltip("The chapel pedestal for the 3 coins (an Anchor). Completing it opens the final phase.")]
        [SerializeField] Anchor pedestal;
        [Tooltip("Optional trigger box: the ground he rides (GDD plan: between the tienda, the panteón and the chapel). " +
                 "Outside it he lets the Padre go and returns. Empty = he hunts everywhere (test arena).")]
        [SerializeField] BoxCollider territory;
        [Tooltip("The hacienda gate: opens when he's defeated.")]
        [SerializeField] Door gate;
        [SerializeField] Animator animator;
        [Tooltip("Light that flares while he's weakened, so the player knows when to strike.")]
        [SerializeField] Light weakGlow;
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip neighClip;
        [SerializeField] AudioClip hurtClip;
        [SerializeField, Min(0.1f)] float dissolveSeconds = 2.5f;

        public State Current { get; private set; } = State.Idle;
        public int HitsTaken { get; private set; }
        public bool FinalPhase { get; private set; }

        public event Action<State> StateChanged;
        // The level listens (the gate, #48's victory flow if needed).
        public event Action Defeated;

        NavMeshAgent agent;
        SpiritBody body;
        Health health;
        Transform player;
        Health playerHealth;
        PlayerCombat playerCombat;
        PlayerRespawn playerRespawn;
        Inventory messages;

        Vector3 home;
        Vector3 chargeTarget;
        Vector3 chargeStart;
        Vector3 lastPosition;
        float stateUntil;
        bool chargeHit;
        bool hasSpeedParam;

        static readonly int SpeedParam = Animator.StringToHash("Speed");

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            body = GetComponent<SpiritBody>();
            health = GetComponent<Health>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            hasSpeedParam = animator != null && animator.runtimeAnimatorController != null
                && Array.Exists(animator.parameters, param => param.nameHash == SpeedParam);
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                playerHealth = p.GetComponent<Health>();
                playerCombat = p.GetComponent<PlayerCombat>();
                playerRespawn = p.GetComponent<PlayerRespawn>();
                messages = p.GetComponent<Inventory>();
            }
            home = transform.position;
            if (weakGlow != null) weakGlow.enabled = false;
        }

        void OnEnable()
        {
            health.Damaged += OnHurt;
            if (playerCombat != null) playerCombat.ChargedHitLanded += OnChargedHit;
            if (playerRespawn != null) playerRespawn.Respawned += OnPlayerRespawned;
            if (pedestal != null) pedestal.OfferingsComplete += BeginFinalPhase;
        }

        void OnDisable()
        {
            health.Damaged -= OnHurt;
            if (playerCombat != null) playerCombat.ChargedHitLanded -= OnChargedHit;
            if (playerRespawn != null) playerRespawn.Respawned -= OnPlayerRespawned;
            if (pedestal != null) pedestal.OfferingsComplete -= BeginFinalPhase;
        }

        void Start() => Enter(State.Chase, 0f);

        void Update()
        {
            if (player == null || Current == State.Dissolved) return;

            switch (Current)
            {
                case State.Idle: UpdateIdle(); break;
                case State.Chase: UpdateChase(); break;
                case State.Charge: UpdateCharge(); break;
                case State.Recover: if (Time.time >= stateUntil) Enter(State.Chase, 0f); break;
                case State.Weakened: UpdateWeakened(); break;
            }
            if (hasSpeedParam) animator.SetFloat(SpeedParam, agent.velocity.magnitude);
        }

        // The Padre is dead, in the safe room or off his ground: back home until he returns.
        void UpdateIdle()
        {
            if (PlayerHuntable()) Enter(State.Chase, 0f);
        }

        void UpdateChase()
        {
            if (!PlayerHuntable())
            {
                Enter(State.Idle, 0f);
                agent.SetDestination(home);
                return;
            }
            // If the Padre is somewhere he can't enter (the chapel, a prayer circle), he waits at its edge.
            Vector3 target = SpiritWards.TryRepel(player.position, out Vector3 edge) ? edge : player.position;
            agent.SetDestination(target);
            if (target == player.position && FlatDistance(transform.position, player.position) <= tuning.chargeRange)
                StartCharge();
        }

        void StartCharge()
        {
            // Aim past the Padre: the charge doesn't steer, so stepping aside at the last second dodges it.
            Vector3 dir = player.position - transform.position;
            dir.y = 0f;
            chargeTarget = player.position + dir.normalized * tuning.chargeOvershoot;
            if (NavMesh.SamplePosition(chargeTarget, out NavMeshHit hit, 4f, NavMesh.AllAreas)) chargeTarget = hit.position;
            chargeHit = false;
            chargeStart = transform.position;
            lastPosition = transform.position;
            Enter(State.Charge, Time.time + tuning.maxChargeSeconds);
            agent.SetDestination(chargeTarget);
            if (audioSource != null && neighClip != null) audioSource.PlayOneShot(neighClip);
        }

        void UpdateCharge()
        {
            if (!chargeHit && !playerHealth.IsDead && FlatDistance(transform.position, player.position) <= tuning.hitRange)
            {
                chargeHit = true;
                playerHealth.TakeDamage(tuning.chargeDamage);
            }
            // Charging across a full canaleta weakens him (only in the final phase, when they can be filled).
            // A charge that starts standing in the water doesn't count: each hit needs a new crossing.
            foreach (HolyWaterChannel channel in HolyWaterChannel.All)
            {
                if (!channel.IsFull || channel.Contains(chargeStart)) continue;
                if (!channel.Crossed(lastPosition, transform.position)) continue;
                Weaken();
                return;
            }
            lastPosition = transform.position;
            bool arrived = !agent.pathPending && agent.remainingDistance <= 0.5f;
            if (arrived || Time.time >= stateUntil) Enter(State.Recover, Time.time + tuning.recoverSeconds);
        }

        void Weaken()
        {
            Enter(State.Weakened, Time.time + tuning.weakenedSeconds);
            if (weakGlow != null) weakGlow.enabled = true;
            if (audioSource != null && hurtClip != null) audioSource.PlayOneShot(hurtClip);
            if (messages != null) messages.Notify("¡El Charro está débil! Golpéalo con la cruz cargada");
        }

        void UpdateWeakened()
        {
            if (Time.time < stateUntil) return;
            if (weakGlow != null) weakGlow.enabled = false;
            Enter(State.Chase, 0f);
        }

        // Only a charged hit counts, and only while weakened; the cross is the only weapon that charges.
        void OnChargedHit(Health target)
        {
            if (target != health || Current != State.Weakened) return;
            HitsTaken++;
            if (audioSource != null && hurtClip != null) audioSource.PlayOneShot(hurtClip);
            if (weakGlow != null) weakGlow.enabled = false;
            if (HitsTaken >= tuning.hitsToDefeat)
            {
                Dissolve();
                return;
            }
            if (messages != null) messages.Notify($"Golpe {HitsTaken} de {tuning.hitsToDefeat}");
            // He shakes it off and charges again: each hit needs a new trip across the water.
            Enter(State.Recover, Time.time + tuning.recoverSeconds);
        }

        // Ordinary damage (machete, shotgun) doesn't wear him down.
        void OnHurt(float amount) => health.Heal(amount);

        void BeginFinalPhase()
        {
            FinalPhase = true;
            HolyWaterChannel.Unlocked = true;
            if (messages != null) messages.Notify("Llena las canaletas con agua bendita y hazlo cruzar");
        }

        void OnPlayerRespawned()
        {
            if (Current == State.Dissolved) return;
            // The coins went back to the world (the pedestal empties itself): back to phase 1.
            HitsTaken = 0;
            FinalPhase = false;
            HolyWaterChannel.Unlocked = false;
            foreach (HolyWaterChannel channel in HolyWaterChannel.All) channel.Drain();
            if (weakGlow != null) weakGlow.enabled = false;
            agent.Warp(home);
            Enter(State.Chase, 0f);
        }

        void Dissolve()
        {
            Enter(State.Dissolved, 0f);
            foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
            if (gate != null)
            {
                gate.Unlock();
                gate.SetOpen(true);
            }
            if (messages != null) messages.Notify("El Charro Negro se disuelve. ¡Corre al portón!");
            Defeated?.Invoke();
            StartCoroutine(SinkAndFade());
        }

        System.Collections.IEnumerator SinkAndFade()
        {
            agent.enabled = false;
            Vector3 start = transform.position;
            for (float t = 0f; t < 1f; t += Time.deltaTime / dissolveSeconds)
            {
                transform.position = start + Vector3.down * (3f * t);
                yield return null;
            }
            gameObject.SetActive(false);
        }

        void Enter(State state, float until)
        {
            Current = state;
            stateUntil = until;
            if (agent.enabled && agent.isOnNavMesh)
            {
                // Weakened and recovering, he stands still. (Not isStopped: SpiritBody uses that for the
                // holy-water stun and clears it by itself.)
                if (state == State.Weakened || state == State.Recover || state == State.Dissolved)
                {
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                }
                agent.speed = state == State.Charge ? tuning.chargeSpeed : tuning.chaseSpeed;
                agent.acceleration = state == State.Charge ? 40f : 12f;
            }
            StateChanged?.Invoke(state);
        }

        bool PlayerHuntable() => !playerHealth.IsDead && !SafeRoom.PlayerInside
            && (territory == null || territory.bounds.Contains(player.position + Vector3.up));

        static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
