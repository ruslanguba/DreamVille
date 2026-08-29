using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using Zenject;

using DreamVille.Core.Input;
using DreamVille.Core.SceneManagement;
using DreamVille.Dialogue;
using DreamVille.Gameplay.Player;

namespace DreamVille.Core
{
    public class GameManager : MonoBehaviour
    {
        public event Action OnOpenMenu;
        public event Action OnCloseMenu;
        public event Action OnSpace;

        public event Action OnOpenComics;

        public static GameManager Instance { get; private set; }

        [SerializeField] private GameInput gameInput;
        private bool _isPaused;

        private DialogueService _dialogue;
        private PlayerInputSwitch _playerInput;
        private SceneTransitionManager _transitions;

        [Inject]
        public void Construct(
            DialogueService dialogue,
            PlayerInputSwitch playerInput,
            SceneTransitionManager transitions)
        {
            _dialogue = dialogue;
            _playerInput = playerInput;
            _transitions = transitions;
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _dialogue.ActiveChanged += StartDialog;
        }

        private void OnDestroy()
        {
            if (_dialogue != null)
                _dialogue.ActiveChanged -= StartDialog;
        }
        private void OnEnable()
        {
            gameInput.OnPause += ShowMenu;
            gameInput.OnTub += ShowInstruction;
        }
        private void OnDisable()
        {
            gameInput.OnPause -= ShowMenu;
            gameInput.OnTub -= ShowInstruction;
        }

        private void ShowMenu()
        {
            OnOpenMenu?.Invoke();
            HandlePause();
        }

        private void ShowInstruction()
        {
            OnOpenComics?.Invoke();
            HandlePause();
        }

        public void HandlePause()
        {
            _isPaused = !_isPaused;

            if (_isPaused)
            {
                // Открыли меню
                PauseGame();
            }
            else
            {
                // Закрыли меню, но нужно учитывать диалог
                if (_dialogue is { IsActive: true })
                {
                    // Идёт диалог игра должна оставаться на паузе
                    OnCloseMenu?.Invoke();        // Закрываем меню
                                                  // но не продолжаем игру

                    // Важно: курсор и блокировка ввода такие же как при диалоге
                    PauseGame();

                    return;
                }

                // Обычно продолжаем игру
                ContinueGame();
            }
        }

        public void PauseGame()
        {
            Time.timeScale = 0;
            _playerInput.DisableInput();
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        public void ContinueGame()
        {
            Time.timeScale = 1;
            _playerInput.EnableInput();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _isPaused = false;
            OnCloseMenu?.Invoke();
        }

        public void ExitToMenu()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            Time.timeScale = 1;
            var request = new SceneTransitionRequest("Main menu 1");
            _transitions.Transition(request);
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            ContinueGame();
        }

        private void StartDialog(bool isActive)
        {
            if (isActive)
            {
                _playerInput.DisableInput();
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }
            else 
            {
                _playerInput.EnableInput();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
