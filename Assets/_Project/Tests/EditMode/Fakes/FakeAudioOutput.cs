using System.Collections.Generic;
using UnityEngine;

using DreamVille.Audio;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>
    /// Hand-written double for <see cref="IAudioOutput"/>. Records every call in the order it
    /// happened, plus the last value of each kind, so a test can assert either the sequence or
    /// just the outcome.
    /// </summary>
    public sealed class FakeAudioOutput : IAudioOutput
    {
        public readonly struct Call
        {
            public string Method { get; }
            public AudioClip Clip { get; }
            public bool Loop { get; }
            public string MixerParameter { get; }
            public float Normalized { get; }

            public Call(string method, AudioClip clip = null, bool loop = false, string mixerParameter = null, float normalized = 0f)
            {
                Method = method;
                Clip = clip;
                Loop = loop;
                MixerParameter = mixerParameter;
                Normalized = normalized;
            }
        }

        private readonly List<Call> _calls = new List<Call>();

        public IReadOnlyList<Call> Calls => _calls;

        public AudioClip LastMusicClip { get; private set; }

        public bool LastMusicLoop { get; private set; }

        public int StopMusicCallCount { get; private set; }

        public AudioClip LastSfxClip { get; private set; }

        public string LastMixerParameter { get; private set; }

        public float LastVolume { get; private set; }

        public float MusicVolume { get; private set; }

        public float SfxVolume { get; private set; }

        public void PlayMusic(AudioClip clip, bool loop)
        {
            LastMusicClip = clip;
            LastMusicLoop = loop;
            _calls.Add(new Call(nameof(PlayMusic), clip: clip, loop: loop));
        }

        public void StopMusic()
        {
            StopMusicCallCount++;
            _calls.Add(new Call(nameof(StopMusic)));
        }

        public void PlaySfx(AudioClip clip)
        {
            LastSfxClip = clip;
            _calls.Add(new Call(nameof(PlaySfx), clip: clip));
        }

        public void SetVolume(string mixerParameter, float normalized)
        {
            LastMixerParameter = mixerParameter;
            LastVolume = normalized;

            if (mixerParameter == MixerParams.Music)
                MusicVolume = normalized;
            else if (mixerParameter == MixerParams.Sfx)
                SfxVolume = normalized;

            _calls.Add(new Call(nameof(SetVolume), mixerParameter: mixerParameter, normalized: normalized));
        }
    }
}
