using System;
using Zenject;

using DreamVille.App;

namespace DreamVille.UI.Menus
{
    /// <summary>
    /// Draws the in-game menu state decided by <see cref="IInGameMenu"/> and sends the view's commands back.
    /// Holds no pause rules: when time stops and when control returns is decided by <c>InGameMenuController</c>.
    /// </summary>
    public sealed class PauseMenuPresenter : IInitializable, IDisposable
    {
        private readonly IPauseMenuView _view;
        private readonly IInGameMenu _menu;

        public PauseMenuPresenter(IPauseMenuView view, IInGameMenu menu)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _menu = menu ?? throw new ArgumentNullException(nameof(menu));
        }

        public void Initialize()
        {
            _menu.MenuOpened += OnMenuOpened;
            _menu.ComicsOpened += OnComicsOpened;
            _menu.MenuClosed += OnMenuClosed;

            _view.ContinueRequested += OnContinueRequested;
            _view.RestartRequested += OnRestartRequested;
            _view.ExitToMenuRequested += OnExitToMenuRequested;
        }

        /// <summary>Unsubscribes only; never calls the view, which may already be destroyed with the scene.</summary>
        public void Dispose()
        {
            _menu.MenuOpened -= OnMenuOpened;
            _menu.ComicsOpened -= OnComicsOpened;
            _menu.MenuClosed -= OnMenuClosed;

            _view.ContinueRequested -= OnContinueRequested;
            _view.RestartRequested -= OnRestartRequested;
            _view.ExitToMenuRequested -= OnExitToMenuRequested;
        }

        private void OnMenuOpened()
        {
            _view.ShowMenu();
        }

        private void OnComicsOpened()
        {
            _view.ShowComics();
        }

        private void OnMenuClosed()
        {
            _view.Hide();
        }

        private void OnContinueRequested()
        {
            _menu.ContinueGame();
        }

        private void OnRestartRequested()
        {
            _menu.RestartLevel();
        }

        private void OnExitToMenuRequested()
        {
            _menu.ExitToMenu();
        }
    }
}
