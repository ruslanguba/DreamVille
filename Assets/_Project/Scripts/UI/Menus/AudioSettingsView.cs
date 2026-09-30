using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace DreamVille.UI.Menus
{
    /// <summary>
    /// Music and effects sliders of an options screen (main menu and pause menu).
    /// Passive: reports what the player drags and shows what <see cref="AudioSettingsPresenter"/> sends.
    /// </summary>
    public sealed class AudioSettingsView : MonoBehaviour, IAudioSettingsView
    {
        // The pause menu prefab still stores the pre-rename keys until it is re-saved.
        [Header("Sliders")]
        [FormerlySerializedAs("musicSlider")]
        [SerializeField] private Slider _musicSlider;
        [FormerlySerializedAs("sfxSlider")]
        [SerializeField] private Slider _sfxSlider;

        public event Action<float> MusicVolumeEdited;

        public event Action<float> SfxVolumeEdited;

        private void OnEnable()
        {
            _musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
            _sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        }

        private void OnDisable()
        {
            _musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
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
