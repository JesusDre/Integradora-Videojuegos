using System;
using Animas.Core;
using Animas.Items;
using Animas.Player;
using Animas.Save;
using Animas.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Animas.Levels
{
    // The final ritual, the same for every instance (GDD §5, P3 and P6):
    // 1) the 3 key items go into the anchor; 2) its fixed candles are lit with F (if it has any);
    // 3) holding R near the anchor fills the bar, with no cooldown; letting go pauses it;
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
        [Tooltip("Seconds of holding R to fill the bar.")]
        [SerializeField, Min(5f)] float prayerSeconds = 60f;
        [Tooltip("How close to the anchor the Padre must be to pray.")]
        [SerializeField, Min(1f)] float radius = 4f;
        [Tooltip("A hit at least this strong pushes the bar back (GDD: 15).")]
        [SerializeField, Min(0f)] float strongHit = 15f;
        [SerializeField, Range(0f, 1f)] float setback = 0.1f;

        public Stage Current { get; private set; } = Stage.WaitingOfferings;
        public float Progress { get; private set; }
        // How fast praying fills the bar (1 = normal). The Tzitzimime sets 0 while she's invisible and
        // more while the flashlight is on her (GDD §13: only revealed, with light and prayer).
        public float FillMultiplier { get; set; } = 1f;

        public event Action<Stage> StageChanged;
        // The entity AI (#35, #45) listens: weaken and dissolve when the ritual completes.
        public event Action Completed;

        Transform player;
        Health playerHealth;
        PlayerPrayer prayer;
        Inventory messages;
        PlayerRespawn respawn;
        InputAction prayAction;
        bool wavesStarted;

        void Start()
        {
            GameObject p = GameObject.FindWithTag("Player");
            player = p.transform;
            playerHealth = p.GetComponent<Health>();
            prayer = p.GetComponent<PlayerPrayer>();
            messages = p.GetComponent<Inventory>();
            respawn = p.GetComponent<PlayerRespawn>();
            prayAction = InputSystem.actions.FindAction("Player/Pray", true);

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
            if (AllCandlesLit()) SetStage(Stage.Ready, "Mantén R junto al ancla para empezar el ritual");
            else SetStage(Stage.LightCandles, $"Enciende las {candles.Length} veladoras del ancla");
        }

        void OnCandleLit(RitualCandle candle)
        {
            if (Current == Stage.LightCandles && AllCandlesLit())
                SetStage(Stage.Ready, "Mantén R junto al ancla para empezar el ritual");
        }

        void Update()
        {
            bool atAnchor = (Current == Stage.Ready || Current == Stage.Praying) && !playerHealth.IsDead && Near();
            // Near the anchor, R is for the ritual; away from it, the normal prayer works again.
            prayer.Suppressed = atAnchor;
            if (!atAnchor || !prayAction.IsPressed()) return;

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
            prayer.Suppressed = false;
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
            prayer.Suppressed = false;
            SetStage(Stage.WaitingOfferings, null);
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
