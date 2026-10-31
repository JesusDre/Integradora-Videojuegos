using Animas.Core;
using UnityEngine;

namespace Animas.Player
{
    // A burst on whatever the Padre hits (issue #44): dark red on people, pale blue on spirits.
    // PlayerCombat already announces each hit; this only shows it.
    public class HitEffects : MonoBehaviour
    {
        [SerializeField] PlayerCombat combat;
        [SerializeField] ParticleSystem humanBurst;
        [SerializeField] ParticleSystem spiritBurst;
        [Tooltip("Height of the burst above the target's feet (the chest).")]
        [SerializeField] float height = 1.2f;

        void OnEnable() => combat.HitLanded += OnHit;
        void OnDisable() => combat.HitLanded -= OnHit;

        void OnHit(Health target, float damage)
        {
            ParticleSystem prefab = target.Kind == HealthKind.Spirit ? spiritBurst : humanBurst;
            if (prefab == null) return;
            Vector3 at = target.transform.position + Vector3.up * height;
            // Facing the Padre, so the burst sprays toward the camera side like a real hit.
            Quaternion facing = Quaternion.LookRotation(transform.position - at);
            ParticleSystem burst = Instantiate(prefab, at, facing);
            Destroy(burst.gameObject, burst.main.duration + burst.main.startLifetime.constantMax);
        }
    }
}
