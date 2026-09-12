using UnityEngine;

namespace DreamVille.Audio
{
    /// <summary>Engine boundary: AudioSources and the AudioMixer. Test doubles replace this.</summary>
    public interface IAudioOutput
    {
        void PlayMusic(AudioClip clip, bool loop);

        void StopMusic();

        void PlaySfx(AudioClip clip);

        /// <summary>Applies a 0..1 value to a mixer parameter as decibels.</summary>
        void SetVolume(string mixerParameter, float normalized);
    }
}
