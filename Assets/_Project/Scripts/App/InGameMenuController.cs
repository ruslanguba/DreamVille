using System;
using Zenject;

using DreamVille.Dialogue;
using DreamVille.Input;

namespace DreamVille.App
{
    /// <summary>
    /// In-game menu: reacts to input, pauses the game and tells the UI what to show.
    /// Pause, cursor and scene transitions themselves live in dedicated services.
    /// </summary>
    public class InGameMenuController : IInGameMenu, IInitializable, IDisposable
    {
        public event Action MenuOpened;
        public event Action MenuClosed;
        public event Action ComicsOpened;

        private readonly IPauseService _pause;
        private readonly IGameFlowService _flow;
        private readonly IDialogueState _dialogue;
        private readonly GameInput _input;

        public InGameMenuController(
            IPauseService pause,
            IGameFlowService flow,
            IDialogueState dialogue,
            GameInput input)
        {
            _pause = pause;
            _flow = flow;
            _dialogue = dialogue;
            _input = input;
        }

        public void Initialize()
        {
            _input.OnPause += ToggleMenu;
            _input.OnTub += ToggleComics;
            _dialogue.ActiveChanged += OnDialogueActiveChanged;

            _pause.Resume();
        }

        public void Dispose()
        {
            _input.OnPause -= ToggleMenu;
            _input.OnTub -= ToggleComics;
            _dialogue.ActiveChanged -= OnDialogueActiveChanged;
        }

        public void ContinueGame() => Close();

        public void RestartLevel() => _flow.RestartLevel();

        public void ExitToMenu() => _flow.ExitToMenu();

        private void ToggleMenu()
        {
            if (_pause.IsPaused)
            {
                Close();
                return;
            }

            MenuOpened?.Invoke();
            _pause.Pause();
        }

        private void ToggleComics()
        {
            if (_pause.IsPaused)
            {
                Close();
                return;
            }

            ComicsOpened?.Invoke();
            _pause.Pause();
        }

        private void Close()
        {
            MenuClosed?.Invoke();

            // While a dialogue runs the game stays paused, even after the menu is closed.
            if (_dialogue.IsActive)
                return;

            _pause.Resume();
        }

        private void OnDialogueActiveChanged(bool isActive)
        {
            _pause.SetPlayerControl(!isActive);
        }
    }
}
