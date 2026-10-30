using System;
using Animas.Enemies;
using Animas.Items;
using UnityEngine;

namespace Animas.Levels
{
    // How hard one instance is (GDD §12 and §20): how many cultists per encounter, how strong they
    // are, the ritual waves and what cultists drop. One asset per instance (Data/Difficulty), so
    // each instance is harder than the last by changing numbers, not code.
    [CreateAssetMenu(fileName = "DIF_", menuName = "Ánimas/Instance Difficulty")]
    public class InstanceDifficulty : ScriptableObject
    {
        [Serializable]
        public struct Drop
        {
            public ItemDefinition item;
            [Min(1)] public int amount;
            [Range(0f, 1f)] public float chance;
        }

        public string instanceName = "Instancia";

        [Header("Cultists")]
        [Tooltip("Speed, damage and senses of the cultists in this instance.")]
        public CultistTuning cultistTuning;
        [Min(1)] public int encounterMin = 2;
        [Min(1)] public int encounterMax = 3;

        [Header("Ritual waves (0 = this instance has no waves)")]
        [Min(0)] public int waveCount = 3;
        [Min(1)] public int waveSize = 3;
        [Tooltip("The next wave comes when the last one is defeated, or after this many seconds at most.")]
        [Min(1f)] public float maxSecondsBetweenWaves = 25f;

        [Header("Loot")]
        [Tooltip("Each entry is rolled when a cultist dies (GDD §19: sal, trapo, sometimes ammo...).")]
        public Drop[] cultistDrops;

        public int RollEncounterSize() => UnityEngine.Random.Range(encounterMin, Mathf.Max(encounterMin, encounterMax) + 1);
    }
}
