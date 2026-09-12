using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using Zenject;

using DreamVille.Audio;

namespace DreamVille.UI
{
    public class AudioSettingsUI : MonoBehaviour
    {
        [Header("Sliders")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        private AudioManager _audio;

        [Inject]
        public void Construct(AudioManager audio)
        {
            _audio = audio;
        }

        private IEnumerator Start()
        {
            // Waits one frame so the sliders finish their own Awake/OnEnable setup
            // before we overwrite their value and attach listeners.
            yield return new WaitForEndOfFrame();
            musicSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.RemoveAllListeners();

            musicSlider.value = _audio.MusicVolume;
            sfxSlider.value = _audio.SfxVolume;

            musicSlider.onValueChanged.AddListener(_audio.SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(_audio.SetSFXVolume);
        }
    }
}
