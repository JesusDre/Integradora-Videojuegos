using UnityEngine;

namespace Animas.UI
{
    // The ritual's progress bar, top center, visible only during a ritual (GDD §16).
    // The ritual system (#34) calls Begin, SetProgress and End.
    public class RitualBarUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] StatBar bar;

        void Awake() => root.SetActive(false);

        public void Begin()
        {
            bar.Set(0f, 1f);
            root.SetActive(true);
        }

        // progress goes from 0 (just started) to 1 (complete).
        public void SetProgress(float progress) => bar.Set(Mathf.Clamp01(progress) * 100f, 100f);

        public void End() => root.SetActive(false);
    }
}
