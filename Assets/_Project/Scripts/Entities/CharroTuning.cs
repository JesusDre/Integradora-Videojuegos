using UnityEngine;

namespace Animas.Entities
{
    // The Charro Negro's numbers (GDD §13), in an asset so playtests can tune them without touching code.
    [CreateAssetMenu(fileName = "CharroTuning", menuName = "Ánimas/Charro Tuning")]
    public class CharroTuning : ScriptableObject
    {
        [Header("Movement (m/s)")]
        [Tooltip("Trotting after the Padre. A bit slower than running (5.5), so hiding is possible.")]
        [Min(0f)] public float chaseSpeed = 5f;
        [Tooltip("The charge is much faster, but in a straight line: stepping aside dodges it.")]
        [Min(0f)] public float chargeSpeed = 12f;

        [Header("Charge")]
        [Tooltip("He charges when the Padre is closer than this.")]
        [Min(1f)] public float chargeRange = 14f;
        [Tooltip("How far past the Padre's position the charge keeps going.")]
        [Min(0f)] public float chargeOvershoot = 8f;
        [Min(0.5f)] public float maxChargeSeconds = 3f;
        [Tooltip("Pause after a charge before trotting again (the horse turns around).")]
        [Min(0f)] public float recoverSeconds = 1.5f;
        [Min(0f)] public float chargeDamage = 25f;
        [Min(0.3f)] public float hitRange = 1.8f;

        [Header("Final phase")]
        [Tooltip("Seconds he stays weakened after crossing a full canaleta: the window for a charged cross hit.")]
        [Min(0.5f)] public float weakenedSeconds = 4f;
        [Min(1)] public int hitsToDefeat = 3;
    }
}
