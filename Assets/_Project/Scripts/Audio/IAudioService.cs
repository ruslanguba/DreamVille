using UnityEngine;

namespace DreamVille.Audio
{
    /// <summary>Playback for gameplay code.</summary>
    public interface IAudioService
    {
        void PlaySfx(AudioClip clip);
    }
}
