using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Animas.Player
{
    // E turns the flashlight on and off. It has no battery in the MVP; the GDD doesn't ask for one.
    [RequireComponent(typeof(Light))]
    public class Flashlight : MonoBehaviour
    {
        [SerializeField] bool startOn = true;
        [Tooltip("Optional until the sound issues (#8) bring the click clip.")]
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip clickClip;

        public bool IsOn => flashlightLight.enabled;
        public event Action<bool> Toggled;

        Light flashlightLight;
        InputAction toggleAction;

        void Awake()
        {
            flashlightLight = GetComponent<Light>();
            toggleAction = InputSystem.actions.FindAction("Player/Flashlight", true);
            flashlightLight.enabled = startOn;
        }

        void Update()
        {
            if (toggleAction.WasPressedThisFrame()) SetOn(!IsOn);
        }

        // Is this collider inside the beam? Used by the Tzitzimime (GDD §13: light hurts her while revealed).
        public bool Illuminates(Collider target)
        {
            if (!IsOn || target == null) return false;
            Vector3 to = target.bounds.center - transform.position;
            float distance = to.magnitude;
            if (distance > flashlightLight.range) return false;
            if (Vector3.Angle(transform.forward, to) > flashlightLight.spotAngle * 0.5f) return false;
            // The first solid thing along the beam must be the target itself, not a wall. Triggers don't count.
            if (!Physics.Raycast(transform.position, to / distance, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Ignore))
                return true;
            return hit.collider == target;
        }

        public void SetOn(bool on)
        {
            flashlightLight.enabled = on;
            if (audioSource != null && clickClip != null) audioSource.PlayOneShot(clickClip);
            Toggled?.Invoke(on);
        }
    }
}
