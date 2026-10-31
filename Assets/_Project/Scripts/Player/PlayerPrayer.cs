using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Animas.Player
{
    // Hold R to pray (GDD §5; 1.5 s since #131): the Padre leaves a 4 m circle that keeps spirits out for
    // 10 s, and for those same 10 s a blessed shield surrounds him and his weapons hit 50 % harder.
    // While holding R he can barely move; letting go early cancels it with no cooldown.
    // After a prayer, 20 s must pass before the next one.
    public class PlayerPrayer : MonoBehaviour
    {
        [SerializeField] PlayerTuning tuning;
        [SerializeField] PlayerController movement;
        [SerializeField] PrayerCircle circlePrefab;
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip prayClip;
        [SerializeField] AudioClip doneClip;
        [Tooltip("The Padre murmuring in Latin while he prays; one at random each time (#123).")]
        [SerializeField] AudioClip[] wordClips;

        public bool IsPraying => holdStarted >= 0f;
        public float CooldownLeft => Mathf.Max(0f, readyAt - Time.time);
        public bool IsReady => CooldownLeft <= 0f;
        // Blocks R entirely when set (kept for scripted moments; the ritual no longer needs it since #131).
        public bool Suppressed { get; set; }
        // The shield of the last prayer: active for prayDuration seconds after it completes.
        public bool ShieldActive => Time.time < shieldUntil;
        public float ShieldLeft => Mathf.Max(0f, shieldUntil - Time.time);
        // Weapons multiply their damage by this (1 when there is no shield).
        public float DamageMultiplier => ShieldActive ? tuning.prayDamageMultiplier : 1f;

        // 0..1 while R is held (the HUD fills a bar); -1 when not praying.
        public event Action<float> ProgressChanged;
        public event Action Completed;
        public event Action<string> Notice;

        InputAction prayAction;
        float holdStarted = -1f;
        float readyAt;
        float shieldUntil;

        void Awake() => prayAction = InputSystem.actions.FindAction("Player/Pray", true);

        void OnDisable() => Cancel();

        void Update()
        {
            if (Suppressed)
            {
                Cancel();
                return;
            }
            if (prayAction.WasPressedThisFrame())
            {
                if (IsReady) Begin();
                else Notice?.Invoke($"Aún no puedes rezar ({Mathf.CeilToInt(CooldownLeft)} s)");
            }
            if (!IsPraying) return;

            if (!prayAction.IsPressed())
            {
                Cancel();
                return;
            }
            float progress = (Time.time - holdStarted) / tuning.prayHoldSeconds;
            ProgressChanged?.Invoke(Mathf.Clamp01(progress));
            if (progress >= 1f) Complete();
        }

        void Begin()
        {
            holdStarted = Time.time;
            movement.SpeedMultiplier = tuning.praySpeedMultiplier;
            if (audioSource != null && prayClip != null) audioSource.PlayOneShot(prayClip);
            if (audioSource != null && wordClips != null && wordClips.Length > 0)
                audioSource.PlayOneShot(wordClips[UnityEngine.Random.Range(0, wordClips.Length)]);
        }

        void Complete()
        {
            Stop();
            readyAt = Time.time + tuning.prayCooldown;
            PrayerCircle circle = Instantiate(circlePrefab);
            circle.Begin(transform.position, tuning.prayRadius, tuning.prayDuration);
            shieldUntil = Time.time + tuning.prayDuration;
            if (audioSource != null && doneClip != null) audioSource.PlayOneShot(doneClip);
            Completed?.Invoke();
        }

        void Cancel()
        {
            if (IsPraying) Stop();
        }

        void Stop()
        {
            holdStarted = -1f;
            if (movement != null) movement.SpeedMultiplier = 1f;
            ProgressChanged?.Invoke(-1f);
        }

        // For tests and for the ritual: pray instantly, as if R had been held the whole time.
        public bool PrayNow()
        {
            if (!IsReady) return false;
            Complete();
            return true;
        }
    }
}
