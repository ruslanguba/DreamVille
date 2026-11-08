using UnityEngine;

namespace DreamVille.Gameplay.Npc
{
    /// <summary>
    /// One speaker of an ambient two-NPC dialogue: shows the talk animation and the speech bubble together.
    /// Driven by <see cref="AmbientConversation"/>.
    /// </summary>
    public sealed class ConversationSpeaker : MonoBehaviour
    {
        [SerializeField] private GameObject _speechBubble;

        private Animator _animator;

        /// <summary>Whether this speaker currently has the word.</summary>
        public bool IsTalking { get; private set; }

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            if (_animator == null)
                Debug.LogError($"[{nameof(ConversationSpeaker)}] '{name}' has no Animator in its children.", this);

            if (_speechBubble == null)
                Debug.LogError($"[{nameof(ConversationSpeaker)}] '{name}' has no speech bubble assigned.", this);
            else
                _speechBubble.SetActive(false);
        }

        public void SetTalking(bool isTalking)
        {
            IsTalking = isTalking;

            if (_animator != null)
                _animator.SetBool(AnimParams.IsTalk, isTalking);

            if (_speechBubble != null)
                _speechBubble.SetActive(isTalking);
        }

        public void SwitchTalking() => SetTalking(!IsTalking);
    }
}
