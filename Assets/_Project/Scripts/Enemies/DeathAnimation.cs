using Animas.Core;
using UnityEngine;

namespace Animas.Enemies
{
    // When an enemy dies: play its death animation and turn off its colliders, so the body stops blocking
    // the way and can't be hit again. The cultist AI (#15) also stops on the same Died event. A few
    // seconds later the body sinks into the ground and is removed (#164): with hordes, dozens of bodies
    // would pile up, and they cost frames for nothing.
    [RequireComponent(typeof(Health))]
    public class DeathAnimation : MonoBehaviour
    {
        [SerializeField] Animator animator;
        [SerializeField] string deathTrigger = "Die";
        [Tooltip("Seconds the body stays on the ground before it starts sinking.")]
        [SerializeField, Min(0f)] float bodySeconds = 6f;
        [SerializeField, Min(0.1f)] float sinkSeconds = 2f;

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
            StartCoroutine(SinkAndRemove());
        }

        System.Collections.IEnumerator SinkAndRemove()
        {
            yield return new WaitForSeconds(bodySeconds);
            Vector3 start = transform.position;
            for (float t = 0f; t < 1f; t += Time.deltaTime / sinkSeconds)
            {
                transform.position = start + Vector3.down * (1.2f * t);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}

