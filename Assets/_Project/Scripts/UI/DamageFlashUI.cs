using Animas.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Animas.UI
{
    // The screen edges flash red when the Padre is hurt, stronger for bigger hits, and fade out.
    public class DamageFlashUI : MonoBehaviour
    {
        [SerializeField] Health playerHealth;
        [SerializeField] Image overlay;
        [SerializeField, Range(0f, 1f)] float maxAlpha = 0.5f;
        [SerializeField, Min(0.1f)] float fadePerSecond = 1.2f;

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
        }

        void Update()
        {
            if (alpha <= 0f) return;
            alpha = Mathf.Max(0f, alpha - fadePerSecond * Time.deltaTime);
            SetAlpha(alpha);
        }

        void SetAlpha(float a)
        {
            Color c = overlay.color;
            c.a = a;
            overlay.color = c;
        }
    }
}

