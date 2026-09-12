using System.Collections.Generic;

using DreamVille.App;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Hand-written double for <see cref="IPauseService"/>, used to isolate
    /// <see cref="GameFlowService"/> from the real pause implementation. Accepts an
    /// external log so a test can interleave its calls with another fake's, to assert
    /// cross-service call order.</summary>
    public sealed class FakePauseService : IPauseService
    {
        private readonly List<string> _callLog;

        public FakePauseService(List<string> callLog = null)
        {
            _callLog = callLog ?? new List<string>();
        }

        public IReadOnlyList<string> CallLog => _callLog;

        public bool IsPaused { get; private set; }

        public void Pause()
        {
            IsPaused = true;
            _callLog.Add("Pause");
        }

        public void Resume()
        {
            IsPaused = false;
            _callLog.Add("Resume");
        }

        public void SetPlayerControl(bool isEnabled)
        {
            _callLog.Add(isEnabled ? "SetPlayerControl:true" : "SetPlayerControl:false");
        }
    }
}
