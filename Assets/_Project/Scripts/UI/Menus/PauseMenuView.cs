using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DreamVille.UI.Menus
{
    /// <summary>
    /// The in-game pause menu: the menu pages, the comics page and the Continue, Restart and Exit buttons.
    /// Passive: shows what <see cref="PauseMenuPresenter"/> asks for and raises <see cref="IPauseMenuView"/>
    /// events for the three buttons, bound in code. Navigation between the menu pages stays on the buttons'
    /// Inspector events (they only switch panels).
    /// </summary>
    public sealed class PauseMenuView : MonoBehaviour, IPauseMenuView
    {
        [Header("Pages")]
        [SerializeField] private GameObject _mainMenuPanel;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _exitPanel;
        [SerializeField] private GameObject _menuParent;
        [SerializeField] private GameObject _comics;

        [Header("Commands")]
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _exitToMenuButton;

        public event Action ContinueRequested;

        public event Action RestartRequested;

        public event Action ExitToMenuRequested;

        private void Awake()
        {
            if (_continueButton == null || _restartButton == null || _exitToMenuButton == null)
                Debug.LogError($"[{nameof(PauseMenuView)}] Continue, Restart and Exit buttons must all be assigned.", this);

            _menuParent.SetActive(false);
            _mainMenuPanel.SetActive(false);
            _settingsPanel.SetActive(false);
            _comics.SetActive(false);
            _exitPanel.SetActive(false);
        }

        // The buttons are bound in code only; Awake reports a missing reference once.
        private void OnEnable()
        {
            if (_continueButton != null)
                _continueButton.onClick.AddListener(OnContinueClicked);
            if (_restartButton != null)
                _restartButton.onClick.AddListener(OnRestartClicked);
            if (_exitToMenuButton != null)
                _exitToMenuButton.onClick.AddListener(OnExitToMenuClicked);
        }

        private void OnDisable()
        {
            if (_continueButton != null)
                _continueButton.onClick.RemoveListener(OnContinueClicked);
            if (_restartButton != null)
                _restartButton.onClick.RemoveListener(OnRestartClicked);
            if (_exitToMenuButton != null)
                _exitToMenuButton.onClick.RemoveListener(OnExitToMenuClicked);
        }

        public void ShowMenu()
        {
            _menuParent.SetActive(true);
            _mainMenuPanel.SetActive(true);
            _settingsPanel.SetActive(false);
            _exitPanel.SetActive(false);
            _comics.SetActive(false);

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }

        public void ShowComics()
        {
            _menuParent.SetActive(false);
            _comics.SetActive(true);
        }

        /// <summary>Closes the menu and the comics; an open sub-page stays as it was until the next <see cref="ShowMenu"/>.</summary>
        public void Hide()
        {
            _menuParent.SetActive(false);
            _mainMenuPanel.SetActive(false);
            _comics.SetActive(false);
        }

        private void OnContinueClicked()
        {
            ContinueRequested?.Invoke();
        }

        private void OnRestartClicked()
        {
            RestartRequested?.Invoke();
        }

        private void OnExitToMenuClicked()
        {
            ExitToMenuRequested?.Invoke();
        }
    }
}
