using System;
using System.Collections.Generic;

namespace DreamVille.Dialogue
{
    /// <summary>Drives a conversation: the sentence queue and the transitions between lines.</summary>
    public interface IDialogueService : IDialogueState
    {
        event Action<Dialogue> SentenceShown;

        /// <summary>
        /// Starts a conversation. A dialogue that is already running is not interrupted:
        /// the new request is ignored and false is returned.
        /// </summary>
        bool Begin(IReadOnlyList<Dialogue> lines, Action onCompleted = null);

        void ShowNext();
    }
}
