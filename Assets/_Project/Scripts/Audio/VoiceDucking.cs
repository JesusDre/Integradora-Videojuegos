using UnityEngine;
using UnityEngine.Audio;

namespace Animas.Audio
{
    // While the Padre speaks, the music and the ambience step back a few dB so his words are clear, and
    // come back when he is done (issue #50; Left 4 Dead does the same with its characters' lines). It
    // only moves two exposed volumes of the AudioMixer; the sources themselves are untouched.
    public class VoiceDucking : MonoBehaviour
    {
        [SerializeField] AudioMixer mixer;
        [Tooltip("The Padre's voice source (SubtitleUI's).")]
        [SerializeField] AudioSource voice;
        [Tooltip("How much quieter the music and the ambience get while he speaks.")]
        [SerializeField, Range(0f, 20f)] float duckDb = 8f;
        [Tooltip("dB per second when going down and coming back up.")]
        [SerializeField, Min(1f)] float speed = 30f;

        const string MusicParam = "VolMusica";
        const string AmbienceParam = "VolAmbiente";

        float musicBase, ambienceBase, duck;

        void Start()
        {
            // The mixer's own levels (the balance) are the starting point; ducking subtracts from them.
            mixer.GetFloat(MusicParam, out musicBase);
            mixer.GetFloat(AmbienceParam, out ambienceBase);
        }

        void Update()
        {
            float target = voice != null && voice.isPlaying ? -duckDb : 0f;
            duck = Mathf.MoveTowards(duck, target, speed * Time.unscaledDeltaTime);
            mixer.SetFloat(MusicParam, musicBase + duck);
            mixer.SetFloat(AmbienceParam, ambienceBase + duck);
        }

        // Leaving the level must not leave the music lowered for the next scene.
        void OnDestroy()
        {
            if (mixer == null) return;
            mixer.SetFloat(MusicParam, musicBase);
            mixer.SetFloat(AmbienceParam, ambienceBase);
        }
    }
}
