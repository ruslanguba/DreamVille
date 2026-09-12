using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

namespace DreamVille.Audio
{
    /// <summary>
    /// Engine adapter for audio: one music source, one SFX source and the mixer.
    /// Lives on a project-scoped prefab, so no DontDestroyOnLoad or duplicate guard is needed.
    /// </summary>
    public sealed class AudioPlayer : MonoBehaviour, IAudioOutput
    {
        /// <summary>Silence in the mixer's decibel scale is minus infinity; clamp just above it.</summary>
        private const float MinNormalizedVolume = 0.001f;

        [Header("Audio Mixer")]
        [FormerlySerializedAs("audioMixer")]
        [SerializeField] private AudioMixer _audioMixer;

        [Header("Audio Sources")]
        [FormerlySerializedAs("musicSource")]
        [SerializeField] private AudioSource _musicSource;
        [FormerlySerializedAs("sfxSource")]
        [SerializeField] private AudioSource _sfxSource;

        public void PlayMusic(AudioClip clip, bool loop)
        {
            if (clip == null)
                return;

            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            _musicSource.Stop();
        }

        public void PlaySfx(AudioClip clip)
        {
            if (clip == null)
                return;

            _sfxSource.PlayOneShot(clip);
        }

        public void SetVolume(string mixerParameter, float normalized)
        {
            float decibels = Mathf.Log10(Mathf.Clamp(normalized, MinNormalizedVolume, 1f)) * 20f;
            _audioMixer.SetFloat(mixerParameter, decibels);
        }
    }
}
