using Animas.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Animas.UI
{
    // The screen edges flash red when the Padre is hurt, stronger for bigger hits, and fade out (the overlay is
    // a vignette: clear in the middle, red at the edges). With low life the edges keep pulsing, and every hit
    // also shakes the camera (issue #44).
    public class DamageFlashUI : MonoBehaviour
    {
        [SerializeField] Health playerHealth;
        [SerializeField] Image overlay;
        [SerializeField, Range(0f, 1f)] float maxAlpha = 0.5f;
        [SerializeField, Min(0.1f)] float fadePerSecond = 1.2f;
        [Tooltip("Shakes on every hit. Optional.")]
        [SerializeField] Animas.Player.PlayerLook look;
        [Tooltip("At or below this life the edges pulse red.")]
        [SerializeField, Min(0f)] float lowHealthAt = 30f;
        [SerializeField, Range(0f, 1f)] float lowHealthAlpha = 0.3f;

        float alpha;

        void OnEnable()
        {
            // The prefab can't point to the player's Health; each scene must wire it.
            if (playerHealth == null)
            {
                Debug.LogError($"[DamageFlashUI] '{name}' has no player Health assigned.", this);
                enabled = false;
                return;
            }
            playerHealth.Damaged += OnDamaged;
            SetAlpha(0f);
        }

        void OnDisable()
        {
            if (playerHealth != null) playerHealth.Damaged -= OnDamaged;
        }

        void OnDamaged(float amount)
        {
            // A 10-point hit gives a medium flash; 30 or more is the strongest.
            alpha = Mathf.Max(alpha, Mathf.Clamp01(amount / 30f) * maxAlpha + 0.15f);
            // Small drains (the Llorona's grip, fire) barely shake; a charge or a blow shakes hard.
            if (look != null && amount >= 1f) look.AddShake(Mathf.Clamp01(amount / 25f));
        }

        void Update()
        {
            alpha = Mathf.Max(0f, alpha - fadePerSecond * Time.deltaTime);
            // Low life: a slow heartbeat on the edges, under the flash of new hits.
            float pulse = 0f;
            if (!playerHealth.IsDead && playerHealth.Current <= lowHealthAt)
                pulse = lowHealthAlpha * (0.6f + 0.4f * Mathf.Sin(Time.time * 4f));
            SetAlpha(Mathf.Max(alpha, pulse));
        }

        void SetAlpha(float a)
        {
            Color c = overlay.color;
            c.a = a;
            overlay.color = c;
        }
    }
}

