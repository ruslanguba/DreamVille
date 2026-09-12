namespace DreamVille.Persistence
{
    /// <summary>
    /// Persisted audio settings. Getters report absence instead of guessing a default,
    /// because the default lives in AudioConfig, not in the save file.
    /// </summary>
    public interface IAudioSettingsRepository
    {
        bool TryGetMusicVolume(out float volume);

        bool TryGetSfxVolume(out float volume);

        void SetMusicVolume(float volume);

        void SetSfxVolume(float volume);

        /// <summary>Writes pending values to disk. Sliders call the setters on every frame of a drag.</summary>
        void Flush();
    }
}
