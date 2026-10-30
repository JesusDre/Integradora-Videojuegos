using UnityEngine;

namespace Animas.Entities
{
    // The Tzitzimime's numbers (GDD §13), in an asset so playtests can tune them without touching code.
    [CreateAssetMenu(fileName = "TzitzimimeTuning", menuName = "Ánimas/Tzitzimime Tuning")]
    public class TzitzimimeTuning : ScriptableObject
    {
        [Header("Phases")]
        [Tooltip("Ritual progress at which she appears on the ceiling (phase 1 = only cultist waves).")]
        [Range(0.1f, 0.9f)] public float phaseTwoAt = 0.5f;
        [Tooltip("Fill speed while revealed (1 = normal) and while revealed with the flashlight on her.")]
        [Min(0f)] public float revealedFill = 1f;
        [Min(0f)] public float litFill = 2f;

        [Header("Ceiling (phase 2)")]
        [Tooltip("Height of the ceiling she hangs from (the catacombs' ceiling is at 5 m).")]
        [Min(1f)] public float ceilingHeight = 5f;
        [Min(0f)] public float ceilingSpeed = 2.5f;

        [Header("Area attack")]
        [Tooltip("Seconds between attacks in phase 2; the final form attacks more often.")]
        [Min(0.5f)] public float attackInterval = 7f;
        [Min(0.5f)] public float finalAttackInterval = 4f;
        [Tooltip("The red circle shows this long before the hit, so the Padre can step out.")]
        [Min(0.1f)] public float warningSeconds = 1.5f;
        [Min(0.5f)] public float attackRadius = 3f;
        [Tooltip("15 or more pushes the ritual bar back (GDD P3).")]
        [Min(0f)] public float attackDamage = 20f;

        [Header("Final form (phase 3)")]
        [Tooltip("She walks after the Padre during the escape. Slower than walking (3): it's pressure, not a wall.")]
        [Min(0f)] public float finalSpeed = 2.2f;
    }
}
