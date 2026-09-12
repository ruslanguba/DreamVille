using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace DreamVille.Dialogue
{
    /// <summary>Advances the current dialogue sentence when the configured input action fires.</summary>
    public class DialogueInputHandler : MonoBehaviour
    {
        [SerializeField] private InputActionReference _nextAction;

        private IDialogueService _dialogue;

        [Inject]
        public void Construct(IDialogueService dialogue)
        {
            _dialogue = dialogue;
        }

        private void OnEnable()
        {
            if (_nextAction?.action == null)
            {
                Debug.LogWarning($"[{nameof(DialogueInputHandler)}] '{name}': {nameof(_nextAction)} is not assigned.", this);
                return;
            }

            _nextAction.action.Enable();
            _nextAction.action.performed += OnNextPerformed;
        }

        private void OnDisable()
        {
            if (_nextAction?.action == null)
                return;

            _nextAction.action.performed -= OnNextPerformed;
            _nextAction.action.Disable();
        }

        private void OnNextPerformed(InputAction.CallbackContext _)
        {
            if (_dialogue is { IsActive: true })
                _dialogue.ShowNext();
        }
    }
}
