using System;
using Zenject;

using DreamVille.Audio;

namespace DreamVille.UI.Menus
{
    /// <summary>
    /// Two-way binding between the volume sliders and <see cref="IAudioSettings"/>, without echo:
    /// the view is updated through its no-notify setters, so a settings change never writes itself back.
    /// </summary>
    public sealed class AudioSettingsPresenter : IInitializable, IDisposable
    {
        private readonly IAudioSettingsView _view;
        private readonly IAudioSettings _settings;

        public AudioSettingsPresenter(IAudioSettingsView view, IAudioSettings settings)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public void Initialize()
        {
            _view.ShowMusicVolume(_settings.MusicVolume);
            _view.ShowSfxVolume(_settings.SfxVolume);

            _view.MusicVolumeEdited += OnMusicVolumeEdited;
            _view.SfxVolumeEdited += OnSfxVolumeEdited;

            // Keeps the sliders honest when the volume is changed elsewhere (the other options screen, a reset).
            _settings.MusicVolumeChanged += OnMusicVolumeChanged;
            _settings.SfxVolumeChanged += OnSfxVolumeChanged;
        }

        /// <summary>Unsubscribes only; never calls the view, which may already be destroyed with the scene.</summary>
        public void Dispose()
        {
            _view.MusicVolumeEdited -= OnMusicVolumeEdited;
            _view.SfxVolumeEdited -= OnSfxVolumeEdited;

            _settings.MusicVolumeChanged -= OnMusicVolumeChanged;
            _settings.SfxVolumeChanged -= OnSfxVolumeChanged;
        }

        private void OnMusicVolumeEdited(float normalized)
        {
            _settings.SetMusicVolume(normalized);
        }

        private void OnSfxVolumeEdited(float normalized)
        {
            _settings.SetSfxVolume(normalized);
        }

        private void OnMusicVolumeChanged(float normalized)
        {
            _view.ShowMusicVolume(normalized);
        }

        private void OnSfxVolumeChanged(float normalized)
        {
            _view.ShowSfxVolume(normalized);
        }
    }
}
