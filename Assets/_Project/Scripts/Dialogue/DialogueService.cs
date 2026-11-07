using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

using DreamVille.Core.Events;

namespace DreamVille.Dialogue
{
    /// <summary>
    /// Dialogue state: text resolution, the line order and the transitions between lines.
    /// Knows nothing about UI, input or the scene.
    /// </summary>
    /// <remarks>
    /// All state changes and events are raised synchronously inside <see cref="TryStart"/> and
    /// <see cref="ShowNext"/> whenever the text provider completes synchronously (preloaded tables).
    /// A text that cannot be resolved is logged and ends the run like a dismissed last line.
    /// </remarks>
    public class DialogueService : IDialogueService
    {
        public event Action<bool> ActiveChanged;
        public event Action<SpokenLine> LineShown;

        private readonly ILocalizedTextProvider _textProvider;

        // Non-null only while a line is on screen: ShowNext has nothing to dismiss when idle or while texts resolve.
        private UniTaskCompletionSource _lineDismissed;

        public DialogueService(ILocalizedTextProvider textProvider)
        {
            _textProvider = textProvider;
        }

        public bool IsActive { get; private set; }

        public bool TryStart(DialogueDefinition dialogue, CancellationToken cancellationToken, out UniTask completion)
        {
            if (IsActive || dialogue == null || dialogue.Lines.Count == 0 || cancellationToken.IsCancellationRequested)
            {
                completion = default;
                return false;
            }

            Activate();
            completion = PlayAsync(dialogue.Lines, cancellationToken);
            return true;
        }

        public void ShowNext()
        {
            _lineDismissed?.TrySetResult();
        }

        private void Activate()
        {
            IsActive = true;
            // Isolated: a throwing listener must not leave the service active with no run to end it.
            IsolatedEvent.Raise(ActiveChanged, true);
        }

        private async UniTask PlayAsync(IReadOnlyList<DialogueLine> lines, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                SpokenLine[] spokenLines;
                try
                {
                    spokenLines = await ResolveAsync(lines, cancellationToken);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    Debug.LogError($"[{nameof(DialogueService)}] Could not resolve the dialogue text, ending the dialogue: {exception}");
                    return;
                }

                await ShowAsync(spokenLines, cancellationToken);
            }
            finally
            {
                End();
            }
        }

        private async UniTask<SpokenLine[]> ResolveAsync(IReadOnlyList<DialogueLine> lines, CancellationToken cancellationToken)
        {
            var spokenLines = new SpokenLine[lines.Count];

            for (int i = 0; i < spokenLines.Length; i++)
            {
                DialogueLine line = lines[i];

                // Attached here so a provider that ignores the token cannot keep a cancelled run alive.
                string text = await _textProvider.GetTextAsync(line.Text, cancellationToken)
                    .AttachExternalCancellation(cancellationToken);

                spokenLines[i] = new SpokenLine(line.Speaker, text);
            }

            return spokenLines;
        }

        private async UniTask ShowAsync(SpokenLine[] lines, CancellationToken cancellationToken)
        {
            foreach (SpokenLine line in lines)
            {
                // Created before the event: a listener may dismiss the line it has just received.
                _lineDismissed = new UniTaskCompletionSource();
                LineShown?.Invoke(line);

                await _lineDismissed.Task.AttachExternalCancellation(cancellationToken);
            }
        }

        private void End()
        {
            // The field is cleared before the event: a listener may start the next conversation from it.
            _lineDismissed = null;
            IsActive = false;
            IsolatedEvent.Raise(ActiveChanged, false);
        }
    }
}
