using Animas.Core;
using Animas.Entities;
using Animas.Levels;
using UnityEngine;
using UnityEngine.AI;

namespace Animas.Enemies
{
    // A possessed doll (#172, Xochimilco): she only moves while the Padre isn't looking at her. On screen
    // she freezes; the moment he turns away she rushes after him (NavMesh), laughing. If she reaches him
    // there's a scare and a hit, and she's back where she stood, still, as if nothing happened. She has
    // no animation on purpose: you never see her move, she just isn't where she was. Any weapon breaks her.
    [RequireComponent(typeof(NavMeshAgent), typeof(Health))]
    public class HauntedDoll : MonoBehaviour
    {
        [SerializeField] DollTuning tuning;
        [SerializeField] AudioSource voice;
        [SerializeField] AudioClip[] laughs;
        [Tooltip("Porcelain breaking: she was destroyed.")]
        [SerializeField] AudioClip breakClip;
        [Tooltip("Her face on the scare screen when she catches the Padre.")]
        [SerializeField] Sprite scareFace;
        [SerializeField] AudioClip scareClip;
        [Tooltip("The point the Padre has to see (her head) for her to freeze.")]
        [SerializeField] Transform head;

        public bool Seen { get; private set; }
        public bool Moving { get; private set; }

        NavMeshAgent agent;
        Health health;
        Transform player;
        Health playerHealth;
        Vector3 home;
        Quaternion homeRotation;
        float restUntil;
        float nextLaugh;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                playerHealth = p.GetComponent<Health>();
            }
            home = transform.position;
            homeRotation = transform.rotation;
            agent.speed = tuning.moveSpeed;
            // Turning is done here, instantly, so she is always already facing him when he looks back.
            agent.updateRotation = false;
        }

        void OnEnable() => health.Died += Break;
        void OnDisable() => health.Died -= Break;

        void Update()
        {
            if (player == null || health.IsDead) return;
            float distance = Vector3.Distance(transform.position, player.position);
            Seen = CameraSight.CanSee(Camera.main, head != null ? head.position : transform.position + Vector3.up * 0.8f, transform);
            bool awake = distance <= tuning.wakeDistance && Time.time >= restUntil
                && !playerHealth.IsDead && !SafeRoom.PlayerInside;
            Moving = awake && !Seen;
            if (!Moving)
            {
                // Frozen the instant she's looked at: no sliding to a stop.
                if (agent.isOnNavMesh) agent.isStopped = true;
                agent.velocity = Vector3.zero;
                return;
            }

            agent.isStopped = false;
            agent.SetDestination(player.position);
            Vector3 look = player.position - transform.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(look);

            if (distance <= tuning.laughDistance && Time.time >= nextLaugh) Laugh();
            if (distance <= tuning.catchDistance) Catch();
        }

        void Laugh()
        {
            nextLaugh = Time.time + Random.Range(tuning.laughEvery.x, tuning.laughEvery.y);
            if (voice != null && laughs != null && laughs.Length > 0) voice.PlayOneShot(laughs[Random.Range(0, laughs.Length)]);
        }

        // Caught: the scare, the hit, and she's back at her place, still, for a while.
        void Catch()
        {
            EntityScare.Trigger(scareFace, scareClip);
            playerHealth.TakeDamage(tuning.catchDamage);
            agent.Warp(home);
            transform.rotation = homeRotation;
            restUntil = Time.time + tuning.restSeconds;
        }

        void Break()
        {
            if (breakClip != null) AudioSource.PlayClipAtPoint(breakClip, transform.position, GameSettings.GetVolume(VolumeCategory.Effects));
            Destroy(gameObject);
        }
    }
}
