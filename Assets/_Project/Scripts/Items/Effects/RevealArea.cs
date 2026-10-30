using Animas.Entities;
using UnityEngine;

namespace Animas.Items
{
    // Veladora and copal (GDD §18): light and smoke that make nearby spirits visible.
    // The veladora reveals what comes close while it burns; the copal reveals the Tzitzimime for
    // 15 s (its phase 2 can only be hurt while revealed). The entity AIs decide what "revealed" does.
    public class RevealArea : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] float radius = 8f;
        [SerializeField, Min(1f)] float duration = 45f;
        [Tooltip("How long a spirit stays revealed after being caught by the light or smoke.")]
        [SerializeField, Min(0.5f)] float revealSeconds = 2f;
        [SerializeField, Min(0.1f)] float checkSeconds = 0.5f;
        [SerializeField] Light glow;

        float endsAt;
        float nextCheck;
        float baseIntensity;
        readonly Collider[] inside = new Collider[32];

        void Start()
        {
            endsAt = Time.time + duration;
            if (glow != null) baseIntensity = glow.intensity;
        }

        void Update()
        {
            float left = endsAt - Time.time;
            if (left <= 0f)
            {
                Destroy(gameObject);
                return;
            }
            if (glow != null) glow.intensity = baseIntensity * Mathf.Clamp01(left / 2f);
            if (Time.time < nextCheck) return;
            nextCheck = Time.time + checkSeconds;

            int count = Physics.OverlapSphereNonAlloc(transform.position, radius, inside, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                SpiritBody spirit = inside[i].GetComponentInParent<SpiritBody>();
                if (spirit != null) spirit.Reveal(revealSeconds);
            }
        }
    }
}
