using UnityEngine;

namespace Animas.Enemies
{
    // The chaneque's numbers (#173), in an asset so playtests can tune them without touching code.
    [CreateAssetMenu(fileName = "ChanequeTuning", menuName = "Ánimas/Chaneque Tuning")]
    public class ChanequeTuning : ScriptableObject
    {
        [Tooltip("It notices the Padre closer than this.")]
        [Min(1f)] public float wakeDistance = 25f;
        [Tooltip("While it stalks it keeps between these distances, circling him.")]
        public Vector2 stalkDistance = new Vector2(7f, 13f);
        [Min(0.5f)] public float stalkSpeed = 3.2f;
        [Tooltip("The sudden run at the Padre. Almost as fast as running (5.5).")]
        [Min(0.5f)] public float dashSpeed = 5.2f;
        [Tooltip("Seconds of stalking before it dashes (random between both).")]
        public Vector2 dashEvery = new Vector2(5f, 10f);
        [Tooltip("A dash that hasn't caught him after this long gives up.")]
        [Min(0.5f)] public float dashSeconds = 4f;
        [Min(0.3f)] public float catchDistance = 1.1f;
        [Min(0f)] public float catchDamage = 5f;
        [Tooltip("After stealing it runs away this long and vanishes with the loot.")]
        [Min(0.5f)] public float fleeSeconds = 7f;
        [Min(0.5f)] public float fleeSpeed = 6f;
        [Tooltip("Seconds between laughs: up close (first) and far away (second). The closer, the more it laughs.")]
        public Vector2 laughEvery = new Vector2(1.5f, 8f);
    }
}
