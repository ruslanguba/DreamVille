using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace DreamVille.Dialogue
{
    /// <summary>Промотка диалога. Раньше это жил Update с Input.GetKeyDown.</summary>
    public class DialogueInputHandler : MonoBehaviour
    {
        [SerializeField] private InputActionReference _nextAction;

        private DialogueService _dialogue;

        [Inject]
        public void Construct(DialogueService dialogue)
        {
            _dialogue = dialogue;
        }

        private void OnEnable()
        {
            if (_nextAction?.action == null)
            {
                Debug.LogWarning($"{nameof(DialogueInputHandler)} на '{name}': не назначен {nameof(_nextAction)}.", this);
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
