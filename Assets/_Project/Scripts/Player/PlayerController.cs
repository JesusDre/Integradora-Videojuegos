using UnityEngine;
using UnityEngine.InputSystem;

namespace Animas.Player
{
    // First-person movement: WASD to walk, Shift to run while stamina lasts, Ctrl held to crouch (#136):
    // slower, silent (no running) and lower, so cultists only see him from closer. No jump (out of scope).
    // CharacterController instead of a Rigidbody: it collides with walls and climbs small steps without physics jitter.
    [RequireComponent(typeof(CharacterController), typeof(PlayerStamina))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] PlayerTuning tuning;
        [Tooltip("The camera goes down with the body when crouching.")]
        [SerializeField] Transform cameraPivot;

        public bool IsRunning { get; private set; }
        public bool IsMoving { get; private set; }
        public bool IsCrouching { get; private set; }
        public float SightMultiplier => IsCrouching ? tuning.crouchSightMultiplier : 1f;
        // Other systems slow the Padre down (praying leaves him almost still). 1 = normal speed.
        public float SpeedMultiplier { get; set; } = 1f;

        CharacterController characterController;
        PlayerStamina stamina;
        InputAction moveAction;
        InputAction sprintAction;
        InputAction crouchAction;
        float verticalVelocity;
        float standHeight, standCameraY;

        void Awake()
        {
            characterController = GetComponent<CharacterController>();
            stamina = GetComponent<PlayerStamina>();
            // Project-wide actions (AnimasControls) are enabled automatically by the Input System.
            moveAction = InputSystem.actions.FindAction("Player/Move", true);
            sprintAction = InputSystem.actions.FindAction("Player/Sprint", true);
            crouchAction = InputSystem.actions.FindAction("Player/Crouch", true);
            standHeight = characterController.height;
            if (cameraPivot != null) standCameraY = cameraPivot.localPosition.y;
        }

        void Update()
        {
            Vector2 input = moveAction.ReadValue<Vector2>();
            // Clamp so walking diagonally isn't faster than walking straight.
            Vector3 direction = Vector3.ClampMagnitude(transform.right * input.x + transform.forward * input.y, 1f);
            IsMoving = direction.sqrMagnitude > 0.01f;

            UpdateCrouch();

            // No running while slowed down (praying) or crouched.
            IsRunning = IsMoving && !IsCrouching && SpeedMultiplier >= 1f && sprintAction.IsPressed() && stamina.CanSpend;
            float speed = (IsRunning ? tuning.runSpeed : tuning.walkSpeed) * SpeedMultiplier;
            if (IsCrouching) speed *= tuning.crouchSpeedMultiplier;
            if (IsRunning) stamina.Spend(tuning.runCostPerSecond * Time.deltaTime);

            // A small downward push while grounded keeps isGrounded stable on slopes and steps.
            if (characterController.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            verticalVelocity += tuning.gravity * Time.deltaTime;

            Vector3 velocity = direction * speed + Vector3.up * verticalVelocity;
            // Multiplying by deltaTime makes the speed in m/s independent of the frame rate.
            characterController.Move(velocity * Time.deltaTime);
        }

        void UpdateCrouch()
        {
            bool wants = crouchAction.IsPressed();
            // Letting go of Ctrl under something low (a crypt, a table): stay down until there is room.
            if (IsCrouching && !wants && !RoomToStand()) wants = true;
            IsCrouching = wants;

            // The body and the camera slide down and up instead of jumping (a few frames, by time).
            float targetHeight = IsCrouching ? tuning.crouchHeight : standHeight;
            float h = Mathf.MoveTowards(characterController.height, targetHeight, 4f * Time.deltaTime);
            characterController.height = h;
            characterController.center = Vector3.up * (h / 2f);
            if (cameraPivot != null)
            {
                Vector3 p = cameraPivot.localPosition;
                p.y = standCameraY - (standHeight - h);
                cameraPivot.localPosition = p;
            }
        }

        bool RoomToStand()
        {
            float radius = characterController.radius * 0.9f;
            Vector3 bottom = transform.position + Vector3.up * (characterController.height - radius);
            return !Physics.SphereCast(bottom, radius, Vector3.up, out _, standHeight - characterController.height + 0.05f,
                ~0, QueryTriggerInteraction.Ignore);
        }
    }
}
