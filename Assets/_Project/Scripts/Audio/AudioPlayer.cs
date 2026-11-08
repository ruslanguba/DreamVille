using UnityEngine;
using UnityEngine.Audio;

namespace DreamVille.Audio
{
    /// <summary>
    /// Engine adapter for audio: one music source, one SFX source and the mixer.
    /// Lives on a project-scoped prefab, so no DontDestroyOnLoad or duplicate guard is needed.
    /// </summary>
    public sealed class AudioPlayer : MonoBehaviour, IAudioOutput
    {
        /// <summary>
        /// Floor for the linear-to-decibel conversion: log10(0) is minus infinity, and 0.001 maps
        /// to -60 dB, which is silence for the mixer's purposes.
        /// </summary>
        private const float MinNormalizedVolume = 0.001f;

        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer _audioMixer;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;

        public void PlayMusic(AudioClip clip, bool loop)
        {
            if (clip == null)
                return;

            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.Play();
        }

        public void PlaySfx(AudioClip clip)
        {
            if (clip == null)
                return;

            _sfxSource.PlayOneShot(clip);
        }

        public void SetVolume(string mixerParameter, float normalized)
        {
            // 20*log10: amplitude ratio to decibels
            float decibels = Mathf.Log10(Mathf.Clamp(normalized, MinNormalizedVolume, 1f)) * 20f;
            _audioMixer.SetFloat(mixerParameter, decibels);
        }
    }
}
