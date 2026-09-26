using System;

using DreamVille.Dialogue;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Hand-written <see cref="IDialogueTrigger"/> double: raises its events on demand, counts attached handlers.</summary>
    public sealed class FakeDialogueTrigger : IDialogueTrigger
    {
        private Action _started;
        private Action _ended;

        public event Action DialogueStarted
        {
            add => Attach(ref _started, value);
            remove => Detach(ref _started, value);
        }

        public event Action DialogueEnded
        {
            add => Attach(ref _ended, value);
            remove => Detach(ref _ended, value);
        }

        public bool IsArmed { get; set; } = true;

        /// <summary>Handlers attached to both events together.</summary>
        public int HandlerCount { get; private set; }

        public void RaiseStarted() => _started?.Invoke();

        public void RaiseEnded() => _ended?.Invoke();

        private void Attach(ref Action field, Action handler)
        {
            field += handler;
            HandlerCount++;
        }

        private void Detach(ref Action field, Action handler)
        {
            Action before = field;
            field -= handler;
            if (!ReferenceEquals(before, field))
                HandlerCount--;
        }
    }
}
