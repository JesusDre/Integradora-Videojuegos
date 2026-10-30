using System;
using System.Collections.Generic;
using Animas.Enemies;
using UnityEngine;

namespace Animas.Levels
{
    // A scripted group of cultists (GDD §5 "Encuentros"): when the Padre walks into this zone, the
    // group appears at its spawn points, with the size the instance's difficulty says (2–3, 3–4, 4–5).
    // Happens once. Never inside a safe room.
    [RequireComponent(typeof(BoxCollider))]
    public class Encounter : MonoBehaviour
    {
        [SerializeField] InstanceDifficulty difficulty;
        [SerializeField] CultistSpawner spawner;
        [SerializeField] Transform[] spawnPoints;
        [SerializeField] Transform[] patrolPoints;
        [Tooltip("0 = use the difficulty's range. Any other number forces that many.")]
        [SerializeField, Min(0)] int fixedCount;
        [Tooltip("The group arrives already hunting the Padre (an ambush) instead of patrolling.")]
        [SerializeField] bool ambush;

        public bool Triggered { get; private set; }
        public int Alive => alive.Count;

        public event Action<Encounter> Started;
        public event Action<Encounter> Cleared;

        readonly List<CultistAI> alive = new List<CultistAI>();
        BoxCollider zone;
        Transform player;
        bool clearedSent;

        void Awake()
        {
            zone = GetComponent<BoxCollider>();
            zone.isTrigger = true;
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }

        void Update()
        {
            // Same reason as SafeRoom: checking the position never misses (teleports, respawns).
            if (!Triggered && player != null && !SafeRoom.PlayerInside && zone.bounds.Contains(player.position + Vector3.up * 0.5f))
                Begin();

            if (Triggered && alive.Count == 0 && !clearedSent)
            {
                clearedSent = true;
                Cleared?.Invoke(this);
            }
        }

        public void Begin()
        {
            if (Triggered) return;
            Triggered = true;
            int count = fixedCount > 0 ? fixedCount : difficulty.RollEncounterSize();
            for (int i = 0; i < count; i++)
            {
                Transform point = spawnPoints[i % spawnPoints.Length];
                // Several cultists on one point would overlap: shift each a little.
                Vector3 offset = Quaternion.Euler(0f, i * 137f, 0f) * Vector3.forward * (i < spawnPoints.Length ? 0f : 0.8f);
                spawner.Spawn(difficulty, point.position + offset, point.rotation, patrolPoints,
                    ambush && player != null ? player.position : (Vector3?)null, alive);
            }
            Started?.Invoke(this);
        }

        void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider>();
            Gizmos.color = new Color(1f, 0.2f, 0.1f, 0.25f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
    }
}
