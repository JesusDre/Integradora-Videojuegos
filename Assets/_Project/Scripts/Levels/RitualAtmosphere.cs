using UnityEngine;

namespace Animas.Levels
{
    // The convent's red fog during the ritual (GDD §9): while the bar is being filled, the fog slowly
    // turns red and thicker; when the ritual ends or restarts, it goes back to the level's own fog.
    public class RitualAtmosphere : MonoBehaviour
    {
        [SerializeField] RitualController ritual;
        [SerializeField] Color ritualFogColor = new Color(0.45f, 0.06f, 0.05f);
        [SerializeField, Min(0f)] float ritualFogDensity = 0.05f;
        [Tooltip("Seconds the change takes.")]
        [SerializeField, Min(0.1f)] float blendSeconds = 4f;

        Color normalColor;
        float normalDensity;
        float blend;

        void Start()
        {
            normalColor = RenderSettings.fogColor;
            normalDensity = RenderSettings.fogDensity;
        }

        void Update()
        {
            bool active = ritual.Current == RitualController.Stage.Praying;
            blend = Mathf.MoveTowards(blend, active ? 1f : 0f, Time.deltaTime / blendSeconds);
            RenderSettings.fogColor = Color.Lerp(normalColor, ritualFogColor, blend);
            RenderSettings.fogDensity = Mathf.Lerp(normalDensity, ritualFogDensity, blend);
        }

        // RenderSettings belong to the scene asset in the Editor: leave them as they were.
        void OnDestroy()
        {
            RenderSettings.fogColor = normalColor;
            RenderSettings.fogDensity = normalDensity;
        }
    }
}
