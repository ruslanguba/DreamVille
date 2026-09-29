using System;

namespace DreamVille.UI.Menus
{
    /// <summary>The in-game pause menu: three pages and three commands.</summary>
    public interface IPauseMenuView
    {
        event Action ContinueRequested;

        event Action RestartRequested;

        event Action ExitToMenuRequested;

        /// <summary>Opens the menu on its first page with sub-pages closed and no button selected.</summary>
        void ShowMenu();

        void ShowComics();

        void Hide();
    }
}
