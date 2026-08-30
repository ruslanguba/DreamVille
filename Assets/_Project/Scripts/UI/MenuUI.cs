using UnityEngine.EventSystems;
using UnityEngine;
using Zenject;

using DreamVille.Core;

namespace DreamVille.UI
{
    public class MenuUI : MonoBehaviour
    {
        [SerializeField] GameObject _mainMenuPanel;
        [SerializeField] GameObject _settingsPanel;
        [SerializeField] GameObject _exitPanel;

        [SerializeField] GameObject _menuParent;

        [SerializeField] GameObject _comics;

        private InGameMenuController _menu;

        // В главном меню игрового контроллера нет — там меню статично.
        [Inject]
        public void Construct([InjectOptional] InGameMenuController menu)
        {
            _menu = menu;
        }

        private void Start()
        {
            if (_menu != null)
            {
                _menu.MenuClosed += CloseMenu;
                _menu.ComicsOpened += OpenCommics;
                _menu.MenuOpened += OpenMenu;
            }
            _menuParent.SetActive(false);
            _mainMenuPanel.SetActive(false);
            _settingsPanel.SetActive(false);
            _comics.SetActive(false);
            _exitPanel.SetActive(false);
        }

        private void OnDisable()
        {
            if (_menu == null)
                return;

            _menu.MenuClosed -= CloseMenu;
            _menu.ComicsOpened -= OpenCommics;
            _menu.MenuOpened -= OpenMenu;
        }

        // Кнопки паузы вызывают эти методы: цель внутри того же префаба,
        // поэтому ссылка сохраняется и не требует настройки в каждой сцене.
        public void ContinueGame() => _menu?.ContinueGame();

        public void RestartLevel() => _menu?.RestartLevel();

        public void ExitToMenu() => _menu?.ExitToMenu();

        private void OpenMenu()
        {
            _menuParent.SetActive(true);
            _mainMenuPanel.gameObject.SetActive(true);
            _settingsPanel.gameObject.SetActive(false);
            _exitPanel.gameObject.SetActive(false);
            _comics.SetActive(false);
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        private void OpenCommics()
        {
            _menuParent.SetActive(false);
            _comics.SetActive(true);
        }

        private void CloseMenu() 
        {
            _menuParent.SetActive(false);
            _mainMenuPanel.SetActive(false);
            _comics.SetActive(false);
        }
    }
}
