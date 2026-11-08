using System;
using UnityEngine;
using UnityEngine.UI;

namespace DreamVille.UI.Menus
{
    /// <summary>
    /// Music and effects sliders of an options screen (main menu and pause menu).
    /// Passive: reports what the player drags and shows what <see cref="AudioSettingsPresenter"/> sends.
    /// </summary>
    public sealed class AudioSettingsView : MonoBehaviour, IAudioSettingsView
    {
        [Header("Sliders")]
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;

        public event Action<float> MusicVolumeEdited;

        public event Action<float> SfxVolumeEdited;

        private void Awake()
        {
            if (_musicSlider == null)
                Debug.LogError($"[{nameof(AudioSettingsView)}] {nameof(_musicSlider)} is not assigned.", this);
            if (_sfxSlider == null)
                Debug.LogError($"[{nameof(AudioSettingsView)}] {nameof(_sfxSlider)} is not assigned.", this);
        }

        private void OnEnable()
        {
            if (_musicSlider != null)
                _musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
            if (_sfxSlider != null)
                _sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        }

        private void OnDisable()
        {
            if (_musicSlider != null)
                _musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
            if (_sfxSlider != null)
                _sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);
        }

        public void ShowMusicVolume(float normalized)
        {
            _musicSlider.SetValueWithoutNotify(normalized);
        }

        public void ShowSfxVolume(float normalized)
        {
            _sfxSlider.SetValueWithoutNotify(normalized);
        }

        private void OnMusicSliderChanged(float normalized)
        {
            MusicVolumeEdited?.Invoke(normalized);
        }

        private void OnSfxSliderChanged(float normalized)
        {
            SfxVolumeEdited?.Invoke(normalized);
        }
    }
}
