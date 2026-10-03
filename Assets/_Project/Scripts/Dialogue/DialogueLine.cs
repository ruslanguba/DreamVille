using System;
using UnityEngine;
using UnityEngine.Localization;

namespace DreamVille.Dialogue
{
    /// <summary>One line of a conversation: who speaks and a reference to the localized text.</summary>
    [Serializable]
    public struct DialogueLine
    {
        [SerializeField] private DialogueSpeaker _speaker;
        [SerializeField] private LocalizedString _text;

        public DialogueLine(DialogueSpeaker speaker, LocalizedString text)
        {
            _speaker = speaker;
            _text = text;
        }

        public DialogueSpeaker Speaker => _speaker;

        public LocalizedString Text => _text;
    }
}
