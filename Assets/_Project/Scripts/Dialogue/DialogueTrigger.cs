using System.Collections.Generic;
using System;
using UnityEngine.Localization.Settings;
using Zenject;
using UnityEngine;

using DreamVille.Gameplay.Player;

namespace DreamVille.Dialogue
{
    public class DialogueTrigger : MonoBehaviour
    {
        public event Action OnDialogueStarted;
        public event Action OnDialogueEnded;

        private PlayerDetectorTrigger _actionSender;
        private DialogueService _dialogue;

        [Inject]
        public void Construct(DialogueService dialogue)
        {
            _dialogue = dialogue;
        }

        [SerializeField] private List<Dialogue> sentence;
        [SerializeField] private List<Dialogue> sentenceEn;

        [SerializeField] private AudioClip[] mumbels;
        [SerializeField] private bool deleteOnEnd;

        /// <summary>Разоружённый триггер игнорирует вход игрока. Объект при этом остаётся активным.</summary>
        public bool IsArmed { get; set; } = true;

        private void OnEnable()
        {
            // includeInactive обязателен: иначе после выключения зоны триггер теряет сам себя.
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

            OnDialogueStarted?.Invoke();

            var selectedLocale = LocalizationSettings.SelectedLocale.Identifier;

            var lines = selectedLocale == "en" ? sentenceEn : sentence;
            _dialogue.Begin(lines, DialogueCompleted);
        }

        public void DialogueCompleted()
        {
            OnDialogueEnded?.Invoke();  // Если события нет - ничего не произойдет

            if (deleteOnEnd)
            {
                Destroy(this);
            }
        }
    }
}
