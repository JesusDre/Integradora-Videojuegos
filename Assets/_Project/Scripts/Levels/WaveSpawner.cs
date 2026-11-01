using System;
using System.Collections.Generic;
using Animas.Enemies;
using UnityEngine;

namespace Animas.Levels
{
    // The ritual waves (GDD §5 and §12): 3 waves of 3 in instance 1, 4 of 4 in instance 3.
    // The ritual (#34) calls Begin and Stop. A wave comes when the previous one is defeated, or
    // after maxSecondsBetweenWaves even if it isn't, so the pressure never drops for long.
    // The cultists arrive already hunting the Padre. Since #162 they come as a horde: all from one
    // direction (the first spawn point), running, and they never lose track of him.
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] InstanceDifficulty difficulty;
        [SerializeField] CultistSpawner spawner;
        [Tooltip("Where the waves come from (doors, tunnels, the canal shore...).")]
        [SerializeField] Transform[] spawnPoints;
        [SerializeField, Min(0f)] float firstWaveDelay = 2f;
        [Header("Horde (#162)")]
        [Tooltip("All the cultists come from the first spawn point, in a pack, instead of spreading over every point.")]
        [SerializeField] bool singleDirection = true;
        [Tooltip("Their chase speed is multiplied by this: they run (the Padre runs at 5.5).")]
        [SerializeField, Min(1f)] float runMultiplier = 1.35f;
        [Tooltip("0 = take the waves from the instance's difficulty. More = this spawner's own (the escape).")]
        [SerializeField, Min(0)] int waveCountOverride;
        [SerializeField, Min(0)] int waveSizeOverride;
        [SerializeField, Min(0f)] float secondsBetweenOverride;

        public bool Running { get; private set; }
        public int WavesStarted { get; private set; }
        public int Alive => alive.Count;
        public bool Finished => WavesStarted >= WaveCount && alive.Count == 0;

        int WaveCount => waveCountOverride > 0 ? waveCountOverride : difficulty.waveCount;
        int WaveSize => waveSizeOverride > 0 ? waveSizeOverride : difficulty.waveSize;
        float SecondsBetween => secondsBetweenOverride > 0f ? secondsBetweenOverride : difficulty.maxSecondsBetweenWaves;

        public event Action<int> WaveStarted;   // wave number, starting at 1
        public event Action AllWavesDefeated;

        readonly List<CultistAI> alive = new List<CultistAI>();
        Transform player;
        float nextWaveAt;
        bool doneSent;

        void Awake()
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }

        public void Begin()
        {
            if (Running || WaveCount == 0) return;
            Running = true;
            WavesStarted = 0;
            doneSent = false;
            nextWaveAt = Time.time + firstWaveDelay;
        }

        // The ritual finished (or was abandoned): no more waves. Cultists already out keep fighting.
        public void Stop() => Running = false;

        // The Padre died during the ritual: it will be repeated from the start, so its cultists go away.
        public void ResetWaves()
        {
            Running = false;
            WavesStarted = 0;
            foreach (CultistAI cultist in alive.ToArray())
                if (cultist != null) Destroy(cultist.gameObject);
            alive.Clear();
        }

        void Update()
        {
            if (!Running) return;

            if (WavesStarted < WaveCount)
            {
                bool previousDefeated = WavesStarted > 0 && alive.Count == 0;
                if (Time.time >= nextWaveAt || previousDefeated) SpawnWave();
            }
            else if (alive.Count == 0 && !doneSent)
            {
                doneSent = true;
                Running = false;
                AllWavesDefeated?.Invoke();
            }
        }

        void SpawnWave()
        {
            WavesStarted++;
            nextWaveAt = Time.time + SecondsBetween;
            for (int i = 0; i < WaveSize; i++)
            {
                Transform point;
                Vector3 offset;
                if (singleDirection)
                {
                    // A pack at one point: rows of 3, 1.2 m apart, lined up behind it.
                    point = spawnPoints[0];
                    offset = point.right * ((i % 3 - 1) * 1.2f) - point.forward * (i / 3 * 1.2f);
                }
                else
                {
                    point = spawnPoints[(i + WavesStarted) % spawnPoints.Length];
                    offset = Quaternion.Euler(0f, i * 137f, 0f) * Vector3.forward * (i < spawnPoints.Length ? 0f : 0.8f);
                }
                Vector3 at = point.position + offset;
                if (UnityEngine.AI.NavMesh.SamplePosition(at, out UnityEngine.AI.NavMeshHit hit, 2f, UnityEngine.AI.NavMesh.AllAreas)) at = hit.position;
                // No patrol route: they come straight for the Padre.
                CultistAI cultist = spawner.Spawn(difficulty, at, point.rotation, null,
                    player != null ? player.position : point.position, alive);
                cultist.MakeHorde(runMultiplier);
            }
            WaveStarted?.Invoke(WavesStarted);
        }
    }
}
