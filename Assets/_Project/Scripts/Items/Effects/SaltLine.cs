using Animas.Core;
using UnityEngine;

namespace Animas.Items
{
    // A line of salt on the ground (GDD §18): for 20 s spirits can't cross it. Placed across the
    // Padre's view, so it blocks what comes toward him. It fades in its last 2 s.
    public class SaltLine : MonoBehaviour, ISpiritWard
    {
        [SerializeField, Min(0.5f)] float length = 3f;
        [Tooltip("How thick the blocked strip is: wide enough that a spirit can't step over it in one frame.")]
        [SerializeField, Min(0.2f)] float thickness = 0.8f;
        [SerializeField, Min(1f)] float duration = 20f;
        [SerializeField] Renderer visual;

        float endsAt;

        void Start()
        {
            endsAt = Time.time + duration;
            SpiritWards.Add(this);
        }

        void OnDestroy() => SpiritWards.Remove(this);

        void Update()
        {
            float left = endsAt - Time.time;
            if (left <= 0f)
            {
                Destroy(gameObject);
                return;
            }
            if (visual != null && left < 2f) visual.transform.localScale = new Vector3(length, 0.02f, 0.15f * left / 2f);
        }

        // In local space the line runs along X and the Padre is on the -Z side.
        public bool Contains(Vector3 point)
        {
            Vector3 local = transform.InverseTransformPoint(point);
            return Mathf.Abs(local.x) <= length * 0.5f && Mathf.Abs(local.z) <= thickness * 0.5f;
        }

        // Back to the side the spirit came from (the far side, away from the Padre), just off the strip.
        public Vector3 PushOut(Vector3 point)
        {
            Vector3 local = transform.InverseTransformPoint(point);
            local.z = local.z >= 0f ? thickness * 0.5f + 0.3f : -(thickness * 0.5f + 0.3f);
            Vector3 world = transform.TransformPoint(local);
            world.y = point.y;
            return world;
        }
    }
}
