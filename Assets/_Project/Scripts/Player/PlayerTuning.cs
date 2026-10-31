using UnityEngine;

namespace Animas.Player
{
    // Balance values for the player live in a data asset so they can be tuned in playtests without touching code.
    // Initial values come from the GDD, section 12 ("Valores iniciales de balance").
    [CreateAssetMenu(fileName = "PlayerTuning", menuName = "Ánimas/Player Tuning")]
    public class PlayerTuning : ScriptableObject
    {
        [Header("Movement (m/s)")]
        [Min(0f)] public float walkSpeed = 3f;
        [Min(0f)] public float runSpeed = 5.5f;
        [Header("Crouch (#136)")]
        [Tooltip("Walking speed while crouched, as a fraction of the normal one.")]
        [Range(0.1f, 1f)] public float crouchSpeedMultiplier = 0.5f;
        [Tooltip("Height of the Padre's body (CharacterController) while crouched; standing it is 1.8 m.")]
        [Min(0.8f)] public float crouchHeight = 1.15f;
        [Tooltip("Cultists see a crouched Padre only within this fraction of their sight range.")]
        [Range(0.1f, 1f)] public float crouchSightMultiplier = 0.5f;
        public float gravity = -9.81f;

        [Header("Stamina")]
        [Min(1f)] public float maxStamina = 100f;
        [Min(0f)] public float runCostPerSecond = 15f;
        [Min(0f)] public float regenPerSecond = 20f;
        [Tooltip("Seconds without spending before stamina starts to come back.")]
        [Min(0f)] public float regenDelay = 1f;
        [Tooltip("After hitting 0, the player can't run or attack until stamina reaches this value.")]
        [Min(0f)] public float exhaustedRecoverThreshold = 25f;

        [Header("Camera")]
        [Min(0.001f)] public float mouseSensitivity = 0.1f;
        [Range(10f, 89f)] public float maxPitch = 80f;

        [Header("Interaction")]
        [Min(0.5f)] public float interactDistance = 2.5f;

        [Header("Prayer (hold R)")]
        [Min(0.1f)] public float prayHoldSeconds = 3f;
        [Min(0.5f)] public float prayRadius = 4f;
        [Min(0.5f)] public float prayDuration = 10f;
        [Min(0f)] public float prayCooldown = 20f;
        [Tooltip("Weapon damage multiplier while the prayer's shield lasts (#131).")]
        [Min(1f)] public float prayDamageMultiplier = 1.5f;
        [Tooltip("Speed while holding R: the GDD says the Padre is almost still and vulnerable.")]
        [Range(0f, 1f)] public float praySpeedMultiplier = 0.15f;
    }
}
