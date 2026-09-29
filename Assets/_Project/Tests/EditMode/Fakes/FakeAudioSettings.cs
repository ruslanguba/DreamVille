using System;
using System.Collections.Generic;

using DreamVille.Audio;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>
    /// Volume settings set by the test. Records every <c>Set*</c> call; the change events are raised
    /// only through <see cref="ChangeMusicVolumeElsewhere"/> and <see cref="ChangeSfxVolumeElsewhere"/>.
    /// </summary>
    public sealed class FakeAudioSettings : IAudioSettings
    {
        private readonly List<float> _musicWrites = new List<float>();
        private readonly List<float> _sfxWrites = new List<float>();

        public event Action<float> MusicVolumeChanged;
        public event Action<float> SfxVolumeChanged;

        public float MusicVolume { get; set; }

        public float SfxVolume { get; set; }

        public IReadOnlyList<float> MusicWrites => _musicWrites;

        public IReadOnlyList<float> SfxWrites => _sfxWrites;

        public void SetMusicVolume(float normalized)
        {
            _musicWrites.Add(normalized);
            MusicVolume = normalized;
        }

        public void SetSfxVolume(float normalized)
        {
            _sfxWrites.Add(normalized);
            SfxVolume = normalized;
        }

        public void ChangeMusicVolumeElsewhere(float normalized)
        {
            MusicVolume = normalized;
            MusicVolumeChanged?.Invoke(normalized);
        }

        public void ChangeSfxVolumeElsewhere(float normalized)
        {
            SfxVolume = normalized;
            SfxVolumeChanged?.Invoke(normalized);
        }
    }
}
