using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

using DreamVille.Dialogue;
using LegacyDialogue = DreamVille.Dialogue.Dialogue;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>
    /// Hand-written <see cref="IDialogueService"/> double: raises its events on demand, counts
    /// <see cref="ShowNext"/> calls and attached handlers, accepts a start while idle.
    /// </summary>
    public sealed class FakeDialogueService : IDialogueService
    {
        private Action<bool> _activeChanged;
        private Action<SpokenLine> _lineShown;
        private UniTaskCompletionSource _run;

        public event Action<bool> ActiveChanged
        {
            add { _activeChanged += value; HandlerCount++; }
            remove { Detach(ref _activeChanged, value); }
        }

        public event Action<SpokenLine> LineShown
        {
            add { _lineShown += value; HandlerCount++; }
            remove { Detach(ref _lineShown, value); }
        }

        public bool IsActive { get; private set; }

        /// <summary>Handlers attached to both events together.</summary>
        public int HandlerCount { get; private set; }

        public int ShowNextCount { get; private set; }

        public int StartCount { get; private set; }

        public DialogueDefinition LastStartedDefinition { get; private set; }

        public bool TryStart(DialogueDefinition dialogue, CancellationToken cancellationToken, out UniTask completion)
        {
            if (IsActive)
            {
                completion = default;
                return false;
            }

            StartCount++;
            LastStartedDefinition = dialogue;
            _run = new UniTaskCompletionSource();
            completion = _run.Task;
            RaiseActiveChanged(true);
            return true;
        }

        [Obsolete("Transitional until spec 11 B4")]
        public bool Begin(IReadOnlyList<LegacyDialogue> lines, Action onCompleted = null) => false;

        public void ShowNext() => ShowNextCount++;

        public void RaiseActiveChanged(bool isActive)
        {
            IsActive = isActive;
            _activeChanged?.Invoke(isActive);
        }

        public void RaiseLineShown(SpokenLine line) => _lineShown?.Invoke(line);

        /// <summary>Ends the run started by <see cref="TryStart"/> the way the real service does: event first, then completion.</summary>
        public void Finish()
        {
            RaiseActiveChanged(false);
            _run?.TrySetResult();
        }

        private void Detach<T>(ref Action<T> field, Action<T> handler)
        {
            Action<T> before = field;
            field -= handler;
            if (!ReferenceEquals(before, field))
                HandlerCount--;
        }
    }
}
