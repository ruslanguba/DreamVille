using System;
using System.Collections.Generic;

using DreamVille.UI.Menus;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>
    /// Records the pages the presenter asked for ("ShowMenu", "ShowComics", "Hide"), in order;
    /// raises its button commands on request.
    /// </summary>
    public sealed class FakePauseMenuView : IPauseMenuView
    {
        private readonly List<string> _calls = new List<string>();

        public event Action ContinueRequested;
        public event Action RestartRequested;
        public event Action ExitToMenuRequested;

        public IReadOnlyList<string> Calls => _calls;

        public void ShowMenu()
        {
            _calls.Add(nameof(ShowMenu));
        }

        public void ShowComics()
        {
            _calls.Add(nameof(ShowComics));
        }

        public void Hide()
        {
            _calls.Add(nameof(Hide));
        }

        public void PressContinue()
        {
            ContinueRequested?.Invoke();
        }

        public void PressRestart()
        {
            RestartRequested?.Invoke();
        }

        public void PressExitToMenu()
        {
            ExitToMenuRequested?.Invoke();
        }
    }
}
