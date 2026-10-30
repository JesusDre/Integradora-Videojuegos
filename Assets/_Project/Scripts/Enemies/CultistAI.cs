using System;
using Animas.Core;
using Animas.Player;
using UnityEngine;
using UnityEngine.AI;

namespace Animas.Enemies
{
    // Cultist brain as a state machine (GDD §21):
    // Patrol → (sees or hears the player) → Chase → (close enough) → Attack
    // → (loses the player) → Search for 10 s → Return to the patrol route → Patrol.
    // The NavMeshAgent does the pathfinding; this script only decides where to go and when to attack.
    [RequireComponent(typeof(NavMeshAgent), typeof(Health))]
    public class CultistAI : MonoBehaviour
    {
        public enum State { Patrol, Chase, Attack, Search, Return }

        [SerializeField] CultistTuning tuning;
        [Tooltip("Points to walk between. Empty: the cultist stands guard where it was placed.")]
        [SerializeField] Transform[] patrolPoints;
        [SerializeField] Animator animator;
        [Tooltip("Where the cultist looks from (head height).")]
        [SerializeField] Transform eyes;
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip alertClip;
        [SerializeField] LayerMask sightBlockers = ~0;

        public State Current { get; private set; } = State.Patrol;
        public event Action<State> StateChanged;

        NavMeshAgent agent;
        Health health;
        Transform player;
        Health playerHealth;
        PlayerController playerMovement;

        Vector3 home;
        Vector3 lastKnownPosition;
        int patrolIndex;
        float waitUntil = -1f;
        float searchUntil;
        float nextAttackTime;
        float hitTime = -1f;

        static readonly int SpeedParam = Animator.StringToHash("Speed");
        static readonly int AttackParam = Animator.StringToHash("Attack");

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            // Found once here, never in Update (ARQUITECTURA: no Find calls every frame).
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                playerHealth = p.GetComponent<Health>();
                playerMovement = p.GetComponent<PlayerController>();
            }
            home = transform.position;
        }

        void OnEnable() => health.Died += OnDied;
        void OnDisable() => health.Died -= OnDied;

        void Start()
        {
            Enter(State.Patrol);
        }

        void Update()
        {
            if (health.IsDead || player == null) return;

            bool playerAlive = playerHealth == null || !playerHealth.IsDead;
            bool sees = playerAlive && CanSeePlayer();
            bool hears = playerAlive && CanHearPlayer();
            if (sees || hears)
            {
                lastKnownPosition = player.position;
                if (Current != State.Chase && Current != State.Attack) Enter(State.Chase);
            }

            switch (Current)
            {
                case State.Patrol: UpdatePatrol(); break;
                case State.Chase: UpdateChase(sees, hears); break;
                case State.Attack: UpdateAttack(sees); break;
                case State.Search: UpdateSearch(); break;
                case State.Return: UpdateReturn(); break;
            }

            // The Animator blends idle → walk → run from the real speed, so feet match the movement.
            animator.SetFloat(SpeedParam, agent.velocity.magnitude);
        }

        // For cultists spawned at runtime (waves, test arena): give them a route before their first frame.
        public void SetPatrol(Transform[] points)
        {
            patrolPoints = points ?? Array.Empty<Transform>();
            patrolIndex = 0;
        }

        // Called by loud things (the shotgun, #24): the cultist goes to look.
        public void HearNoise(Vector3 position)
        {
            if (health.IsDead) return;
            lastKnownPosition = position;
            if (Current != State.Chase && Current != State.Attack) Enter(State.Chase);
        }

        void Enter(State next)
        {
            Current = next;
            agent.isStopped = false;
            switch (next)
            {
                case State.Patrol:
                    agent.speed = tuning.patrolSpeed;
                    waitUntil = -1f;
                    GoToPatrolPoint();
                    break;
                case State.Chase:
                    agent.speed = tuning.chaseSpeed;
                    if (audioSource != null && alertClip != null) audioSource.PlayOneShot(alertClip);
                    break;
                case State.Attack:
                    agent.isStopped = true;
                    break;
                case State.Search:
                    agent.speed = tuning.patrolSpeed;
                    searchUntil = Time.time + tuning.searchDuration;
                    agent.SetDestination(lastKnownPosition);
                    break;
                case State.Return:
                    agent.speed = tuning.patrolSpeed;
                    agent.SetDestination(patrolPoints.Length > 0 ? patrolPoints[patrolIndex].position : home);
                    break;
            }
            StateChanged?.Invoke(next);
        }

        void UpdatePatrol()
        {
            if (!Arrived()) return;
            if (patrolPoints.Length == 0) return;  // a guard just stands there

            // Wait a moment at each point before walking to the next one.
            if (waitUntil < 0f) waitUntil = Time.time + tuning.patrolWait;
            else if (Time.time >= waitUntil)
            {
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                waitUntil = -1f;
                GoToPatrolPoint();
            }
        }

        void UpdateChase(bool sees, bool hears)
        {
            agent.SetDestination(lastKnownPosition);
            float distance = Vector3.Distance(transform.position, player.position);
            if (sees && distance <= tuning.attackRange) Enter(State.Attack);
            else if (!sees && !hears && Arrived()) Enter(State.Search);
        }

        void UpdateAttack(bool sees)
        {
            FacePlayer();
            float distance = Vector3.Distance(transform.position, player.position);

            if (hitTime < 0f && Time.time >= nextAttackTime)
            {
                animator.SetTrigger(AttackParam);
                nextAttackTime = Time.time + tuning.attackCooldown;
                hitTime = Time.time + tuning.attackHitDelay;
            }

            // Damage lands when the machete comes down in the animation, not when the swing starts,
            // and only if the player is still in reach: stepping back in time dodges the blow.
            if (hitTime >= 0f && Time.time >= hitTime)
            {
                hitTime = -1f;
                if (distance <= tuning.attackRange + 0.3f && playerHealth != null) playerHealth.TakeDamage(tuning.damage);
            }

            if (hitTime < 0f && (distance > tuning.attackRange + 0.4f || !sees)) Enter(State.Chase);
            if (playerHealth != null && playerHealth.IsDead) Enter(State.Return);
        }

        void UpdateSearch()
        {
            if (Time.time >= searchUntil)
            {
                Enter(State.Return);
                return;
            }
            // Wander to random reachable spots around where the player was last seen.
            if (Arrived())
            {
                Vector3 random = lastKnownPosition + UnityEngine.Random.insideUnitSphere * tuning.searchRadius;
                if (NavMesh.SamplePosition(random, out NavMeshHit hit, tuning.searchRadius, NavMesh.AllAreas))
                    agent.SetDestination(hit.position);
            }
        }

        void UpdateReturn()
        {
            if (Arrived()) Enter(State.Patrol);
        }

        bool CanSeePlayer()
        {
            Vector3 eye = eyes != null ? eyes.position : transform.position + Vector3.up * 1.6f;
            Vector3 target = player.position + Vector3.up * 1.4f;
            Vector3 toPlayer = target - eye;
            if (toPlayer.magnitude > tuning.sightRange) return false;

            Vector3 flat = new Vector3(toPlayer.x, 0f, toPlayer.z);
            if (Vector3.Angle(transform.forward, flat) > tuning.sightAngle * 0.5f) return false;

            // Line of sight: the first thing the ray hits must be the player (walls and closed doors block it).
            if (Physics.Raycast(eye, toPlayer.normalized, out RaycastHit hit, toPlayer.magnitude + 0.5f,
                    sightBlockers, QueryTriggerInteraction.Ignore))
                return hit.transform == player || hit.transform.IsChildOf(player);
            return false;
        }

        bool CanHearPlayer()
        {
            return playerMovement != null && playerMovement.IsRunning
                && Vector3.Distance(transform.position, player.position) <= tuning.hearRunningRange;
        }

        void FacePlayer()
        {
            Vector3 flat = player.position - transform.position;
            flat.y = 0f;
            if (flat.sqrMagnitude < 0.001f) return;
            Quaternion target = Quaternion.LookRotation(flat);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, tuning.turnSpeed * Time.deltaTime);
        }

        void GoToPatrolPoint()
        {
            if (patrolPoints.Length > 0) agent.SetDestination(patrolPoints[patrolIndex].position);
        }

        bool Arrived()
        {
            return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f;
        }

        void OnDied()
        {
            agent.isStopped = true;
            agent.enabled = false;
            animator.SetFloat(SpeedParam, 0f);
            enabled = false;
        }

        // In the Scene view: the sight cone and hearing range of the selected cultist.
        void OnDrawGizmosSelected()
        {
            if (tuning == null) return;
            Gizmos.color = new Color(1f, 0.2f, 0.1f, 0.6f);
            Vector3 left = Quaternion.Euler(0, -tuning.sightAngle * 0.5f, 0) * transform.forward;
            Vector3 right = Quaternion.Euler(0, tuning.sightAngle * 0.5f, 0) * transform.forward;
            Vector3 origin = transform.position + Vector3.up * 1.6f;
            Gizmos.DrawLine(origin, origin + left * tuning.sightRange);
            Gizmos.DrawLine(origin, origin + right * tuning.sightRange);
            Gizmos.color = new Color(1f, 0.8f, 0.1f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, tuning.hearRunningRange);
        }
    }
}
