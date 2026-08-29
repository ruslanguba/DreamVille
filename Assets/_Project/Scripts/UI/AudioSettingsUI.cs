using System.Collections;
using UnityEngine.UI;
using UnityEngine;

using DreamVille.Audio;

namespace DreamVille.UI
{
    public class AudioSettingsUI : MonoBehaviour
    {
        [Header("Sliders")]
        //[SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            musicSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.RemoveAllListeners();
            // Загружаем текущие громкости в UI
            //masterSlider.value = AudioManager.Instance.GetVolume("MasterVolume");
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.3f);
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.3f);

            // Подписываем события
            //masterSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
            musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
        }
    }
}
