namespace DreamVille.Persistence
{
    /// <summary>Default <see cref="IAudioSettingsRepository"/> backed by an <see cref="ISaveStorage"/>.</summary>
    public sealed class AudioSettingsRepository : IAudioSettingsRepository
    {
        private readonly ISaveStorage _storage;

        public AudioSettingsRepository(ISaveStorage storage)
        {
            _storage = storage;
        }

        public bool TryGetMusicVolume(out float volume)
        {
            if (!_storage.HasKey(SaveKeys.MusicVolume))
            {
                volume = default;
                return false;
            }

            volume = _storage.GetFloat(SaveKeys.MusicVolume);
            return true;
        }

        public bool TryGetSfxVolume(out float volume)
        {
            if (!_storage.HasKey(SaveKeys.SfxVolume))
            {
                volume = default;
                return false;
            }

            volume = _storage.GetFloat(SaveKeys.SfxVolume);
            return true;
        }

        public void SetMusicVolume(float volume) => _storage.SetFloat(SaveKeys.MusicVolume, volume);

        public void SetSfxVolume(float volume) => _storage.SetFloat(SaveKeys.SfxVolume, volume);

        public void Flush() => _storage.Save();
    }
}
