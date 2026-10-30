using UnityEngine;
using UnityEngine.AI;

namespace Animas.Testing
{
    // A test spirit for the arena: it just walks toward the Padre, so the prayer circle and the
    // salt line can be checked before the real entities have their AI.
    [RequireComponent(typeof(NavMeshAgent))]
    public class SpiritTestChaser : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] float repathSeconds = 0.25f;

        NavMeshAgent agent;
        Transform player;
        float nextRepath;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }

        void Update()
        {
            // Asking for a new path every frame is wasteful; a few times per second is enough.
            if (player == null || Time.time < nextRepath || !agent.isOnNavMesh) return;
            nextRepath = Time.time + repathSeconds;
            agent.SetDestination(player.position);
        }
    }
}
