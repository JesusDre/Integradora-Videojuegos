using System;
using System.Collections.Generic;
using Animas.Enemies;
using UnityEngine;

namespace Animas.Levels
{
    // The ritual waves (GDD §5 and §12): 3 waves of 3 in instance 1, 4 of 4 in instance 3.
    // The ritual (#34) calls Begin and Stop. A wave comes when the previous one is defeated, or
    // after maxSecondsBetweenWaves even if it isn't, so the pressure never drops for long.
    // The cultists arrive already hunting the Padre.
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] InstanceDifficulty difficulty;
        [SerializeField] CultistSpawner spawner;
        [Tooltip("Where the waves come from (doors, tunnels, the canal shore...).")]
        [SerializeField] Transform[] spawnPoints;
        [SerializeField, Min(0f)] float firstWaveDelay = 2f;

        public bool Running { get; private set; }
        public int WavesStarted { get; private set; }
        public int Alive => alive.Count;
        public bool Finished => WavesStarted >= difficulty.waveCount && alive.Count == 0;

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
            if (Running || difficulty.waveCount == 0) return;
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

            if (WavesStarted < difficulty.waveCount)
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
            nextWaveAt = Time.time + difficulty.maxSecondsBetweenWaves;
            for (int i = 0; i < difficulty.waveSize; i++)
            {
                Transform point = spawnPoints[(i + WavesStarted) % spawnPoints.Length];
                Vector3 offset = Quaternion.Euler(0f, i * 137f, 0f) * Vector3.forward * (i < spawnPoints.Length ? 0f : 0.8f);
                // No patrol route: they come straight for the Padre.
                spawner.Spawn(difficulty, point.position + offset, point.rotation, null,
                    player != null ? player.position : point.position, alive);
            }
            WaveStarted?.Invoke(WavesStarted);
        }
    }
}
