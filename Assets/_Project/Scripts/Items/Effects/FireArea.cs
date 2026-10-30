using System.Collections.Generic;
using Animas.Core;
using UnityEngine;

namespace Animas.Items
{
    // Molotov fire (GDD §12): 15 damage per second to anyone inside 3 m, for 5 s. Meant for groups of
    // cultists; it doesn't burn the Padre (the player would lose control of the tool).
    public class FireArea : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] float radius = 3f;
        [SerializeField, Min(0f)] float damagePerSecond = 15f;
        [SerializeField, Min(0.5f)] float duration = 5f;
        [Tooltip("Damage is applied in small steps instead of every frame.")]
        [SerializeField, Min(0.05f)] float tickSeconds = 0.25f;
        [SerializeField] Light glow;

        float endsAt;
        float nextTick;
        float baseIntensity;
        readonly Collider[] inside = new Collider[32];
        readonly HashSet<Health> burned = new HashSet<Health>();

        void Start()
        {
            endsAt = Time.time + duration;
            if (glow != null) baseIntensity = glow.intensity;
        }

        void Update()
        {
            if (Time.time >= endsAt)
            {
                Destroy(gameObject);
                return;
            }
            // A flickering light so the fire reads in the dark.
            if (glow != null) glow.intensity = baseIntensity * (0.75f + 0.25f * Mathf.PerlinNoise(Time.time * 8f, 0f));
            if (Time.time < nextTick) return;
            nextTick = Time.time + tickSeconds;

            burned.Clear();
            int count = Physics.OverlapSphereNonAlloc(transform.position, radius, inside, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                Health health = inside[i].GetComponentInParent<Health>();
                if (health == null || health.CompareTag("Player") || !burned.Add(health)) continue;
                health.TakeDamage(damagePerSecond * tickSeconds);
            }
        }
    }
}
