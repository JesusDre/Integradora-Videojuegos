using Animas.Items;
using UnityEngine;

namespace Animas.Player
{
    // The canals of Xochimilco are only a water surface: under it there is nothing (the trajinera is the
    // way across). Stepping off a dock or between the dock and the boat made the Padre fall forever (#146).
    // He remembers the last solid ground he stood on; if he drops well below it, he climbs back out there.
    [RequireComponent(typeof(CharacterController))]
    public class FallRecovery : MonoBehaviour
    {
        [Tooltip("Falling this many meters below the last solid ground brings him back.")]
        [SerializeField, Min(1f)] float fallLimit = 4f;
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip splashClip;

        CharacterController body;
        Inventory messages;
        Vector3 lastSafe;
        float nextSample;

        void Awake()
        {
            body = GetComponent<CharacterController>();
            messages = GetComponent<Inventory>();
            lastSafe = transform.position;
        }

        void Update()
        {
            // On the trajinera (or any scripted moment) the controller is off: nothing to watch.
            if (!body.enabled) return;

            // A few times a second is enough to remember where the ground was.
            if (body.isGrounded && Time.time >= nextSample)
            {
                nextSample = Time.time + 0.25f;
                lastSafe = transform.position;
            }

            if (transform.position.y < lastSafe.y - fallLimit) Recover();
        }

        void Recover()
        {
            // The CharacterController would undo a position set from outside, so it is off for the move.
            body.enabled = false;
            transform.position = lastSafe + Vector3.up * 0.1f;
            body.enabled = true;
            if (audioSource != null && splashClip != null) audioSource.PlayOneShot(splashClip);
            if (messages != null) messages.Notify("Caíste al agua. Usa la trajinera para cruzar.");
        }
    }
}
