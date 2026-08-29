using System;
using System.Collections.Generic;

namespace DreamVille.Dialogue
{
    /// <summary>
    /// Состояние диалога: очередь реплик и переходы между ними.
    /// Не знает ни про UI, ни про ввод, ни про сцену.
    /// </summary>
    public class DialogueService
    {
        public event Action<bool> ActiveChanged;
        public event Action<Dialogue> SentenceShown;

        private readonly Queue<Dialogue> _sentences = new Queue<Dialogue>();
        private Action _onCompleted;

        public bool IsActive { get; private set; }

        public void Begin(IReadOnlyList<Dialogue> lines, Action onCompleted = null)
        {
            if (lines == null || lines.Count == 0)
                return;

            _sentences.Clear();
            foreach (Dialogue line in lines)
                _sentences.Enqueue(line);

            _onCompleted = onCompleted;

            IsActive = true;
            ActiveChanged?.Invoke(true);

            ShowNext();
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

            SentenceShown?.Invoke(_sentences.Dequeue());
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
