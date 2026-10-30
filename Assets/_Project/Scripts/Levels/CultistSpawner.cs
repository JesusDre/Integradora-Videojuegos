using System;
using System.Collections.Generic;
using Animas.Core;
using Animas.Enemies;
using Animas.Items;
using UnityEngine;

namespace Animas.Levels
{
    // Creates cultists for encounters and ritual waves, set up for the instance's difficulty:
    // its tuning, a patrol route and what they drop when they die. Encounter and WaveSpawner use it.
    [Serializable]
    public class CultistSpawner
    {
        public CultistAI prefab;
        [Tooltip("Used when a cultist drops loot.")]
        public GameObject pickupPrefab;

        public CultistAI Spawn(InstanceDifficulty difficulty, Vector3 position, Quaternion rotation,
            Transform[] patrol, Vector3? alertAt, List<CultistAI> alive)
        {
            CultistAI cultist = UnityEngine.Object.Instantiate(prefab, position, rotation);
            cultist.SetTuning(difficulty.cultistTuning);
            cultist.SetPatrol(patrol);
            if (alertAt.HasValue) cultist.HearNoise(alertAt.Value);
            alive.Add(cultist);

            Health health = cultist.GetComponent<Health>();
            // When it dies: leave the list and roll its loot where it fell.
            health.Died += () =>
            {
                alive.Remove(cultist);
                DropLoot(difficulty, cultist.transform.position);
            };
            return cultist;
        }

        void DropLoot(InstanceDifficulty difficulty, Vector3 at)
        {
            if (pickupPrefab == null || difficulty.cultistDrops == null) return;
            int dropped = 0;
            foreach (var drop in difficulty.cultistDrops)
            {
                if (drop.item == null || UnityEngine.Random.value > drop.chance) continue;
                // Spread the drops a little so they don't sit inside each other.
                Vector3 offset = new Vector3(0.35f * dropped, 0.15f, 0.25f * (dropped % 2));
                GameObject go = UnityEngine.Object.Instantiate(pickupPrefab, at + offset, Quaternion.identity);
                if (go.TryGetComponent(out PickupItem pickup)) pickup.Set(drop.item, drop.amount);
                dropped++;
            }
        }
    }
}
