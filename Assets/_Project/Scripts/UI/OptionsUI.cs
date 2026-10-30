using System;
using Animas.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Animas.UI
{
    // The Options panel (GDD §17), used by both the main menu and the pause menu. Each control writes
    // straight to GameSettings, which saves it and tells the camera, sounds and subtitles.
    public class OptionsUI : MonoBehaviour
    {
        [SerializeField] Slider sensitivity;
        [SerializeField] TMP_Text sensitivityValue;
        [SerializeField] Toggle invertY;
        [SerializeField] Slider master;
        [SerializeField] Slider music;
        [SerializeField] Slider effects;
        [SerializeField] Slider voices;
        [SerializeField] Toggle subtitles;
        [SerializeField] Button back;

        // The menu that opened it shows itself again.
        public event Action Closed;

        void Awake()
        {
            sensitivity.minValue = GameSettings.MinSensitivity;
            sensitivity.maxValue = GameSettings.MaxSensitivity;
            foreach (Slider s in new[] { master, music, effects, voices })
            {
                s.minValue = 0f;
                s.maxValue = 1f;
            }
            back.onClick.AddListener(Close);
        }

        void OnEnable()
        {
            // Show the saved values without triggering the listeners (they would save them again).
            sensitivity.SetValueWithoutNotify(GameSettings.Sensitivity);
            invertY.SetIsOnWithoutNotify(GameSettings.InvertY);
            master.SetValueWithoutNotify(GameSettings.MasterVolume);
            music.SetValueWithoutNotify(GameSettings.GetVolume(VolumeCategory.Music));
            effects.SetValueWithoutNotify(GameSettings.GetVolume(VolumeCategory.Effects));
            voices.SetValueWithoutNotify(GameSettings.GetVolume(VolumeCategory.Voices));
            subtitles.SetIsOnWithoutNotify(GameSettings.Subtitles);
            ShowSensitivity(GameSettings.Sensitivity);

            sensitivity.onValueChanged.AddListener(SetSensitivity);
            invertY.onValueChanged.AddListener(SetInvertY);
            master.onValueChanged.AddListener(SetMaster);
            music.onValueChanged.AddListener(SetMusic);
            effects.onValueChanged.AddListener(SetEffects);
            voices.onValueChanged.AddListener(SetVoices);
            subtitles.onValueChanged.AddListener(SetSubtitles);
        }

        void OnDisable()
        {
            sensitivity.onValueChanged.RemoveListener(SetSensitivity);
            invertY.onValueChanged.RemoveListener(SetInvertY);
            master.onValueChanged.RemoveListener(SetMaster);
            music.onValueChanged.RemoveListener(SetMusic);
            effects.onValueChanged.RemoveListener(SetEffects);
            voices.onValueChanged.RemoveListener(SetVoices);
            subtitles.onValueChanged.RemoveListener(SetSubtitles);
        }

        public Selectable FirstSelected => sensitivity;

        public void Close()
        {
            gameObject.SetActive(false);
            Closed?.Invoke();
        }

        void SetSensitivity(float value)
        {
            GameSettings.Sensitivity = value;
            ShowSensitivity(GameSettings.Sensitivity);
        }

        void ShowSensitivity(float value)
        {
            if (sensitivityValue != null) sensitivityValue.text = $"x{value:0.00}";
        }

        void SetInvertY(bool on) => GameSettings.InvertY = on;
        void SetMaster(float value) => GameSettings.MasterVolume = value;
        void SetMusic(float value) => GameSettings.SetVolume(VolumeCategory.Music, value);
        void SetEffects(float value) => GameSettings.SetVolume(VolumeCategory.Effects, value);
        void SetVoices(float value) => GameSettings.SetVolume(VolumeCategory.Voices, value);
        void SetSubtitles(bool on) => GameSettings.Subtitles = on;
    }
}
