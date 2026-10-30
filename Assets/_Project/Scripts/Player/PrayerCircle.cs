using Animas.Core;
using UnityEngine;

namespace Animas.Player
{
    // The circle left on the ground after praying: for its duration, spirits can't enter it.
    // Seen from above it's a ring of light; it fades out in the last second.
    public class PrayerCircle : MonoBehaviour, ISpiritWard
    {
        [SerializeField] LineRenderer ring;
        [SerializeField] Light glow;
        [SerializeField, Min(8)] int segments = 48;

        float radius;
        float endsAt;
        float baseIntensity;
        Color baseColor;

        public void Begin(Vector3 center, float newRadius, float duration)
        {
            transform.position = center;
            radius = newRadius;
            endsAt = Time.time + duration;
            DrawRing();
            if (glow != null) { baseIntensity = glow.intensity; glow.range = radius * 1.5f; }
            baseColor = ring.startColor;
            SpiritWards.Add(this);
        }

        void OnDisable() => SpiritWards.Remove(this);

        void Update()
        {
            float left = endsAt - Time.time;
            if (left <= 0f)
            {
                Destroy(gameObject);
                return;
            }
            // Fade in the last second, so the player sees it's about to end.
            float alpha = Mathf.Clamp01(left);
            Color c = baseColor; c.a *= alpha;
            ring.startColor = ring.endColor = c;
            if (glow != null) glow.intensity = baseIntensity * alpha;
        }

        // A spirit is "in" if it's within the radius on the ground plane (height doesn't matter).
        public bool Contains(Vector3 point)
        {
            Vector3 d = point - transform.position; d.y = 0f;
            return d.sqrMagnitude < radius * radius;
        }

        public Vector3 PushOut(Vector3 point)
        {
            Vector3 d = point - transform.position; d.y = 0f;
            // A spirit exactly at the center has no direction; send it any way.
            Vector3 dir = d.sqrMagnitude > 0.0001f ? d.normalized : Vector3.forward;
            Vector3 outside = transform.position + dir * (radius + 0.3f);
            outside.y = point.y;
            return outside;
        }

        void DrawRing()
        {
            ring.useWorldSpace = false;
            ring.loop = true;
            ring.positionCount = segments;
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments;
                ring.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, 0.05f, Mathf.Sin(a) * radius));
            }
        }
    }
}
