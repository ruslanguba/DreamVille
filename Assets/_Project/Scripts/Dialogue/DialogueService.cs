using System;
using System.Collections.Generic;

namespace DreamVille.Dialogue
{
    /// <summary>
    /// Dialogue state: the sentence queue and the transitions between lines.
    /// Knows nothing about UI, input or the scene.
    /// </summary>
    public class DialogueService : IDialogueService
    {
        public event Action<bool> ActiveChanged;
        public event Action<Dialogue> SentenceShown;

        private readonly Queue<Dialogue> _sentences = new Queue<Dialogue>();
        private Action _onCompleted;

        public bool IsActive { get; private set; }

        public bool Begin(IReadOnlyList<Dialogue> lines, Action onCompleted = null)
        {
            if (IsActive || lines == null || lines.Count == 0)
                return false;

            _sentences.Clear();
            foreach (Dialogue line in lines)
                _sentences.Enqueue(line);

            _onCompleted = onCompleted;

            IsActive = true;
            ActiveChanged?.Invoke(true);

            ShowNext();

            return true;
        }

        public void ShowNext()
        {
            if (!IsActive)
                return;

            if (_sentences.Count == 0)
            {
                End();
                return;
            }

            // Dequeue outside the null-conditional call: the queue must advance even when
            // nobody listens, otherwise the dialogue could never end.
            Dialogue next = _sentences.Dequeue();
            SentenceShown?.Invoke(next);
        }

        private void End()
        {
            IsActive = false;
            ActiveChanged?.Invoke(false);

            Action completed = _onCompleted;
            _onCompleted = null;
            completed?.Invoke();
        }
    }
}
