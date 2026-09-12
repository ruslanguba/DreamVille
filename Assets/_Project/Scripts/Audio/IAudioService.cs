using UnityEngine;

namespace DreamVille.Audio
{
    /// <summary>Playback for gameplay code.</summary>
    public interface IAudioService
    {
        void PlaySfx(AudioClip clip);

        void PlayMusic(AudioClip clip, bool loop = true);

        void StopMusic();
    }
}
