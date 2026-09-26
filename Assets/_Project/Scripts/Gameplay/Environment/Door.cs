using UnityEngine;

namespace DreamVille.Gameplay.Environment
{
    /// <summary>
    /// A one-way animated door: fires the Animator's <c>open</c> trigger once and plays its open sound
    /// from its own <see cref="AudioSource"/>, so no scene-level audio object is needed.
    /// </summary>
    [RequireComponent(typeof(Animator), typeof(AudioSource))]
    public sealed class Door : MonoBehaviour, IDoor
    {
        [Tooltip("Optional: a silent door is valid.")]
        [SerializeField] private AudioClip _openSound;

        private Animator _animator;
        private AudioSource _audio;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _audio = GetComponent<AudioSource>();
        }

        public void Open()
        {
            if (IsOpen)
                return;

            IsOpen = true;
            _animator.SetTrigger(AnimParams.Open);

            if (_openSound != null)
                _audio.PlayOneShot(_openSound);
        }
    }
}
