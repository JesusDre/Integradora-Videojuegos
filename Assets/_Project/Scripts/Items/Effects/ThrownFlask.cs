using Animas.Core;
using Animas.Levels;
using UnityEngine;

namespace Animas.Items
{
    // A thrown bottle: when it hits anything it breaks and leaves its effect (splash or fire).
    [RequireComponent(typeof(Rigidbody))]
    public class ThrownFlask : MonoBehaviour
    {
        [SerializeField] GameObject impactPrefab;
        [Tooltip("The glass breaking.")]
        [SerializeField] AudioClip breakClip;
        [Tooltip("If it hits nothing (thrown into the void), it breaks anyway after this time.")]
        [SerializeField, Min(0.5f)] float maxLifetime = 5f;

        bool broken;
        bool holyWater;

        void Start()
        {
            Invoke(nameof(BreakHere), maxLifetime);
            holyWater = impactPrefab != null && impactPrefab.GetComponent<HolyWaterSplash>() != null;
        }

        // Holy water drops into an open canaleta it flies over (HolyWaterChannel.Catches, #158).
        void Update()
        {
            if (!holyWater || broken) return;
            foreach (HolyWaterChannel channel in HolyWaterChannel.All)
            {
                if (!channel.Catches(transform.position)) continue;
                Vector3 point = transform.position;
                if (Physics.Raycast(point, Vector3.down, out RaycastHit ground, 5f, ~0, QueryTriggerInteraction.Ignore))
                    point = ground.point;
                Break(point);
                return;
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (broken) return;
            // The effect sits on the ground: from the contact point, look down for the floor.
            Vector3 point = collision.GetContact(0).point;
            if (Physics.Raycast(point + Vector3.up * 0.5f, Vector3.down, out RaycastHit ground, 3f, ~0, QueryTriggerInteraction.Ignore))
                point = ground.point;
            Break(point);
        }

        void BreakHere() => Break(transform.position);

        void Break(Vector3 at)
        {
            if (broken) return;
            broken = true;
            if (impactPrefab != null) Instantiate(impactPrefab, at, Quaternion.identity);
            // PlayClipAtPoint: this object is destroyed right away and would cut its own AudioSource.
            if (breakClip != null) AudioSource.PlayClipAtPoint(breakClip, at, GameSettings.GetVolume(VolumeCategory.Effects));
            Destroy(gameObject);
        }
    }
}
