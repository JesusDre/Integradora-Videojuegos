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

        // Settable from the Options menu (issue #47); starts at the tuning default.
        public float Sensitivity { get; set; }

        InputAction lookAction;
        float pitch;

        void Awake()
        {
            lookAction = InputSystem.actions.FindAction("Player/Look", true);
            Sensitivity = tuning.mouseSensitivity;
        }

        void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void Update()
        {
            // Mouse delta is already "how far the mouse moved this frame", so it is NOT multiplied by
            // Time.deltaTime: doing so would make the camera slower at high frame rates.
            Vector2 delta = lookAction.ReadValue<Vector2>() * Sensitivity;

            transform.Rotate(0f, delta.x, 0f);

            // Clamping the pitch keeps the camera from flipping upside down (the rubric checks unplayable angles).
            pitch = Mathf.Clamp(pitch - delta.y, -tuning.maxPitch, tuning.maxPitch);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }
}
