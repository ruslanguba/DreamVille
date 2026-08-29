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

        [Inject]
        public void Construct(InGameMenuController menu)
        {
            _menu = menu;
        }

        private void Start()
        {
            _menu.MenuClosed += CloseMenu;
            _menu.ComicsOpened += OpenCommics;
            _menu.MenuOpened += OpenMenu;
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
