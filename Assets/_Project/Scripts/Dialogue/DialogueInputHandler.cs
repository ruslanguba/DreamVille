using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

using DreamVille.Core.Pause;

namespace DreamVille.Dialogue
{
    /// <summary>
    /// Advances the current dialogue sentence when the configured input action fires. Silent while the game is
    /// paused: the pause menu covers the dialogue, and a line must not advance or end behind it.
    /// </summary>
    public class DialogueInputHandler : MonoBehaviour
    {
        [SerializeField] private InputActionReference _nextAction;

        private InputAction _next;
        private IDialogueService _dialogue;
        private IPauseState _pause;

        [Inject]
        private void Construct(IDialogueService dialogue, IPauseState pause)
        {
            _dialogue = dialogue;
            _pause = pause;
        }

        private void Awake()
        {
            // Resolved once: InputActionReference is a ScriptableObject, so it is compared with Unity's null, never ?.
            _next = _nextAction == null ? null : _nextAction.action;
            if (_next == null)
                Debug.LogError($"[{nameof(DialogueInputHandler)}] '{name}': {nameof(_nextAction)} is not assigned; dialogue lines cannot be advanced from the keyboard.", this);
        }

        private void OnEnable()
        {
            if (_next == null)
                return;

            _next.Enable();
            _next.performed += OnNextPerformed;
        }

        private void OnDisable()
        {
            if (_next == null)
                return;

            _next.performed -= OnNextPerformed;
            _next.Disable();
        }

        private void OnNextPerformed(InputAction.CallbackContext _)
        {
            if (_pause.IsPaused || !_dialogue.IsActive)
                return;

            _dialogue.ShowNext();
        }
    }
}
