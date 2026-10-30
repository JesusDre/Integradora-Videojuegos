using UnityEngine;

namespace Animas.Core
{
    // Which instances the player can choose (GDD §17: they unlock in order). Instance 1 is always
    // open; finishing one opens the next (#48 calls Unlock). Kept in PlayerPrefs, like the options.
    public static class GameProgress
    {
        public const int InstanceCount = 3;

        public static int UnlockedInstances => Mathf.Clamp(PlayerPrefs.GetInt("progress_unlocked", 1), 1, InstanceCount);

        public static bool IsUnlocked(int instanceNumber) => instanceNumber >= 1 && instanceNumber <= UnlockedInstances;

        public static void Unlock(int instanceNumber)
        {
            if (instanceNumber <= UnlockedInstances) return;
            PlayerPrefs.SetInt("progress_unlocked", Mathf.Clamp(instanceNumber, 1, InstanceCount));
            PlayerPrefs.Save();
        }
    }
}
