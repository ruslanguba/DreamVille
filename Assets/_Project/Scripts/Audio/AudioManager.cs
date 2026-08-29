using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine;

using DreamVille.Configs;

namespace DreamVille.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioSettingsData musicDatabase;

        [Header("Audio Sources")]
        public AudioSource musicSource;
        public AudioSource sfxSource;

        // Время жизни задаёт ProjectContext: ни DontDestroyOnLoad, ни защиты от дублей не требуется.
        private void Awake()
        {
            SetStartVolumes();
        }
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PlayMusicForScene(scene.name);
        }
        public void PlayMusicForScene(string sceneName)
        {
            foreach (var entry in musicDatabase.entries)
            {
                if (entry.sceneName == sceneName)
                {
                    PlayMusic(entry.musicClip);
                    return;
                }
            }

            Debug.LogWarning($"AudioManager: Нет музыки для сцены {sceneName}");
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null) return;
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip);
        }

        public void SetMasterVolume(float value) => SetVolume("MasterVolume", value);
        public void SetMusicVolume(float value) => SetVolume("MusicVolume", value);
        public void SetSFXVolume(float value) => SetVolume("SFXVolume", value);

        private void SetVolume(string parameter, float value)
        {
            float dB = Mathf.Log10(Mathf.Clamp(value, 0.001f, 1f)) * 20f;
            audioMixer.SetFloat(parameter, dB);

            PlayerPrefs.SetFloat(parameter, value);
        }

        public float GetVolume(string parameter)
        {
            if (audioMixer.GetFloat(parameter, out float value))
            {
                return Mathf.Pow(10f, value / 20f);
            }
            return 1;
        }

        private void SetStartVolumes()
        {
            float startVolium = musicDatabase.StartVolume;
            SetMusicVolume(startVolium);
            SetSFXVolume(startVolium);    
        }
    }
}
