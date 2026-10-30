using System;
using Animas.Items;
using Animas.Save;
using UnityEngine;

namespace Animas.Levels
{
    // The end of an instance (GDD §6): reaching it after the escape completes the instance and saves
    // (GDD §26: "al terminar una instancia"). The end-of-instance summary and loading the next
    // instance come with #48, which listens to Reached.
    [RequireComponent(typeof(BoxCollider))]
    public class LevelExit : MonoBehaviour
    {
        [SerializeField] string instanceName = "la instancia";

        public static event Action<LevelExit> Reached;
        public bool Done { get; private set; }

        BoxCollider zone;
        Transform player;

        void Awake()
        {
            zone = GetComponent<BoxCollider>();
            zone.isTrigger = true;
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }

        void Update()
        {
            if (Done || player == null || !zone.bounds.Contains(player.position + Vector3.up * 0.5f)) return;
            Done = true;
            var persistence = player.GetComponent<PlayerPersistence>();
            if (persistence != null) SaveSystem.Save(persistence.Capture("salida", 0));
            var inventory = player.GetComponent<Inventory>();
            if (inventory != null) inventory.Notify($"Escapaste de {instanceName}");
            Reached?.Invoke(this);
        }
    }
}
