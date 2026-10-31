using Animas.Core;
using Animas.Enemies;
using Animas.Levels;
using UnityEngine;

namespace Animas.Audio
{
    // The level's sound bed, the way Left 4 Dead does it: the ambience is always there, the tension music
    // rises only while a cultist is chasing or attacking the Padre and falls when they lose him, an
    // encounter opens with a stinger ("they're coming"), and the ritual has its own drums. Everything
    // fades out when the Padre dies (the defeat screen plays its own music).
    public class MusicDirector : MonoBehaviour
    {
        [SerializeField] AudioSource ambience;
        [SerializeField] AudioSource tension;
        [SerializeField] AudioSource ritual;
        [SerializeField] AudioSource stinger;
        [SerializeField] Health playerHealth;
        [SerializeField] RitualController ritualController;
        [Tooltip("Seconds the tension music keeps playing after the last chase ends.")]
        [SerializeField, Min(0f)] float tensionHold = 4f;
        [SerializeField, Min(0.1f)] float fadeSpeed = 0.5f;

        float ambienceVolume, tensionVolume, ritualVolume;
        float tensionUntil;
        float nextCheck;
        CultistAI[] cultists = new CultistAI[0];

        void Awake()
        {
            // Inspector volumes are the loudest each layer gets; the music ones start silent.
            ambienceVolume = ambience.volume;
            tensionVolume = tension.volume;
            ritualVolume = ritual != null ? ritual.volume : 0f;
            tension.volume = 0f;
            if (ritual != null) ritual.volume = 0f;
        }

        void OnEnable()
        {
            foreach (Encounter e in FindObjectsByType<Encounter>(FindObjectsSortMode.None)) e.Started += OnEncounterStarted;
        }

        void OnDisable()
        {
            foreach (Encounter e in FindObjectsByType<Encounter>(FindObjectsSortMode.None)) e.Started -= OnEncounterStarted;
        }

        void Update()
        {
            // Twice a second is enough to notice a chase, and cheaper than every frame. The list is
            // refreshed too, because waves spawn new cultists.
            if (Time.time >= nextCheck)
            {
                nextCheck = Time.time + 0.5f;
                cultists = FindObjectsByType<CultistAI>(FindObjectsSortMode.None);
                if (AnyoneHunting()) tensionUntil = Time.time + tensionHold;
            }

            bool dead = playerHealth != null && playerHealth.IsDead;
            bool praying = ritualController != null && ritualController.Current == RitualController.Stage.Praying;
            float music = GameSettings.GetVolume(VolumeCategory.Music);
            float effects = GameSettings.GetVolume(VolumeCategory.Effects);

            Fade(ambience, dead ? 0f : ambienceVolume * effects);
            Fade(tension, !dead && !praying && Time.time < tensionUntil ? tensionVolume * music : 0f);
            if (ritual != null) Fade(ritual, !dead && praying ? ritualVolume * music : 0f);
        }

        bool AnyoneHunting()
        {
            foreach (CultistAI c in cultists)
                if (c.isActiveAndEnabled && (c.Current == CultistAI.State.Chase || c.Current == CultistAI.State.Attack))
                    return true;
            return false;
        }

        void OnEncounterStarted(Encounter encounter)
        {
            stinger.volume = GameSettings.GetVolume(VolumeCategory.Music);
            stinger.Play();
            // The horde is coming: start the tension right away instead of waiting for the first chase.
            tensionUntil = Time.time + tensionHold + 6f;
        }

        void Fade(AudioSource layer, float target)
        {
            layer.volume = Mathf.MoveTowards(layer.volume, target, fadeSpeed * Time.deltaTime);
            if (layer.volume > 0f && !layer.isPlaying) layer.Play();
            else if (layer.volume <= 0f && layer.isPlaying) layer.Stop();
        }
    }
}
