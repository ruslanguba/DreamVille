using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

using DreamVille.Core.Players;

namespace DreamVille.Dialogue
{
    /// <summary>Starts a dialogue when the player enters its trigger zone.</summary>
    public class DialogueTrigger : MonoBehaviour, IDialogueTrigger
    {
        /// <summary>Raised after the dialogue service accepted this trigger's lines.</summary>
        public event Action DialogueStarted;

        /// <summary>Raised when the last line of this trigger's dialogue has been dismissed.</summary>
        public event Action DialogueEnded;

        [SerializeField] private DialogueDefinition _definition;

        [FormerlySerializedAs("deleteOnEnd")]
        [SerializeField] private bool _deleteOnEnd;

        private IPlayerZone _playerZone;
        private IDialogueService _service;

        [Inject]
        public void Construct(IDialogueService service)
        {
            _service = service;
        }

        /// <summary>A disarmed trigger ignores the player. The object stays active.</summary>
        public bool IsArmed { get; set; } = true;

        /// <summary>The conversation this trigger plays.</summary>
        public DialogueDefinition Definition => _definition;

        private void OnEnable()
        {
            // includeInactive is required: otherwise the trigger loses itself once the zone is disabled.
            _playerZone = GetComponentInChildren<IPlayerZone>(true);

            if (_playerZone != null)
                _playerZone.PlayerEntered += TriggerDialog;
        }

        private void OnDisable()
        {
            if (_playerZone != null)
                _playerZone.PlayerEntered -= TriggerDialog;
        }

        public void TriggerDialog()
        {
            if (!IsArmed)
                return;

            if (_definition == null)
            {
                Debug.LogError($"[{nameof(DialogueTrigger)}] '{name}' has no dialogue definition assigned.", this);
                return;
            }

            if (_definition.Lines.Count == 0)
            {
                Debug.LogError($"[{nameof(DialogueTrigger)}] '{name}': dialogue '{_definition.name}' has no lines.", this);
                return;
            }

            if (_service.IsActive)
            {
                Debug.LogWarning($"[{nameof(DialogueTrigger)}] '{name}': a dialogue is already active, ignoring the trigger.", this);
                return;
            }

            PlayAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        // Everything before the first await runs inside TriggerDialog, so DialogueStarted
        // is raised in the very call in which the service accepted the run.
        private async UniTaskVoid PlayAsync(CancellationToken destroyToken)
        {
            if (!_service.TryStart(_definition, destroyToken, out UniTask completion))
                return;

            // A throwing listener must not cost the conversation its end: the portal and rewards wait for DialogueEnded.
            Raise(DialogueStarted);

            try
            {
                await completion;
            }
            catch (OperationCanceledException)
            {
                // Destroyed (or the scene unloaded) mid-dialogue; the service has already ended the run.
                return;
            }
            catch (Exception exception)
            {
                // A line listener failed; the service has still ended the run.
                Debug.LogException(exception, this);
            }

            Raise(DialogueEnded);

            if (_deleteOnEnd)
                Destroy(this);
        }

        private void Raise(Action dialogueEvent)
        {
            try
            {
                dialogueEvent?.Invoke();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }
}
