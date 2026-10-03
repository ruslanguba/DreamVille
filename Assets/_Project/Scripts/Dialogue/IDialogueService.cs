using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace DreamVille.Dialogue
{
    /// <summary>Drives a conversation: the line queue, text resolution and the transitions between lines.</summary>
    public interface IDialogueService : IDialogueState
    {
        /// <summary>Raised for every line, the first one included, after its text was resolved.</summary>
        event Action<SpokenLine> LineShown;

        /// <summary>
        /// Starts a conversation if none is running. Acceptance is synchronous: on true, IsActive is already true and
        /// ActiveChanged(true) has been raised; on false nothing changed. <paramref name="completion"/> finishes after the
        /// last line was dismissed and ActiveChanged(false) was raised; it is cancelled (OperationCanceledException) when
        /// <paramref name="cancellationToken"/> fires, in which case the service also ends the run and raises ActiveChanged(false).
        /// </summary>
        /// <remarks>
        /// An already cancelled token is rejected like a running conversation. A text that cannot be resolved is logged
        /// and the run ends with no line shown; <paramref name="completion"/> then finishes normally, so whatever waits
        /// for the end of the conversation (a portal, a reward) still happens.
        /// </remarks>
        bool TryStart(DialogueDefinition dialogue, CancellationToken cancellationToken, out UniTask completion);

        /// <summary>Dismisses the current line; ignored when idle or while texts are still resolving.</summary>
        void ShowNext();
    }
}
