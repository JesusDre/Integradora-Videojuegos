using System;
using UnityEngine;

namespace Animas.Narrative
{
    // The recorded voice of each line the Padre says (issue #123). SubtitleUI looks the text up here, so
    // PadreVoice and the SubtitleTriggers keep working with plain text and the voice simply follows.
    [CreateAssetMenu(fileName = "VoiceLines", menuName = "Ánimas/Voice lines")]
    public class VoiceLines : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            [TextArea] public string text;
            public AudioClip clip;
        }

        public Entry[] entries;

        // Compared without the «» quotes and spaces, so «Levántate, Anselmo.» and "Levántate, Anselmo." match.
        public AudioClip Find(string line)
        {
            if (entries == null || string.IsNullOrEmpty(line)) return null;
            string key = Clean(line);
            foreach (Entry e in entries)
                if (e.clip != null && Clean(e.text) == key) return e.clip;
            return null;
        }

        static string Clean(string s) => s.Trim().Trim('«', '»').Trim();
    }
}
