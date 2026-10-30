using Animas.Core;
using UnityEngine;

namespace Animas.Levels
{
    // An area that hurts whoever stands in it (fire, cursed water...). Also handy for testing health and death.
    [RequireComponent(typeof(Collider))]
    public class DamageZone : MonoBehaviour
    {
        [SerializeField, Min(0f)] float damagePerHit = 10f;
        [SerializeField, Min(0.1f)] float secondsBetweenHits = 1f;

        Health victim;
        float nextHitTime;

        void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            victim = other.GetComponentInParent<Health>();
            nextHitTime = Time.time;  // hurts right away, then every few seconds
        }

        void OnTriggerExit(Collider other)
        {
            if (other.GetComponentInParent<Health>() == victim) victim = null;
        }

        void Update()
        {
            if (victim == null || Time.time < nextHitTime) return;
            victim.TakeDamage(damagePerHit);
            nextHitTime = Time.time + secondsBetweenHits;
        }
    }
}

