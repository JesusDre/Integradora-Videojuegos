using Animas.Core;
using Animas.Narrative;
using TMPro;
using UnityEngine;

namespace Animas.UI
{
    // What the Padre says, at the bottom of the screen (GDD §16 and §25). The notes and the
    // Padre's lines (#33) call Show; the subtitles on/off option (#47) hides the text. Since #123 the line
    // is also heard: its recorded voice is looked up in VoiceLines and plays even with subtitles off.
    public class SubtitleUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TMP_Text label;
        [Tooltip("Seconds per character, so long lines stay longer on screen.")]
        [SerializeField, Min(0f)] float secondsPerCharacter = 0.06f;
        [SerializeField, Min(0.5f)] float minimumSeconds = 2f;
        [SerializeField] VoiceLines voices;
        [Tooltip("2D source for the Padre's voice (its AudioVolume follows the Voices slider).")]
        [SerializeField] AudioSource voice;

        float hideAt;

        void Awake() => root.SetActive(false);

        public void Show(string line)
        {
            Speak(line);
            if (!GameSettings.Subtitles) return;
            label.text = line;
            root.SetActive(true);
            hideAt = Time.time + Mathf.Max(minimumSeconds, line.Length * secondsPerCharacter);
        }

        void Speak(string line)
        {
            AudioClip clip = voices != null ? voices.Find(line) : null;
            if (clip == null || voice == null) return;
            // A new line interrupts the previous one: two Padres talking at once would sound wrong.
            voice.Stop();
            voice.clip = clip;
            voice.Play();
        }

        void Update()
        {
            if (root.activeSelf && Time.time >= hideAt) root.SetActive(false);
        }
    }
}
