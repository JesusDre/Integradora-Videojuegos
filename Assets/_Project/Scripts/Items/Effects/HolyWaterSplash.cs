using System.Collections.Generic;
using Animas.Core;
using Animas.Entities;
using UnityEngine;

namespace Animas.Items
{
    // Agua bendita breaking (GDD §18): area damage to spirits and stops their attack for 3 s.
    // Humans are not hurt. A pale flash and a ring that grows show where it landed.
    public class HolyWaterSplash : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] float radius = 3f;
        [SerializeField, Min(0f)] float spiritDamage = 20f;
        [SerializeField, Min(0f)] float stunSeconds = 3f;
        [SerializeField] Transform visual;
        [SerializeField] Light flash;
        [SerializeField, Min(0.1f)] float visualSeconds = 0.8f;

        float started;
        float flashIntensity;

        void Start()
        {
            started = Time.time;
            if (flash != null) flashIntensity = flash.intensity;
            // A HashSet so a spirit with several colliders is hit once.
            var hit = new HashSet<Health>();
            foreach (Collider c in Physics.OverlapSphere(transform.position, radius, ~0, QueryTriggerInteraction.Ignore))
            {
                Health health = c.GetComponentInParent<Health>();
                if (health == null || health.Kind != HealthKind.Spirit || !hit.Add(health)) continue;
                health.TakeDamage(spiritDamage);
                SpiritBody spirit = health.GetComponent<SpiritBody>();
                if (spirit != null) spirit.Stun(stunSeconds);
            }
        }

        void Update()
        {
            float t = (Time.time - started) / visualSeconds;
            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }
            if (visual != null) visual.localScale = new Vector3(radius * 2f * t, 0.05f, radius * 2f * t);
            if (flash != null) flash.intensity = flashIntensity * (1f - t);
        }
    }
}
