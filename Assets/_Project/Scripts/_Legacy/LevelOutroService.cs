using System;

namespace DreamVille.Legacy
{
    /// <summary>
    /// Финал уровня как последовательность фактов. Ничего не делает сам —
    /// сообщает, что произошло, и каждый участник реагирует самостоятельно.
    /// </summary>
    public class LevelOutroService
    {
        /// <summary>Все враги побеждены: финальная зона открывается.</summary>
        public event Action Started;

        /// <summary>Игрок дошёл до зоны, начался финальный диалог.</summary>
        public event Action DialogueStarted;

        /// <summary>Диалог закончен: можно открывать проход дальше.</summary>
        public event Action Finished;

        public bool IsStarted { get; private set; }
        public bool IsFinished { get; private set; }

        public void Begin()
        {
            if (IsStarted)
                return;

            IsStarted = true;
            Started?.Invoke();
        }

        public void BeginDialogue()
        {
            if (!IsStarted || IsFinished)
                return;

            DialogueStarted?.Invoke();
        }

        public void Finish()
        {
            if (!IsStarted || IsFinished)
                return;

            IsFinished = true;
            Finished?.Invoke();
        }
    }
}
