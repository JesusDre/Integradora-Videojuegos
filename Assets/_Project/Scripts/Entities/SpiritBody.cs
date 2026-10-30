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
        // Agua bendita stops a spirit's attack for a moment (GDD §18).
        public event Action<float> Stunned;
        // Veladora light and copal smoke make it visible (the Tzitzimime can only be hurt then).
        public event Action Revealed;

        public bool IsStunned => Time.time < stunnedUntil;
        public bool IsRevealed => Time.time < revealedUntil;

        float stunnedUntil;
        float revealedUntil;

        public void Stun(float seconds)
        {
            stunnedUntil = Mathf.Max(stunnedUntil, Time.time + seconds);
            if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
            Stunned?.Invoke(seconds);
        }

        public void Reveal(float seconds)
        {
            bool wasHidden = !IsRevealed;
            revealedUntil = Mathf.Max(revealedUntil, Time.time + seconds);
            if (wasHidden) Revealed?.Invoke();
        }

        void Awake()
        {
            if (agent == null) agent = GetComponent<NavMeshAgent>();
        }

        void LateUpdate()
        {
            // The stun wears off by itself.
            if (agent != null && agent.isOnNavMesh && agent.isStopped && !IsStunned) agent.isStopped = false;

            if (!SpiritWards.TryRepel(transform.position, out Vector3 safePoint)) return;
            // Warp keeps the NavMeshAgent in sync; moving the transform alone would fight the agent.
            if (agent != null && agent.isOnNavMesh) agent.Warp(safePoint);
            else transform.position = safePoint;
            Repelled?.Invoke();
        }
    }
}
