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

        public void SetOn(bool on)
        {
            flashlightLight.enabled = on;
            if (audioSource != null && clickClip != null) audioSource.PlayOneShot(clickClip);
            Toggled?.Invoke(on);
        }
    }
}
