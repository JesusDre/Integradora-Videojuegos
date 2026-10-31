using System;
using Animas.Core;
using Animas.Items;
using Animas.Player;
using Animas.Save;
using Animas.UI;
using UnityEngine;

namespace Animas.Levels
{
    // The final ritual, the same for every instance (GDD §5, P3 and P6):
    // 1) the 3 key items go into the anchor; 2) its fixed candles are lit with F (if it has any);
    // 3) standing inside the circle drawn around the anchor fills the bar by itself (#131: no key to hold);
    //    stepping out pauses it;
    // 4) cultist waves arrive while it fills; a hit of 15 or more pushes the bar back 10 %;
    // 5) when full, the entity weakens and the escape opens. If the Padre dies, it starts over.
    public class RitualController : MonoBehaviour
    {
        public enum Stage { WaitingOfferings, LightCandles, Ready, Praying, Complete }

        [SerializeField] Anchor anchor;
        [SerializeField] RitualCandle[] candles;
        [SerializeField] WaveSpawner waves;
        [SerializeField] RitualBarUI bar;
        [Tooltip("The escape boat (instance 1) is locked until the ritual is complete. Optional.")]
        [SerializeField] TrajineraRide escapeRide;
        [Tooltip("The escape door (instance 3: the tunnel) stays locked until the ritual is complete. Optional.")]
        [SerializeField] Door escapeDoor;
        [Tooltip("Seconds inside the circle to fill the bar.")]
        [SerializeField, Min(5f)] float prayerSeconds = 60f;
        [Tooltip("Radius of the ritual circle around the anchor.")]
        [SerializeField, Min(1f)] float radius = 4f;
        [Tooltip("The ring drawn on the floor (the same golden material as the prayer circle).")]
        [SerializeField] Material ringMaterial;
        [Tooltip("A hit at least this strong pushes the bar back (GDD: 15).")]
        [SerializeField, Min(0f)] float strongHit = 15f;
        [SerializeField, Range(0f, 1f)] float setback = 0.1f;

        public Stage Current { get; private set; } = Stage.WaitingOfferings;
        public float Progress { get; private set; }
        // How fast praying fills the bar (1 = normal). The Tzitzimime sets 0 while she's invisible and
        // more while the flashlight is on her (GDD §13: only revealed, with light and prayer).
        public float FillMultiplier { get; set; } = 1f;
        // True while the bar is filling (the Padre is inside the circle): the arms join their hands.
        public bool Filling { get; private set; }

        public event Action<Stage> StageChanged;
        // The entity AI (#35, #45) listens: weaken and dissolve when the ritual completes.
        public event Action Completed;

        Transform player;
        Health playerHealth;
        Inventory messages;
        PlayerRespawn respawn;
        LineRenderer ring;
        bool wavesStarted;
        bool wasInside;

        void Start()
        {
            GameObject p = GameObject.FindWithTag("Player");
            player = p.transform;
            playerHealth = p.GetComponent<Health>();
            messages = p.GetComponent<Inventory>();
            respawn = p.GetComponent<PlayerRespawn>();
            ring = BuildRing();

            anchor.OfferingsComplete += OnOfferings;
            foreach (RitualCandle candle in candles) candle.LitUp += OnCandleLit;
            playerHealth.Damaged += OnPlayerHurt;
            if (respawn != null) respawn.Respawned += Restart;
        }

        void OnDestroy()
        {
            if (anchor != null) anchor.OfferingsComplete -= OnOfferings;
            if (playerHealth != null) playerHealth.Damaged -= OnPlayerHurt;
            if (respawn != null) respawn.Respawned -= Restart;
        }

        void OnOfferings()
        {
            if (AllCandlesLit()) SetStage(Stage.Ready, "Entra al círculo del ancla para empezar el ritual");
            else SetStage(Stage.LightCandles, $"Enciende las {candles.Length} veladoras del ancla");
        }

        void OnCandleLit(RitualCandle candle)
        {
            if (Current == Stage.LightCandles && AllCandlesLit())
                SetStage(Stage.Ready, "Entra al círculo del ancla para empezar el ritual");
        }

        void Update()
        {
            bool open = Current == Stage.Ready || Current == Stage.Praying;
            ring.enabled = open;
            if (open) PulseRing();
            bool inside = open && !playerHealth.IsDead && Near();
            Filling = inside;
            // Stepping out pauses the ritual: say it once, not every frame.
            if (wasInside && !inside && Current == Stage.Praying) messages.Notify("Regresa al círculo para seguir el ritual");
            wasInside = inside;
            if (!inside) return;

            if (Current == Stage.Ready) BeginRitual();
            Progress = Mathf.Clamp01(Progress + Time.deltaTime / prayerSeconds * FillMultiplier);
            bar.SetProgress(Progress);
            if (Progress >= 1f) Finish();
        }

        void BeginRitual()
        {
            SetStage(Stage.Praying, null);
            bar.Begin();
            bar.SetProgress(Progress);
            if (!wavesStarted && waves != null)
            {
                wavesStarted = true;
                waves.Begin();
            }
        }

        void OnPlayerHurt(float amount)
        {
            if (Current != Stage.Praying || amount < strongHit) return;
            Progress = Mathf.Max(0f, Progress - setback);
            bar.SetProgress(Progress);
            messages.Notify("¡El golpe te hizo perder el rezo!");
        }

        void Finish()
        {
            SetStage(Stage.Complete, "El ritual está completo. ¡Huye!");
            Filling = false;
            ring.enabled = false;
            if (waves != null) waves.Stop();
            bar.End();
            if (escapeRide != null) escapeRide.Unlock();
            if (escapeDoor != null)
            {
                escapeDoor.Unlock();
                escapeDoor.SetOpen(true);
            }
            Completed?.Invoke();
        }

        // The Padre died: the anchor already emptied itself; the candles go out and the bar resets.
        void Restart()
        {
            if (Current == Stage.Complete) return;
            Progress = 0f;
            FillMultiplier = 1f;
            wavesStarted = false;
            if (waves != null) waves.ResetWaves();
            foreach (RitualCandle candle in candles) candle.SetLit(false);
            bar.End();
            Filling = false;
            SetStage(Stage.WaitingOfferings, null);
        }

        // The ritual circle: a ring on the floor around the anchor, so the player sees where to stand.
        LineRenderer BuildRing()
        {
            var go = new GameObject("CirculoRitual");
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.loop = true;
            line.widthMultiplier = 0.12f;
            line.material = ringMaterial;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            const int points = 64;
            line.positionCount = points;
            Vector3 center = anchor.transform.position;
            for (int i = 0; i < points; i++)
            {
                float a = i * Mathf.PI * 2f / points;
                Vector3 p = center + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * radius;
                // Sit on whatever floor is there (the anchor may stand on a step).
                if (Physics.Raycast(p + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 4f, ~0, QueryTriggerInteraction.Ignore)) p = hit.point;
                line.SetPosition(i, p + Vector3.up * 0.05f);
            }
            line.enabled = false;
            return line;
        }

        // Brighter while filling, softly breathing while it waits.
        void PulseRing()
        {
            float glow = Filling ? 1f : 0.55f + 0.25f * Mathf.Sin(Time.time * 2f);
            Color c = new Color(1f, 0.82f, 0.4f, glow);
            ring.startColor = c;
            ring.endColor = c;
        }

        bool AllCandlesLit()
        {
            foreach (RitualCandle candle in candles)
                if (!candle.Lit) return false;
            return true;
        }

        bool Near()
        {
            Vector3 d = player.position - anchor.transform.position;
            d.y = 0f;
            return d.sqrMagnitude <= radius * radius;
        }

        void SetStage(Stage stage, string message)
        {
            Current = stage;
            if (message != null) messages.Notify(message);
            StageChanged?.Invoke(stage);
        }
    }
}
