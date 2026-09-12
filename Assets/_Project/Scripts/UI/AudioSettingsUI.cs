using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

using DreamVille.Audio;

namespace DreamVille.UI
{
    /// <summary>Options screen sliders bound to the persisted audio settings.</summary>
    public class AudioSettingsUI : MonoBehaviour
    {
        [Header("Sliders")]
        [FormerlySerializedAs("musicSlider")]
        [SerializeField] private Slider _musicSlider;
        [FormerlySerializedAs("sfxSlider")]
        [SerializeField] private Slider _sfxSlider;

        private IAudioSettings _settings;

        [Inject]
        public void Construct(IAudioSettings settings)
        {
            _settings = settings;
        }

        private void OnEnable()
        {
            _musicSlider.SetValueWithoutNotify(_settings.MusicVolume);
            _sfxSlider.SetValueWithoutNotify(_settings.SfxVolume);

            _musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
            _sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        }

        private void OnDisable()
        {
            _musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
            _sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);
        }

        private void OnMusicSliderChanged(float value) => _settings.SetMusicVolume(value);

        private void OnSfxSliderChanged(float value) => _settings.SetSfxVolume(value);
    }
}
