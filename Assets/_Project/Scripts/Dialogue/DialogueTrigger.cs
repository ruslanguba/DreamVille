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

        private void OnEnable()
        {
            _actionSender = GetComponentInChildren<PlayerDetectorTrigger>();
            _actionSender.OnPlayerTriggered += TriggerDialog;
        }

        private void OnDisable()
        {
            _actionSender.OnPlayerTriggered -= TriggerDialog;
        }

        public void TriggerDialog()
        {
            OnDialogueStarted?.Invoke();

            var selectedLocale = LocalizationSettings.SelectedLocale.Identifier;

            var lines = selectedLocale == "en" ? sentenceEn : sentence;
            _dialogue.Begin(lines, DialogueCompleted);
        }

        public void EnableTrigger()
        {
            _actionSender.gameObject.SetActive(true);
        }

        public void DisableTrigger()
        {
            _actionSender.gameObject.SetActive(false);
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
