using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

using DreamVille.Dialogue;

namespace DreamVille.Dialogue
{
    /// <summary>Raises this Cinemachine camera above the gameplay camera while the parent DialogueTrigger's conversation runs, and drops it back when it ends.</summary>
    public class CameraPriorityHandler : MonoBehaviour
    {
        CinemachineCamera _cinCamera;
        DialogueTrigger _dialogueTrigger;

        private void Start()
        {
            _cinCamera = GetComponent<CinemachineCamera>();
            _cinCamera.Priority = -1;

            _dialogueTrigger = GetComponentInParent<DialogueTrigger>(true);

            if (_dialogueTrigger == null)
                return;

            _dialogueTrigger.DialogueStarted += SetCameraPriority;
            _dialogueTrigger.DialogueEnded += ResetCameraPriority;
        }

        private void OnDisable()
        {
            if (_dialogueTrigger == null)
                return;

            _dialogueTrigger.DialogueStarted -= SetCameraPriority;
            _dialogueTrigger.DialogueEnded -= ResetCameraPriority;
        }
        private void SetCameraPriority()
        {
            _cinCamera.Priority = 1;
        }

        private void ResetCameraPriority()
        {
            _cinCamera.Priority = -1;
        }
    }
}
