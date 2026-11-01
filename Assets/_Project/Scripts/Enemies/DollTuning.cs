using UnityEngine;

namespace Animas.Enemies
{
    // The possessed doll's numbers (#172), in an asset so playtests can tune them without touching code.
    [CreateAssetMenu(fileName = "DollTuning", menuName = "Ánimas/Doll Tuning")]
    public class DollTuning : ScriptableObject
    {
        [Tooltip("She wakes up when the Padre comes closer than this.")]
        [Min(1f)] public float wakeDistance = 22f;
        [Tooltip("How fast she moves while nobody looks at her. Faster than walking (3): turning your back is a risk.")]
        [Min(0.5f)] public float moveSpeed = 4f;
        [Tooltip("She catches the Padre at this distance.")]
        [Min(0.3f)] public float catchDistance = 1.1f;
        [Min(0f)] public float catchDamage = 15f;
        [Tooltip("After catching him she goes back to where she was and waits this long.")]
        [Min(0f)] public float restSeconds = 10f;
        [Tooltip("She laughs while she moves and the Padre is closer than this.")]
        [Min(1f)] public float laughDistance = 14f;
        [Tooltip("Seconds between laughs (random between both).")]
        public Vector2 laughEvery = new Vector2(3f, 7f);
    }
}
