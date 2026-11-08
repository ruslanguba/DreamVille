using Unity.Cinemachine;
using UnityEngine;

namespace DreamVille.Dialogue
{
    /// <summary>Raises this Cinemachine camera above the gameplay camera while the parent DialogueTrigger's conversation runs, and drops it back when it ends.</summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public sealed class CameraPriorityHandler : MonoBehaviour
    {
        // The player camera keeps Cinemachine's default 0 (its priority is disabled), so 1 wins and -1 loses.
        private const int DialoguePriority = 1;
        private const int IdlePriority = -1;

        private CinemachineCamera _camera;
        private DialogueTrigger _dialogueTrigger;

        private void Awake()
        {
            _camera = GetComponent<CinemachineCamera>();
            _camera.Priority = IdlePriority;

            _dialogueTrigger = GetComponentInParent<DialogueTrigger>(true);

            if (_dialogueTrigger == null)
                Debug.LogError($"[{nameof(CameraPriorityHandler)}] No {nameof(DialogueTrigger)} in the parents of '{name}'; this camera never takes over.", this);
        }

        private void OnEnable()
        {
            // Also null once the trigger has destroyed itself after its dialogue (deleteOnEnd).
            if (_dialogueTrigger == null)
                return;

            _dialogueTrigger.DialogueStarted += OnDialogueStarted;
            _dialogueTrigger.DialogueEnded += OnDialogueEnded;
        }

        private void OnDisable()
        {
            // Disabled mid-dialogue, the camera would miss DialogueEnded and come back on top.
            _camera.Priority = IdlePriority;

            if (_dialogueTrigger == null)
                return;

            _dialogueTrigger.DialogueStarted -= OnDialogueStarted;
            _dialogueTrigger.DialogueEnded -= OnDialogueEnded;
        }

        private void OnDialogueStarted() => _camera.Priority = DialoguePriority;

        private void OnDialogueEnded() => _camera.Priority = IdlePriority;
    }
}
