using System.Collections.Generic;
using System;
using UnityEngine.Localization.Settings;
using UnityEngine.Serialization;
using Zenject;
using UnityEngine;

using DreamVille.Gameplay.Player;

namespace DreamVille.Dialogue
{
    /// <summary>Starts a dialogue when the player enters its trigger zone.</summary>
    public class DialogueTrigger : MonoBehaviour
    {
        /// <summary>Raised after the dialogue service accepted this trigger's lines.</summary>
        public event Action DialogueStarted;

        /// <summary>Raised when the last line of this trigger's dialogue has been dismissed.</summary>
        public event Action DialogueEnded;

        private PlayerDetectorTrigger _actionSender;
        private DialogueService _dialogue;

        [Inject]
        public void Construct(DialogueService dialogue)
        {
            _dialogue = dialogue;
        }

        [FormerlySerializedAs("sentence")]
        [SerializeField] private List<Dialogue> _sentencesRu;
        [FormerlySerializedAs("sentenceEn")]
        [SerializeField] private List<Dialogue> _sentencesEn;

        [FormerlySerializedAs("deleteOnEnd")]
        [SerializeField] private bool _deleteOnEnd;

        /// <summary>A disarmed trigger ignores the player. The object stays active.</summary>
        public bool IsArmed { get; set; } = true;

        private void OnEnable()
        {
            // includeInactive is required: otherwise the trigger loses itself once the zone is disabled.
            _actionSender = GetComponentInChildren<PlayerDetectorTrigger>(true);

            if (_actionSender != null)
                _actionSender.OnPlayerTriggered += TriggerDialog;
        }

        private void OnDisable()
        {
            if (_actionSender != null)
                _actionSender.OnPlayerTriggered -= TriggerDialog;
        }

        public void TriggerDialog()
        {
            if (!IsArmed)
                return;

            var selectedLocale = LocalizationSettings.SelectedLocale.Identifier;
            var lines = selectedLocale == "en" ? _sentencesEn : _sentencesRu;

            if (lines == null || lines.Count == 0)
            {
                Debug.LogError($"[{nameof(DialogueTrigger)}] '{name}': no dialogue lines for locale '{selectedLocale}'.", this);
                return;
            }

            if (_dialogue.IsActive)
            {
                Debug.LogWarning($"[{nameof(DialogueTrigger)}] '{name}': a dialogue is already active, ignoring the trigger.", this);
                return;
            }

            if (!_dialogue.Begin(lines, DialogueCompleted))
                return;

            DialogueStarted?.Invoke();
        }

        public void DialogueCompleted()
        {
            DialogueEnded?.Invoke();

            if (_deleteOnEnd)
            {
                Destroy(this);
            }
        }
    }
}
