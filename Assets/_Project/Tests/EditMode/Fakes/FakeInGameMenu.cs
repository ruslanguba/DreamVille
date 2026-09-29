using System;
using System.Collections.Generic;

using DreamVille.App;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>
    /// In-game menu whose events are raised on request and whose commands are recorded
    /// ("ContinueGame", "RestartLevel", "ExitToMenu"), in order.
    /// </summary>
    public sealed class FakeInGameMenu : IInGameMenu
    {
        private readonly List<string> _calls = new List<string>();

        public event Action MenuOpened;
        public event Action MenuClosed;
        public event Action ComicsOpened;

        public IReadOnlyList<string> Calls => _calls;

        public void ContinueGame()
        {
            _calls.Add(nameof(ContinueGame));
        }

        public void RestartLevel()
        {
            _calls.Add(nameof(RestartLevel));
        }

        public void ExitToMenu()
        {
            _calls.Add(nameof(ExitToMenu));
        }

        public void RaiseMenuOpened()
        {
            MenuOpened?.Invoke();
        }

        public void RaiseMenuClosed()
        {
            MenuClosed?.Invoke();
        }

        public void RaiseComicsOpened()
        {
            ComicsOpened?.Invoke();
        }
    }
}
