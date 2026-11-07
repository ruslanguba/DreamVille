using System;
using System.Collections.Generic;

using DreamVille.Progression;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>
    /// Hand-written <see cref="IFinalZone"/> double: records Arm and OpenPortal into a log that can be shared with
    /// another fake, to pin cross-object order, and raises its dialogue events on demand.
    /// </summary>
    public sealed class FakeFinalZone : IFinalZone
    {
        private readonly List<string> _log;

        public FakeFinalZone(List<string> log = null)
        {
            _log = log ?? new List<string>();
        }

        public event Action DialogueStarted;
        public event Action DialogueEnded;

        public IReadOnlyList<string> Log => _log;

        public void Arm() => _log.Add(nameof(Arm));

        public void OpenPortal() => _log.Add(nameof(OpenPortal));

        public void RaiseDialogueStarted() => DialogueStarted?.Invoke();

        public void RaiseDialogueEnded() => DialogueEnded?.Invoke();
    }
}
