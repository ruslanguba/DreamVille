using UnityEngine;
using Zenject;

using DreamVille.Audio;

namespace DreamVille.Gameplay.Interactables.Effects
{
    /// <summary>Plays a 2D sound effect through the audio service (a balloon pop). An empty clip is silent.</summary>
    public sealed class PlaySfxEffect : MonoBehaviour, IInteractionEffect
    {
        [SerializeField] private AudioClip _clip;

        private IAudioService _audio;

        [Inject]
        private void Construct(IAudioService audio)
        {
            _audio = audio;
        }

        public void Apply(in InteractionContext context)
        {
            _audio.PlaySfx(_clip);
        }
    }
}
