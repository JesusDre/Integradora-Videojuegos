using UnityEngine;

namespace Animas.Entities
{
    // The Llorona's numbers (GDD §13), in an asset so playtests can tune them without touching code.
    [CreateAssetMenu(fileName = "LloronaTuning", menuName = "Ánimas/Llorona Tuning")]
    public class LloronaTuning : ScriptableObject
    {
        [Header("Movement (m/s)")]
        [Tooltip("Drifting along the canals. The Padre walks at 3 and runs at 5.5.")]
        [Min(0f)] public float stalkSpeed = 1.6f;
        [Tooltip("Slower than running: the Padre can always escape her if he has stamina.")]
        [Min(0f)] public float chaseSpeed = 3.4f;
        [Tooltip("How long she waits at each canal point before drifting to the next.")]
        [Min(0f)] public float waitAtPoint = 3f;

        [Header("Senses (m)")]
        [Min(1f)] public float detectRadius = 12f;
        [Tooltip("Farther than this, she gives up and sinks back into the water.")]
        [Min(1f)] public float loseRadius = 22f;
        [Min(0.3f)] public float grabRange = 1.3f;

        [Header("Grab (GDD: drains life; counts as a strong hit for the ritual)")]
        [Tooltip("Damage when she grabs. 15 or more pushes the ritual bar back (GDD P3).")]
        [Min(0f)] public float grabDamage = 15f;
        [Min(0f)] public float drainPerSecond = 4f;
        [Min(0.1f)] public float grabSeconds = 2f;

        [Header("Disappearing")]
        [Tooltip("Seconds hidden under the water after a grab, a repel or losing the Padre.")]
        [Min(0f)] public float hiddenSeconds = 12f;
        [Tooltip("She reappears at a canal point at least this far from the Padre.")]
        [Min(0f)] public float reappearMinDistance = 15f;
    }
}
