using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Animas.Enemies;
using Animas.Entities;
using Animas.Levels;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Animas.Core
{
    // Performance test of the build (issue #65). Started ONLY with the command-line argument
    // "-benchmark": it loads each instance, puts the Padre in its heaviest spot (the anchor with its
    // candles, or the Charro's field), brings the level's cultists around him with their AI running, and
    // measures 10 s with vsync off so the number is the real headroom, not the monitor's 60 Hz limit.
    // The result goes to a text file and the game closes. A normal game never runs any of this.
    public class BuildBenchmark : MonoBehaviour
    {
        static readonly string[] Levels = { "I1_Xochimilco", "I2_Hacienda", "I3_Convento" };
        const float WarmUp = 3f;
        const float Measure = 10f;
        const int Cultists = 6;

        readonly StringBuilder report = new StringBuilder();
        static string ResultPath => Path.Combine(Application.persistentDataPath, "benchmark.txt");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartIfAsked()
        {
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-benchmark") < 0) return;
            var go = new GameObject("BuildBenchmark");
            DontDestroyOnLoad(go);
            go.AddComponent<BuildBenchmark>();
        }

        IEnumerator Start()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            // A normal game pauses when its window loses focus (that's correct); the test must not, or it
            // freezes as soon as another window ends up on top.
            Application.runInBackground = true;
            report.AppendLine($"Ánimas mexicanas — benchmark {System.DateTime.Now:yyyy-MM-dd HH:mm}");
            report.AppendLine($"{SystemInfo.graphicsDeviceName} · {SystemInfo.processorType} · {Screen.width}x{Screen.height} · calidad {QualitySettings.names[QualitySettings.GetQualityLevel()]}");

            Debug.Log("[Benchmark] start, results in " + ResultPath);
            foreach (string level in Levels)
            {
                Debug.Log("[Benchmark] loading " + level);
                yield return SceneManager.LoadSceneAsync(level);
                Debug.Log("[Benchmark] loaded " + level);
                yield return null;
                Health padre = PlaceInHeaviestSpot();
                Debug.Log("[Benchmark] placed in " + level);
                yield return Sample(level, padre);
                // Saved after every level: if something fails later, the numbers so far are not lost.
                File.WriteAllText(ResultPath, report.ToString());
                Debug.Log("[Benchmark] " + level + " done");
            }

            Debug.Log("[Benchmark]\n" + report);
            Application.Quit();
        }

        Health PlaceInHeaviestSpot()
        {
            GameObject player = GameObject.FindWithTag("Player");
            Vector3 spot = player.transform.position;
            Anchor anchor = FindAnyObjectByType<Anchor>();
            CharroNegroEntity charro = FindAnyObjectByType<CharroNegroEntity>();
            if (anchor != null) spot = anchor.transform.position + Vector3.back * 4f;
            else if (charro != null) spot = charro.transform.position + Vector3.back * 8f;
            if (NavMesh.SamplePosition(spot, out NavMeshHit hit, 6f, NavMesh.AllAreas)) spot = hit.position;

            var controller = player.GetComponent<CharacterController>();
            controller.enabled = false;
            player.transform.position = spot;
            if (anchor != null) player.transform.LookAt(new Vector3(anchor.transform.position.x, spot.y, anchor.transform.position.z));
            controller.enabled = true;

            // Worst case: every encounter of the level starts at once (they create their cultists the same
            // way as in a real game), and six of them are brought around the Padre.
            foreach (Encounter encounter in FindObjectsByType<Encounter>(FindObjectsSortMode.None))
                if (!encounter.Triggered) encounter.Begin();
            var cultists = new List<CultistAI>(FindObjectsByType<CultistAI>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            for (int i = 0; i < cultists.Count && i < Cultists; i++)
            {
                float angle = i * Mathf.PI * 2f / Cultists;
                Vector3 p = spot + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 9f;
                if (NavMesh.SamplePosition(p, out NavMeshHit h, 4f, NavMesh.AllAreas)) p = h.position;
                cultists[i].gameObject.SetActive(true);
                var agent = cultists[i].GetComponent<NavMeshAgent>();
                if (agent != null) agent.Warp(p); else cultists[i].transform.position = p;
                cultists[i].HearNoise(spot);
            }
            return player.GetComponent<Health>();
        }

        IEnumerator Sample(string level, Health padre)
        {
            float end = Time.realtimeSinceStartup + WarmUp;
            float beat = 0f;
            while (Time.realtimeSinceStartup < end)
            {
                Keep(padre);
                if (Time.realtimeSinceStartup > beat) { beat = Time.realtimeSinceStartup + 1f; Debug.Log($"[Benchmark] warm-up frame {Time.frameCount}"); }
                yield return null;
            }

            var frames = new List<float>();
            var spikes = new List<string>();
            float start = Time.realtimeSinceStartup;
            end = start + Measure;
            while (Time.realtimeSinceStartup < end)
            {
                Keep(padre);
                frames.Add(Time.unscaledDeltaTime);
                // A frame over 50 ms is a visible stutter: note when it happened, to find its cause.
                if (Time.realtimeSinceStartup > beat) { beat = Time.realtimeSinceStartup + 1f; Debug.Log($"[Benchmark] frame {Time.frameCount}"); }
                if (Time.unscaledDeltaTime > 0.05f && spikes.Count < 8)
                    spikes.Add($"{Time.realtimeSinceStartup - start:0.0}s={Time.unscaledDeltaTime * 1000f:0}ms");
                yield return null;
            }

            frames.Sort();
            float total = 0f;
            foreach (float f in frames) total += f;
            float average = frames.Count / total;
            // "1 % low": the FPS of the slowest 1 % of frames, which is what the player feels as stutter.
            int worst = Mathf.Max(1, frames.Count / 100);
            float slow = 0f;
            for (int i = frames.Count - worst; i < frames.Count; i++) slow += frames[i];
            float low = worst / slow;
            int busy = FindObjectsByType<CultistAI>(FindObjectsSortMode.None).Length;
            report.AppendLine($"{level}: {average:0} FPS de promedio · 1 % más lento: {low:0} FPS · {frames.Count} cuadros · {busy} cultistas activos");
            if (spikes.Count > 0) report.AppendLine("    tirones: " + string.Join(", ", spikes));
        }

        // The cultists really attack (their AI is part of the cost), but the Padre must not die mid-test.
        static void Keep(Health padre)
        {
            if (padre != null && padre.Current < padre.Max) padre.Heal(padre.Max);
        }
    }
}
