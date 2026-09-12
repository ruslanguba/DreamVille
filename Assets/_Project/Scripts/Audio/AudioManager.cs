using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine;
using Zenject;

using DreamVille.Configs;
using DreamVille.Persistence;

namespace DreamVille.Audio
{
    /// <summary>
    /// Scene-agnostic music/SFX player. Lives in ProjectContext for the lifetime of the app;
    /// no DontDestroyOnLoad or duplicate-guard is needed.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        // Mixer parameter names; they happen to equal the SaveKeys volume keys, kept verbatim.
        private const string MusicMixerParam = "MusicVolume";
        private const string SfxMixerParam = "SFXVolume";

        [Header("Audio Mixer")]
        [FormerlySerializedAs("audioMixer")]
        [SerializeField] private AudioMixer _audioMixer;
        [FormerlySerializedAs("musicDatabase")]
        [SerializeField] private AudioSettingsData _musicDatabase;

        [Header("Audio Sources")]
        [FormerlySerializedAs("musicSource")]
        [SerializeField] private AudioSource _musicSource;
        [FormerlySerializedAs("sfxSource")]
        [SerializeField] private AudioSource _sfxSource;

        /// <summary>Current music volume in the 0..1 range, kept in sync with the saved value.</summary>
        public float MusicVolume { get; private set; }

        /// <summary>Current SFX volume in the 0..1 range, kept in sync with the saved value.</summary>
        public float SfxVolume { get; private set; }

        private IAudioSettingsRepository _settings;

        [Inject]
        private void Construct(IAudioSettingsRepository settings)
        {
            _settings = settings;
            LoadStartVolumes();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            // Volumes are written to the repository on every slider change; flush them once here
            // instead of hitting the disk on each frame of a drag.
            _settings.Flush();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PlayMusicForScene(scene.name);
        }

        public void PlayMusicForScene(string sceneName)
        {
            foreach (var entry in _musicDatabase.entries)
            {
                if (entry.sceneName == sceneName)
                {
                    PlayMusic(entry.musicClip);
                    return;
                }
            }

            Debug.LogWarning($"[{nameof(AudioManager)}] No music configured for scene '{sceneName}'.");
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null) return;
            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.Play();
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            _sfxSource.PlayOneShot(clip);
        }

        /// <summary>Applies the music volume to the mixer and remembers it for the next flush.</summary>
        public void SetMusicVolume(float value)
        {
            ApplyVolume(MusicMixerParam, value);
            MusicVolume = value;
            _settings.SetMusicVolume(value);
        }

        /// <summary>Applies the SFX volume to the mixer and remembers it for the next flush.</summary>
        public void SetSFXVolume(float value)
        {
            ApplyVolume(SfxMixerParam, value);
            SfxVolume = value;
            _settings.SetSfxVolume(value);
        }

        private void ApplyVolume(string parameter, float value)
        {
            float dB = Mathf.Log10(Mathf.Clamp(value, 0.001f, 1f)) * 20f;
            _audioMixer.SetFloat(parameter, dB);
        }

        private void LoadStartVolumes()
        {
            float music = _settings.TryGetMusicVolume(out float savedMusic) ? savedMusic : _musicDatabase.StartVolume;
            float sfx = _settings.TryGetSfxVolume(out float savedSfx) ? savedSfx : _musicDatabase.StartVolume;

            ApplyVolume(MusicMixerParam, music);
            ApplyVolume(SfxMixerParam, sfx);
            MusicVolume = music;
            SfxVolume = sfx;
        }
    }
}
