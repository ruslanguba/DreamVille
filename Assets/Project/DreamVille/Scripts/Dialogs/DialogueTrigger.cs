using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class DialogueTrigger : MonoBehaviour
{
    public event Action OnDialogueStarted;
    public event Action OnDialogueEnded;

    private PlayerDetectorTrigger _actionSender;
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

        if (selectedLocale == "en")
        {
            Debug.Log("Selected English locale");
            DialogueManager.Instance.StartDialog(sentenceEn, this);
        }

        else
        {
            Debug.Log("Selected Russian or another locale");
            DialogueManager.Instance.StartDialog(sentence, this);
        }
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
