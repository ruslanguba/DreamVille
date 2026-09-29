using System;

namespace DreamVille.UI.Menus
{
    /// <summary>Main-menu commands that reach services. Panel navigation stays in the Inspector.</summary>
    public interface IMainMenuView
    {
        event Action NewGameRequested;

        event Action QuitRequested;

        /// <summary>The player picked a language; the argument is a project locale code, for example "ru".</summary>
        event Action<string> LocaleRequested;
    }
}
