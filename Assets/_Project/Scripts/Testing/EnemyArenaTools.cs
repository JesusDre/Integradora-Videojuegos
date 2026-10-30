using System;
using System.Collections.Generic;
using Animas.Core;
using Animas.Enemies;
using Animas.Items;
using Animas.UI;
using UnityEngine;

namespace Animas.Testing
{
    // The actions behind the arena's test buttons. Public methods so TestButton can call them
    // from the Inspector without extra code.
    public class EnemyArenaTools : MonoBehaviour
    {
        [Serializable]
        public struct ItemGift
        {
            public ItemDefinition item;
            [Min(1)] public int amount;
        }

        [SerializeField] Health playerHealth;
        [SerializeField] Inventory playerInventory;
        [SerializeField] NotificationUI notifications;
        [SerializeField] CultistAI cultistPrefab;
        [SerializeField] Transform[] spawnPoints;
        [Tooltip("Spawned cultists walk between these.")]
        [SerializeField] Transform[] spawnPatrol;
        [SerializeField, Min(1)] int maxSpawned = 6;
        [SerializeField] ItemGift[] testItems;
        [Tooltip("The arena's main light: the lights button switches between lit (to watch) and dark (like the real levels).")]
        [SerializeField] Light mainLight;
        [SerializeField] float darkIntensity = 0.05f;
        [SerializeField] Color darkAmbient = new Color(0.03f, 0.035f, 0.05f);

        readonly List<CultistAI> spawned = new List<CultistAI>();
        int nextSpawnPoint;
        bool dark;
        float litIntensity;
        Color litAmbient;

        void Awake()
        {
            litIntensity = mainLight.intensity;
            litAmbient = RenderSettings.ambientLight;
        }

        public void ToggleLights()
        {
            dark = !dark;
            mainLight.intensity = dark ? darkIntensity : litIntensity;
            RenderSettings.ambientLight = dark ? darkAmbient : litAmbient;
            Notify(dark ? "Luces apagadas: usa la linterna (E)" : "Luces encendidas");
        }

        public void SpawnCultist()
        {
            spawned.RemoveAll(c => c == null);
            // A cap so holding F by accident doesn't fill the arena.
            if (spawned.Count >= maxSpawned)
            {
                Notify($"Ya hay {maxSpawned} cultistas de prueba");
                return;
            }
            Transform point = spawnPoints[nextSpawnPoint];
            nextSpawnPoint = (nextSpawnPoint + 1) % spawnPoints.Length;

            // The route is set right after Instantiate, before the cultist's Start picks its first point.
            CultistAI cultist = Instantiate(cultistPrefab, point.position, point.rotation);
            cultist.SetPatrol(spawnPatrol);
            spawned.Add(cultist);
            cultist.name = $"Cultista_extra_{spawned.Count}";
            Notify("Apareció un cultista");
        }

        public void RemoveSpawned()
        {
            foreach (CultistAI cultist in spawned)
                if (cultist != null) Destroy(cultist.gameObject);
            spawned.Clear();
            Notify("Cultistas de prueba eliminados");
        }

        public void HealPlayer()
        {
            playerHealth.Heal(playerHealth.Max);
            Notify("Vida al máximo");
        }

        public void HurtPlayer(float amount)
        {
            playerHealth.TakeDamage(amount);
        }

        public void GiveTestItems()
        {
            foreach (ItemGift gift in testItems) playerInventory.TryAdd(gift.item, gift.amount);
        }

        void Notify(string message)
        {
            if (notifications != null) notifications.Show(message);
        }
    }
}
