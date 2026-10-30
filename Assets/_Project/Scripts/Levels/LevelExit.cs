using System;
using Animas.Core;
using Animas.Items;
using UnityEngine;

namespace Animas.Levels
{
    // The end of an instance (GDD §6): reaching it after the escape completes the instance.
    // The victory screen (#48) listens to Reached: it saves the luggage for the next instance,
    // unlocks it and shows the summary.
    [RequireComponent(typeof(BoxCollider))]
    public class LevelExit : MonoBehaviour
    {
        [Tooltip("Completes \"Escapaste ...\": \"de Xochimilco\", \"de la hacienda\", \"del ex-convento\".")]
        [SerializeField] string instanceName = "de la instancia";

        public static event Action<LevelExit> Reached;
        public bool Done { get; private set; }
        public string InstanceName => instanceName;

        BoxCollider zone;
        Transform player;
        Health playerHealth;

        void Awake()
        {
            zone = GetComponent<BoxCollider>();
            zone.isTrigger = true;
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                playerHealth = p.GetComponent<Health>();
            }
        }

        void Update()
        {
            if (Done || player == null || !zone.bounds.Contains(player.position + Vector3.up * 0.5f)) return;
            // A body sliding into the exit zone doesn't escape: only a living Padre completes the instance.
            if (playerHealth != null && playerHealth.IsDead) return;
            Done = true;
            var inventory = player.GetComponent<Inventory>();
            if (inventory != null) inventory.Notify($"Escapaste {instanceName}");
            Reached?.Invoke(this);
        }
    }
}
