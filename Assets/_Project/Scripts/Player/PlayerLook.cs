using Animas.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Animas.Player
{
    // Mouse look: the body turns left/right (yaw) and only the camera tilts up/down (pitch),
    // so WASD always moves relative to where the player faces.
    public class PlayerLook : MonoBehaviour
    {
        [SerializeField] PlayerTuning tuning;
        [SerializeField] Transform cameraPivot;

        // The tuning default times the Options multiplier (GameSettings, issue #47).
        public float Sensitivity { get; private set; }

        InputAction lookAction;
        float pitch;
        float kick;  // extra upward tilt from a swing or a hit, fading back to 0
        bool invertY;

        // A small jolt of the camera: feedback for swings and for being hit.
        public void AddKick(float degrees) => kick += degrees;

        void Awake()
        {
            lookAction = InputSystem.actions.FindAction("Player/Look", true);
            ApplySettings();
        }

        void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            GameSettings.Changed += ApplySettings;
            ApplySettings();
        }

        void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            GameSettings.Changed -= ApplySettings;
        }

        void ApplySettings()
        {
            Sensitivity = tuning.mouseSensitivity * GameSettings.Sensitivity;
            invertY = GameSettings.InvertY;
        }

        void Update()
        {
            // Mouse delta is already "how far the mouse moved this frame", so it is NOT multiplied by
            // Time.deltaTime: doing so would make the camera slower at high frame rates.
            Vector2 delta = lookAction.ReadValue<Vector2>() * Sensitivity;

            transform.Rotate(0f, delta.x, 0f);

            // Clamping the pitch keeps the camera from flipping upside down (the rubric checks unplayable angles).
            float vertical = invertY ? -delta.y : delta.y;
            pitch = Mathf.Clamp(pitch - vertical, -tuning.maxPitch, tuning.maxPitch);
            // The kick decays over time (deltaTime), so it lasts the same on any computer.
            kick = Mathf.MoveTowards(kick, 0f, 20f * Time.deltaTime);
            cameraPivot.localRotation = Quaternion.Euler(pitch - kick, 0f, 0f);
        }
    }
}
