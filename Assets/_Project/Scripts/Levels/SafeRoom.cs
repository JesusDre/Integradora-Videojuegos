using System;
using Animas.Core;
using Animas.Save;
using UnityEngine;

namespace Animas.Levels
{
    // The safe room between the two sections (GDD §5 and §26). Its box covers the inside:
    // entering closes the door behind the Padre and saves; leaving saves again (to keep what was
    // crafted). Inside, cultists can't see or hear him and spirits can't come in.
    // It is also where the Padre respawns after dying.
    // It checks the Padre's position every frame instead of using trigger events: a teleport
    // (respawning) doesn't always produce enter/exit events, a position check never misses.
    [RequireComponent(typeof(BoxCollider))]
    public class SafeRoom : MonoBehaviour, ISpiritWard
    {
        [Tooltip("Unique per level, stored in the save (e.g. \"I1_Choza\").")]
        [SerializeField] string id = "SafeRoom";
        [Tooltip("1 if the safe room is reached from section 1 (always, in the MVP).")]
        [SerializeField] int sectionAfter = 2;
        [Tooltip("Every door of the room (the choza has one on each side). All close when the Padre enters.")]
        [SerializeField] Door[] doors;
        [SerializeField] Transform respawnPoint;
        [Tooltip("Where a spirit that touches the room is sent: just outside the door.")]
        [SerializeField] Transform spiritExit;

        public string Id => id;
        public Transform RespawnPoint => respawnPoint;

        // Only one safe room per level, and the player is in at most one.
        public static bool PlayerInside { get; private set; }
        public static SafeRoom Last { get; private set; }
        public static event Action<SafeRoom> Saved;

        BoxCollider area;
        Transform player;
        bool inside;

        void Awake()
        {
            area = GetComponent<BoxCollider>();
            area.isTrigger = true;
            // Found once, never in Update (ARQUITECTURA: no Find calls every frame).
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }

        void OnEnable() => SpiritWards.Add(this);

        void OnDisable()
        {
            SpiritWards.Remove(this);
            PlayerInside = false;
        }

        void Update()
        {
            if (player == null) return;
            bool nowInside = Contains(player.position);
            if (nowInside == inside) return;
            inside = nowInside;
            PlayerInside = inside;
            if (inside)
            {
                Last = this;
                foreach (Door door in doors)
                    if (door != null) door.SetOpen(false);
            }
            // Saving on both: entering keeps the progress, leaving keeps what was crafted.
            SaveNow(player.GetComponent<PlayerPersistence>());
        }

        void SaveNow(PlayerPersistence player)
        {
            if (player == null) return;
            SaveSystem.Save(player.Capture(id, sectionAfter));
            Saved?.Invoke(this);
        }

        public bool Contains(Vector3 point) => area.bounds.Contains(point + Vector3.up * 0.5f);

        public Vector3 PushOut(Vector3 point) => spiritExit != null ? spiritExit.position : point;

        // Domain reload can be off in the Editor: start each Play Mode clean.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnPlay()
        {
            PlayerInside = false;
            Last = null;
        }
    }
}
