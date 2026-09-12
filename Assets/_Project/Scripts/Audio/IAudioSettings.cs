using System;

namespace DreamVille.Audio
{
    /// <summary>Volume settings for the options screen. Values are normalized to 0..1.</summary>
    public interface IAudioSettings
    {
        float MusicVolume { get; }

        float SfxVolume { get; }

        event Action<float> MusicVolumeChanged;

        event Action<float> SfxVolumeChanged;

        void SetMusicVolume(float normalized);

        void SetSfxVolume(float normalized);
    }
}
