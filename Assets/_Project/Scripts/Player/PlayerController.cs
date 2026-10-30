using UnityEngine;
using UnityEngine.InputSystem;

namespace Animas.Player
{
    // First-person movement: WASD to walk, Shift to run while stamina lasts. No jump or crouch (out of scope in the GDD).
    // CharacterController instead of a Rigidbody: it collides with walls and climbs small steps without physics jitter.
    [RequireComponent(typeof(CharacterController), typeof(PlayerStamina))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] PlayerTuning tuning;

        public bool IsRunning { get; private set; }
        public bool IsMoving { get; private set; }
        // Other systems slow the Padre down (praying leaves him almost still). 1 = normal speed.
        public float SpeedMultiplier { get; set; } = 1f;

        CharacterController characterController;
        PlayerStamina stamina;
        InputAction moveAction;
        InputAction sprintAction;
        float verticalVelocity;

        void Awake()
        {
            characterController = GetComponent<CharacterController>();
            stamina = GetComponent<PlayerStamina>();
            // Project-wide actions (AnimasControls) are enabled automatically by the Input System.
            moveAction = InputSystem.actions.FindAction("Player/Move", true);
            sprintAction = InputSystem.actions.FindAction("Player/Sprint", true);
        }

        void Update()
        {
            Vector2 input = moveAction.ReadValue<Vector2>();
            // Clamp so walking diagonally isn't faster than walking straight.
            Vector3 direction = Vector3.ClampMagnitude(transform.right * input.x + transform.forward * input.y, 1f);
            IsMoving = direction.sqrMagnitude > 0.01f;

            // No running while slowed down (praying).
            IsRunning = IsMoving && SpeedMultiplier >= 1f && sprintAction.IsPressed() && stamina.CanSpend;
            float speed = (IsRunning ? tuning.runSpeed : tuning.walkSpeed) * SpeedMultiplier;
            if (IsRunning) stamina.Spend(tuning.runCostPerSecond * Time.deltaTime);

            // A small downward push while grounded keeps isGrounded stable on slopes and steps.
            if (characterController.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            verticalVelocity += tuning.gravity * Time.deltaTime;

            Vector3 velocity = direction * speed + Vector3.up * verticalVelocity;
            // Multiplying by deltaTime makes the speed in m/s independent of the frame rate.
            characterController.Move(velocity * Time.deltaTime);
        }
    }
}
