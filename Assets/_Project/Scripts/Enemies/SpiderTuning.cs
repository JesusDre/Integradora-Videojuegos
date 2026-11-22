using UnityEngine;

namespace Animas.Enemies
{
    // The ceiling spider's numbers (#174), in an asset so playtests can tune them without touching code.
    [CreateAssetMenu(fileName = "SpiderTuning", menuName = "Ánimas/Spider Tuning")]
    public class SpiderTuning : ScriptableObject
    {
        [Tooltip("It drops when the Padre passes under it, closer than this (measured on the floor).")]
        [Min(0.5f)] public float triggerRadius = 2.2f;
        [Min(1f)] public float dropSpeed = 9f;
        [Tooltip("How fast it pulls the Padre up toward the ceiling.")]
        [Min(0.1f)] public float liftSpeed = 0.6f;
        [Min(0f)] public float damagePerSecond = 4f;
        [Tooltip("If it gets him to the ceiling: one hard hit, and it lets go.")]
        [Min(0f)] public float topDamage = 25f;
        [Tooltip("Presses of F to break free.")]
        [Min(1)] public int mashToEscape = 8;
        [Tooltip("After letting go it climbs back and waits this long before it can drop again.")]
        [Min(0f)] public float cooldown = 8f;
    }
}
