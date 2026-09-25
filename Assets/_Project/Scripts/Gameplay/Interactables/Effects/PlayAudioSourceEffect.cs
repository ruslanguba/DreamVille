using UnityEngine;

namespace DreamVille.Gameplay.Interactables.Effects
{
    /// <summary>Plays a 3D <see cref="AudioSource"/> placed on the object (a lamp click, a rocket engine).</summary>
    public sealed class PlayAudioSourceEffect : MonoBehaviour, IInteractionEffect
    {
        [SerializeField] private AudioSource _source;

        private void Reset()
        {
            _source = GetComponent<AudioSource>();
        }

        private void Awake()
        {
            if (_source == null)
                Debug.LogError($"[{nameof(PlayAudioSourceEffect)}] Audio source is not assigned.", this);
        }

        public void Apply(in InteractionContext context)
        {
            _source.Play();
        }
    }
}
