using UnityEngine;

namespace DreamVille.Gameplay.Npc
{
    /// <summary>
    /// One speaker of an ambient two-NPC dialogue: shows the talk animation and the speech bubble together.
    /// Driven by <see cref="AmbientConversation"/>.
    /// </summary>
    public sealed class ConversationSpeaker : MonoBehaviour
    {
        [SerializeField] private GameObject _dialogCanvas;

        private Animator _animator;

        /// <summary>Whether this speaker currently has the word.</summary>
        public bool IsTalking { get; private set; }

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            if (_animator == null)
                Debug.LogError($"[{nameof(ConversationSpeaker)}] '{name}' has no Animator in its children.", this);

            if (_dialogCanvas == null)
                Debug.LogError($"[{nameof(ConversationSpeaker)}] '{name}' has no speech bubble assigned.", this);
            else
                _dialogCanvas.SetActive(false);
        }

        public void SetTalking(bool isTalking)
        {
            IsTalking = isTalking;

            if (_animator != null)
                _animator.SetBool(AnimParams.IsTalk, isTalking);

            if (_dialogCanvas != null)
                _dialogCanvas.SetActive(isTalking);
        }

        public void SwitchTalking() => SetTalking(!IsTalking);
    }
}
