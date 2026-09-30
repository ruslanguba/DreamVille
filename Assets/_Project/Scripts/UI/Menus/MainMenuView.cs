using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace DreamVille.UI.Menus
{
    /// <summary>
    /// The main-menu buttons that reach services: New Game, Quit and one button per language.
    /// Panel navigation stays on the buttons' Inspector events. Passive: raises <see cref="IMainMenuView"/>
    /// events, and each language button carries the project <see cref="Locale"/> asset it selects.
    /// </summary>
    public sealed class MainMenuView : MonoBehaviour, IMainMenuView
    {
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private LocaleOption[] _locales = Array.Empty<LocaleOption>();

        private UnityAction[] _localeClickHandlers;

        public event Action NewGameRequested;

        public event Action QuitRequested;

        public event Action<string> LocaleRequested;

        private void Awake()
        {
            if (_newGameButton == null)
                Debug.LogError($"[{nameof(MainMenuView)}] {nameof(_newGameButton)} is not assigned.", this);
            if (_quitButton == null)
                Debug.LogError($"[{nameof(MainMenuView)}] {nameof(_quitButton)} is not assigned.", this);

            _localeClickHandlers = new UnityAction[_locales.Length];
            for (int i = 0; i < _locales.Length; i++)
            {
                LocaleOption option = _locales[i];
                if (option == null || option.Button == null || option.Locale == null)
                {
                    Debug.LogError($"[{nameof(MainMenuView)}] Language option {i} needs both a button and a locale.", this);
                    continue;
                }

                string localeCode = option.Locale.Identifier.Code;
                _localeClickHandlers[i] = () => LocaleRequested?.Invoke(localeCode);
            }
        }

        private void OnEnable()
        {
            if (_newGameButton != null)
                _newGameButton.onClick.AddListener(OnNewGameClicked);
            if (_quitButton != null)
                _quitButton.onClick.AddListener(OnQuitClicked);

            for (int i = 0; i < _locales.Length; i++)
            {
                if (_localeClickHandlers[i] != null)
                    _locales[i].Button.onClick.AddListener(_localeClickHandlers[i]);
            }
        }

        private void OnDisable()
        {
            if (_newGameButton != null)
                _newGameButton.onClick.RemoveListener(OnNewGameClicked);
            if (_quitButton != null)
                _quitButton.onClick.RemoveListener(OnQuitClicked);

            for (int i = 0; i < _locales.Length; i++)
            {
                if (_localeClickHandlers[i] != null)
                    _locales[i].Button.onClick.RemoveListener(_localeClickHandlers[i]);
            }
        }

        private void OnNewGameClicked()
        {
            NewGameRequested?.Invoke();
        }

        private void OnQuitClicked()
        {
            QuitRequested?.Invoke();
        }

        /// <summary>A language button and the project locale asset it selects.</summary>
        [Serializable]
        private sealed class LocaleOption
        {
            [SerializeField] private Button _button;
            [SerializeField] private Locale _locale;

            public Button Button => _button;

            public Locale Locale => _locale;
        }
    }
}
