using Animas.Core;
using UnityEngine;

namespace Animas.Audio
{
    // Put next to an AudioSource to make it follow the Music, Effects or Voices slider of the Options.
    // Its own volume in the Inspector is the loudest it gets; the slider scales it down.
    [RequireComponent(typeof(AudioSource))]
    public class AudioVolume : MonoBehaviour
    {
        [SerializeField] VolumeCategory category = VolumeCategory.Effects;

        AudioSource source;
        float designedVolume;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            designedVolume = source.volume;
        }

        void OnEnable()
        {
            GameSettings.Changed += Apply;
            Apply();
        }

        void OnDisable() => GameSettings.Changed -= Apply;

        void Apply() => source.volume = designedVolume * GameSettings.GetVolume(category);
    }
}
