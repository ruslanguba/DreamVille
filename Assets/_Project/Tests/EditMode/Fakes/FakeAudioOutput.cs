using UnityEngine;

using DreamVille.Audio;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>
    /// Hand-written double for <see cref="IAudioOutput"/>: keeps the last music clip, the last SFX clip and
    /// the volume applied to each mixer channel.
    /// </summary>
    public sealed class FakeAudioOutput : IAudioOutput
    {
        public AudioClip LastMusicClip { get; private set; }

        public bool LastMusicLoop { get; private set; }

        public AudioClip LastSfxClip { get; private set; }

        public float MusicVolume { get; private set; }

        public float SfxVolume { get; private set; }

        public void PlayMusic(AudioClip clip, bool loop)
        {
            LastMusicClip = clip;
            LastMusicLoop = loop;
        }

        public void PlaySfx(AudioClip clip)
        {
            LastSfxClip = clip;
        }

        public void SetVolume(string mixerParameter, float normalized)
        {
            if (mixerParameter == MixerParams.Music)
                MusicVolume = normalized;
            else if (mixerParameter == MixerParams.Sfx)
                SfxVolume = normalized;
        }
    }
}
