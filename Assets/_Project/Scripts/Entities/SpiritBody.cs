using System;
using Animas.Core;
using UnityEngine;
using UnityEngine.AI;

namespace Animas.Entities
{
    // Goes on every spirit (the three entities and test spirits). Each frame it checks the active
    // wards (prayer circle, salt line); if the spirit stepped into one, it's pushed back out.
    // The entity AIs (#35, #39, #45) listen to Repelled to react (scream, change route...).
    public class SpiritBody : MonoBehaviour
    {
        [SerializeField] NavMeshAgent agent;

        public event Action Repelled;

        void Awake()
        {
            if (agent == null) agent = GetComponent<NavMeshAgent>();
        }

        void LateUpdate()
        {
            if (!SpiritWards.TryRepel(transform.position, out Vector3 safePoint)) return;
            // Warp keeps the NavMeshAgent in sync; moving the transform alone would fight the agent.
            if (agent != null && agent.isOnNavMesh) agent.Warp(safePoint);
            else transform.position = safePoint;
            Repelled?.Invoke();
        }
    }
}
