using System;

using DreamVille.UI.Menus;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Main-menu buttons pressed on request by the test.</summary>
    public sealed class FakeMainMenuView : IMainMenuView
    {
        public event Action NewGameRequested;
        public event Action QuitRequested;
        public event Action<string> LocaleRequested;

        public void PressNewGame()
        {
            NewGameRequested?.Invoke();
        }

        public void PressQuit()
        {
            QuitRequested?.Invoke();
        }

        public void PressLocale(string localeCode)
        {
            LocaleRequested?.Invoke(localeCode);
        }
    }
}
