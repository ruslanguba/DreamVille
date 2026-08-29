using System;
using UnityEngine;
using Zenject;

using DreamVille.Core.Input;
using DreamVille.Dialogue;

namespace DreamVille.Core
{
    /// <summary>
    /// Игровое меню: реагирует на ввод, ставит паузу и сообщает UI, что показывать.
    /// Сама пауза, курсор и переходы сцен вынесены в сервисы.
    /// </summary>
    public class InGameMenuController : MonoBehaviour
    {
        public event Action MenuOpened;
        public event Action MenuClosed;
        public event Action ComicsOpened;

        [SerializeField] private GameInput gameInput;
        [SerializeField] private string mainMenuScene = "Main menu 1";

        private PauseService _pause;
        private GameFlowService _flow;
        private DialogueService _dialogue;

        [Inject]
        public void Construct(PauseService pause, GameFlowService flow, DialogueService dialogue)
        {
            _pause = pause;
            _flow = flow;
            _dialogue = dialogue;
        }

        private void OnEnable()
        {
            gameInput.OnPause += ToggleMenu;
            gameInput.OnTub += ToggleComics;
        }

        private void OnDisable()
        {
            gameInput.OnPause -= ToggleMenu;
            gameInput.OnTub -= ToggleComics;
        }

        private void Start()
        {
            _pause.Resume();
            _dialogue.ActiveChanged += OnDialogueActiveChanged;
        }

        private void OnDestroy()
        {
            if (_dialogue != null)
                _dialogue.ActiveChanged -= OnDialogueActiveChanged;
        }

        public void ContinueGame() => Close();

        public void RestartLevel() => _flow.RestartLevel();

        public void ExitToMenu() => _flow.ExitToMenu(mainMenuScene);

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

            // Во время диалога игра остаётся на паузе, даже если меню закрыли.
            if (_dialogue is { IsActive: true })
                return;

            _pause.Resume();
        }

        private void OnDialogueActiveChanged(bool isActive)
        {
            _pause.SetPlayerControl(!isActive);
        }
    }
}
