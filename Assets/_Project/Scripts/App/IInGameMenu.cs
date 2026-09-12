using System;

namespace DreamVille.App
{
    /// <summary>
    /// The pause menu as the UI sees it: three things to show and three commands to run.
    /// </summary>
    public interface IInGameMenu
    {
        event Action MenuOpened;

        event Action MenuClosed;

        event Action ComicsOpened;

        void ContinueGame();

        void RestartLevel();

        void ExitToMenu();
    }
}
