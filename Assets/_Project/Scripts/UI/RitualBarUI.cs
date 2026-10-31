using TMPro;
using UnityEngine;

namespace Animas.UI
{
    // The ritual's progress bar, top center, visible only during a ritual (GDD §16).
    // The ritual system (#34) calls Begin, SetProgress and End. The hacienda has no ritual, so the
    // Charro uses the same bar for his life in the final fight (#158), with his own title.
    public class RitualBarUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] StatBar bar;
        [Tooltip("The title over the bar. Empty: it keeps the one in the prefab.")]
        [SerializeField] TMP_Text title;

        void Awake() => root.SetActive(false);

        public void Begin()
        {
            bar.Set(0f, 1f);
            root.SetActive(true);
        }

        // progress goes from 0 (just started) to 1 (complete).
        public void SetProgress(float progress) => bar.Set(Mathf.Clamp01(progress) * 100f, 100f);

        public void End() => root.SetActive(false);

        public void SetTitle(string text)
        {
            if (title != null) title.text = text;
        }
    }
}
