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
    }
}
