using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

using DreamVille.Core.Events;
using DreamVille.Core.Players;

namespace DreamVille.Dialogue
{
    /// <summary>
    /// Starts a dialogue when the player enters its trigger zone. Each listener of its events runs on its own:
    /// one that throws is logged and the others still run, so listener order is not part of the contract.
    /// </summary>
    public sealed class DialogueTrigger : MonoBehaviour, IDialogueTrigger
    {
        /// <summary>Raised after the dialogue service accepted this trigger's lines.</summary>
        public event Action DialogueStarted;

        /// <summary>Raised when the last line of this trigger's dialogue has been dismissed.</summary>
        public event Action DialogueEnded;

        [SerializeField] private DialogueDefinition _definition;

        [SerializeField] private bool _deleteOnEnd;

        private IPlayerZone _playerZone;
        private IDialogueService _service;

        [Inject]
        internal void Construct(IDialogueService service)
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
                _playerZone.PlayerEntered += OnPlayerEntered;
        }

        private void OnDisable()
        {
            if (_playerZone != null)
                _playerZone.PlayerEntered -= OnPlayerEntered;
        }

        private void OnPlayerEntered()
        {
            StartDialogue();
        }

        /// <summary>Plays this trigger's dialogue unless the trigger is disarmed or the service is busy.</summary>
        public void StartDialogue()
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

        // Everything before the first await runs inside StartDialogue, so DialogueStarted
        // is raised in the very call in which the service accepted the run.
        private async UniTaskVoid PlayAsync(CancellationToken destroyToken)
        {
            if (!_service.TryStart(_definition, destroyToken, out UniTask completion))
                return;

            // A throwing listener must not cost the conversation its end: the portal and rewards wait for DialogueEnded.
            IsolatedEvent.Raise(DialogueStarted, this);

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

            IsolatedEvent.Raise(DialogueEnded, this);

            if (_deleteOnEnd)
                Destroy(this);
        }
    }
}
