using System.Collections.Generic;

using DreamVille.App;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Records "RestartLevel" and "ExitToMenu" calls, in order.</summary>
    public sealed class FakeGameFlowService : IGameFlowService
    {
        private readonly List<string> _calls = new List<string>();

        public IReadOnlyList<string> Calls => _calls;

        public void RestartLevel()
        {
            _calls.Add(nameof(RestartLevel));
        }

        public void ExitToMenu()
        {
            _calls.Add(nameof(ExitToMenu));
        }
    }
}
