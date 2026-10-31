using Animas.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Animas.UI
{
    // The prayer's shield as the Padre sees it (#131): a golden glow at the edges of the screen that
    // breathes while the shield lasts and fades out in its last second, plus a warm light around him.
    public class PrayerShieldUI : MonoBehaviour
    {
        [SerializeField] Image glow;
        [SerializeField, Range(0f, 1f)] float maxAlpha = 0.35f;
        [Tooltip("Optional warm light that follows the Padre while the shield lasts.")]
        [SerializeField] Light aura;
        [SerializeField, Min(0f)] float auraIntensity = 1.6f;

        PlayerPrayer prayer;

        void Start()
        {
            // The HUD is its own prefab: the Padre is found once by his tag.
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) prayer = p.GetComponent<PlayerPrayer>();
            if (aura == null && p != null) aura = CreateAura(p.transform);
        }

        void Update()
        {
            float strength = 0f;
            if (prayer != null && prayer.ShieldActive)
            {
                float breath = 0.75f + 0.25f * Mathf.Sin(Time.time * 4f);
                strength = Mathf.Clamp01(prayer.ShieldLeft) * breath;   // fades during its last second
            }
            Color c = glow.color;
            c.a = maxAlpha * strength;
            glow.color = c;
            glow.enabled = strength > 0.001f;
            if (aura != null)
            {
                aura.intensity = auraIntensity * strength;
                aura.enabled = strength > 0.001f;
            }
        }

        static Light CreateAura(Transform padre)
        {
            var go = new GameObject("AuraRezo");
            go.transform.SetParent(padre, false);
            go.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.8f, 0.45f);
            light.range = 5f;
            light.shadows = LightShadows.None;
            light.enabled = false;
            return light;
        }
    }
}
