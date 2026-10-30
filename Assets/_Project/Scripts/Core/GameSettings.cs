using System;
using UnityEngine;

namespace Animas.Core
{
    public enum VolumeCategory { Music, Effects, Voices }

    // The Options menu values (GDD §17): mouse sensitivity, invert Y, volumes and subtitles.
    // Saved in PlayerPrefs (Unity's small settings storage), apart from the game save, so deleting
    // a game never resets the player's options. Every value is clamped when it's set.
    public static class GameSettings
    {
        public const float MinSensitivity = 0.25f;
        public const float MaxSensitivity = 3f;

        // Anything showing these values (the camera, subtitles, sounds) listens and updates itself.
        public static event Action Changed;

        // A multiplier over PlayerTuning.mouseSensitivity: 1 = the designed default.
        public static float Sensitivity
        {
            get => PlayerPrefs.GetFloat("opt_sensitivity", 1f);
            set => SetFloat("opt_sensitivity", Mathf.Clamp(value, MinSensitivity, MaxSensitivity));
        }

        public static bool InvertY
        {
            get => PlayerPrefs.GetInt("opt_invertY", 0) == 1;
            set => SetInt("opt_invertY", value ? 1 : 0);
        }

        public static bool Subtitles
        {
            get => PlayerPrefs.GetInt("opt_subtitles", 1) == 1;
            set => SetInt("opt_subtitles", value ? 1 : 0);
        }

        // 0 to 1. The master volume goes straight to the AudioListener (everything the Padre hears).
        public static float MasterVolume
        {
            get => PlayerPrefs.GetFloat("opt_master", 1f);
            set
            {
                SetFloat("opt_master", Mathf.Clamp01(value));
                AudioListener.volume = MasterVolume;
            }
        }

        public static float GetVolume(VolumeCategory category) => PlayerPrefs.GetFloat("opt_vol_" + category, 1f);
        public static void SetVolume(VolumeCategory category, float value) => SetFloat("opt_vol_" + category, Mathf.Clamp01(value));

        // Called once when the game starts, so the saved master volume applies from the first frame.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void ApplyOnStart() => AudioListener.volume = MasterVolume;

        static void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        static void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }
    }
}
