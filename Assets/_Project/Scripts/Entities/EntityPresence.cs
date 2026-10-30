using Animas.UI;
using UnityEngine;
using UnityEngine.Rendering;

namespace Animas.Entities
{
    // How the Padre feels an entity nearby (GDD §13 and §16): the closer it is, the more the screen
    // distorts (a post-processing Volume whose weight goes 0 → 1) and the louder its sound. A subtitle
    // describes the sound, so a player without audio also knows it's close.
    public class EntityPresence : MonoBehaviour
    {
        [Tooltip("Volume with the distortion (lens distortion, chromatic aberration...). Weight starts at 0.")]
        [SerializeField] Volume distortion;
        [Tooltip("Only felt while this is active (the model: hidden when she's under the water).")]
        [SerializeField] GameObject presentWhen;
        [SerializeField] AudioSource loop;
        [SerializeField] SubtitleUI subtitles;
        [SerializeField, TextArea] string subtitle = "(Se oye un llanto) ¡Ay, mis hijos!";
        [Tooltip("Farther than this, nothing is felt.")]
        [SerializeField, Min(1f)] float radius = 18f;
        [Tooltip("How fast the effect follows the distance (higher = snappier).")]
        [SerializeField, Min(0.1f)] float smoothing = 3f;
        [SerializeField, Min(0f)] float subtitleCooldown = 20f;

        Transform listener;
        float weight;
        float nextSubtitle;

        void Start()
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) listener = p.transform;
            if (distortion != null) distortion.weight = 0f;
        }

        void Update()
        {
            if (listener == null) return;
            float closeness = 0f;
            if (presentWhen == null || presentWhen.activeInHierarchy)
                closeness = 1f - Mathf.Clamp01(Vector3.Distance(transform.position, listener.position) / radius);

            // Smoothed, so the screen doesn't jump when she appears or sinks.
            weight = Mathf.MoveTowards(weight, closeness, smoothing * Time.deltaTime);
            if (distortion != null) distortion.weight = weight;
            if (loop != null) loop.volume = Mathf.Lerp(0.2f, 1f, weight);

            if (closeness > 0f && subtitles != null && Time.time >= nextSubtitle)
            {
                subtitles.Show(subtitle);
                nextSubtitle = Time.time + subtitleCooldown;
            }
        }

        // Dissolved or destroyed: the screen goes back to normal.
        void OnDisable()
        {
            if (distortion != null) distortion.weight = 0f;
        }
    }
}
