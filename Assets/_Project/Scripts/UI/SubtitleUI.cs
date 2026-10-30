using TMPro;
using UnityEngine;

namespace Animas.UI
{
    // What the Padre says, at the bottom of the screen (GDD §16 and §25). The notes and the
    // Padre's lines (#33) call Show; the subtitles on/off option (#47) will switch this off.
    public class SubtitleUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TMP_Text label;
        [Tooltip("Seconds per character, so long lines stay longer on screen.")]
        [SerializeField, Min(0f)] float secondsPerCharacter = 0.06f;
        [SerializeField, Min(0.5f)] float minimumSeconds = 2f;

        float hideAt;

        void Awake() => root.SetActive(false);

        public void Show(string line)
        {
            label.text = line;
            root.SetActive(true);
            hideAt = Time.time + Mathf.Max(minimumSeconds, line.Length * secondsPerCharacter);
        }

        void Update()
        {
            if (root.activeSelf && Time.time >= hideAt) root.SetActive(false);
        }
    }
}
