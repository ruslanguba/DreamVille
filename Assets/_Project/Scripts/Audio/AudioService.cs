using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

using DreamVille.Configs;
using DreamVille.Persistence;

namespace DreamVille.Audio
{
    /// <summary>
    /// Application-scoped audio: applies the saved volumes, plays the music the catalog
    /// assigns to the active scene and keeps both in sync when a scene is loaded.
    /// </summary>
    public sealed class AudioService : IAudioService, IAudioSettings, IInitializable, IDisposable
    {
        private readonly IAudioOutput _output;
        private readonly IAudioSettingsRepository _settings;
        private readonly SceneCatalog _catalog;
        private readonly AudioConfig _config;

        public AudioService(
            IAudioOutput output,
            IAudioSettingsRepository settings,
            SceneCatalog catalog,
            AudioConfig config)
        {
            _output = output;
            _settings = settings;
            _catalog = catalog;
            _config = config;
        }

        public event Action<float> MusicVolumeChanged;
        public event Action<float> SfxVolumeChanged;

        public float MusicVolume { get; private set; }

        public float SfxVolume { get; private set; }

        public void Initialize()
        {
            ApplyMusicVolume(_settings.TryGetMusicVolume(out float music) ? music : _config.DefaultVolume);
            ApplySfxVolume(_settings.TryGetSfxVolume(out float sfx) ? sfx : _config.DefaultVolume);

            SceneManager.sceneLoaded += OnSceneLoaded;

            // The scene that was active when the service was created never raises sceneLoaded.
            PlaySceneMusic(SceneManager.GetActiveScene().name);
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            // The sliders write on every frame of a drag; the disk is touched once, here.
            _settings.Flush();
        }

        public void PlaySfx(AudioClip clip) => _output.PlaySfx(clip);

        public void PlayMusic(AudioClip clip, bool loop = true) => _output.PlayMusic(clip, loop);

        public void StopMusic() => _output.StopMusic();

        public void SetMusicVolume(float normalized)
        {
            ApplyMusicVolume(normalized);
            _settings.SetMusicVolume(normalized);
        }

        public void SetSfxVolume(float normalized)
        {
            ApplySfxVolume(normalized);
            _settings.SetSfxVolume(normalized);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => PlaySceneMusic(scene.name);

        private void PlaySceneMusic(string sceneName)
        {
            SceneDefinition definition = _catalog.FindBySceneName(sceneName);

            if (definition == null)
            {
                Debug.LogWarning($"[{nameof(AudioService)}] Scene '{sceneName}' is not in the catalog; the current music keeps playing.");
                return;
            }

            // A definition without a clip is deliberate silence, for example the bootstrap scene.
            if (definition.Music == null)
                return;

            _output.PlayMusic(definition.Music, true);
        }

        private void ApplyMusicVolume(float normalized)
        {
            MusicVolume = normalized;
            _output.SetVolume(MixerParams.Music, normalized);
            MusicVolumeChanged?.Invoke(normalized);
        }

        private void ApplySfxVolume(float normalized)
        {
            SfxVolume = normalized;
            _output.SetVolume(MixerParams.Sfx, normalized);
            SfxVolumeChanged?.Invoke(normalized);
        }
    }
}
