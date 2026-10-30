using Animas.Core;
using UnityEngine;

namespace Animas.Enemies
{
    // When an enemy dies: play its death animation and turn off its colliders, so the body stops blocking
    // the way and can't be hit again. The cultist AI (#15) also stops on the same Died event.
    [RequireComponent(typeof(Health))]
    public class DeathAnimation : MonoBehaviour
    {
        [SerializeField] Animator animator;
        [SerializeField] string deathTrigger = "Die";

        Health health;

        void Awake()
        {
            health = GetComponent<Health>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        void OnEnable() => health.Died += OnDied;
        void OnDisable() => health.Died -= OnDied;

        void OnDied()
        {
            if (animator != null) animator.SetTrigger(deathTrigger);
            foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
        }
    }
}

