using System;
using Zenject;

using DreamVille.Dialogue;
using DreamVille.Input;

namespace DreamVille.App
{
    /// <summary>
    /// In-game menu rules: Esc and Tab open or close the menu and pause the game, and the UI is told what to show.
    /// While a dialogue runs the player has no control; closing the menu during a dialogue restores time
    /// but keeps the player's control off until the dialogue ends. While the menu is open it alone decides
    /// control: a dialogue that ends under it hands control back only when the menu closes.
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
        private readonly IMenuInput _input;

        public InGameMenuController(
            IPauseService pause,
            IGameFlowService flow,
            IDialogueState dialogue,
            IMenuInput input)
        {
            _pause = pause ?? throw new ArgumentNullException(nameof(pause));
            _flow = flow ?? throw new ArgumentNullException(nameof(flow));
            _dialogue = dialogue ?? throw new ArgumentNullException(nameof(dialogue));
            _input = input ?? throw new ArgumentNullException(nameof(input));
        }

        public void Initialize()
        {
            _input.PausePressed += ToggleMenu;
            _input.ComicsPressed += ToggleComics;
            _dialogue.ActiveChanged += OnDialogueActiveChanged;

            _pause.Resume();
        }

        public void Dispose()
        {
            _input.PausePressed -= ToggleMenu;
            _input.ComicsPressed -= ToggleComics;
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
            _pause.Resume();

            // The dialogue still owns the player: time runs again, control comes back when it ends.
            if (_dialogue.IsActive)
                _pause.SetPlayerControl(false);
        }

        private void OnDialogueActiveChanged(bool isActive)
        {
            // The menu owns control while it is open; Close() hands it back according to the dialogue state.
            if (_pause.IsPaused)
                return;

            _pause.SetPlayerControl(!isActive);
        }
    }
}
