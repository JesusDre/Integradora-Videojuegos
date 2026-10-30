using UnityEngine;

namespace Animas.Enemies
{
    // Balance values for the cultist AI (GDD §12 "Valores iniciales de balance" and §21).
    // Harder instances can use a copy with more range or damage without touching code.
    [CreateAssetMenu(fileName = "CultistTuning", menuName = "Ánimas/Cultist Tuning")]
    public class CultistTuning : ScriptableObject
    {
        [Header("Senses")]
        [Min(0f)] public float sightRange = 15f;
        [Range(10f, 180f)] public float sightAngle = 110f;
        [Tooltip("Hears the player running within this distance (walking is silent).")]
        [Min(0f)] public float hearRunningRange = 10f;

        [Header("Movement (m/s)")]
        [Min(0f)] public float patrolSpeed = 2f;
        [Tooltip("Slower than the player running (5.5), so running away is possible.")]
        [Min(0f)] public float chaseSpeed = 3.5f;
        [Min(0f)] public float patrolWait = 1.5f;
        [Tooltip("Degrees per second when turning to face the player during an attack.")]
        [Min(0f)] public float turnSpeed = 360f;

        [Header("Attack")]
        [Min(0.5f)] public float attackRange = 1.6f;
        [Min(0f)] public float damage = 10f;
        [Min(0.1f)] public float attackCooldown = 1.5f;
        [Tooltip("Seconds from the start of the swing to the moment the machete lands (matches the Attack clip).")]
        [Min(0f)] public float attackHitDelay = 0.45f;

        [Header("Search")]
        [Tooltip("After losing the player, searches the area this long before going back to patrol.")]
        [Min(0f)] public float searchDuration = 10f;
        [Min(0f)] public float searchRadius = 4f;
    }
}
