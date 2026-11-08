using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

using DreamVille.App;
using DreamVille.Configs;
using DreamVille.Core.Async;
using DreamVille.SceneManagement;

namespace DreamVille.UI.Menus
{
    /// <summary>
    /// Main-menu commands: new game, quit, language. Puts the cursor in UI mode for the scene.
    /// A new game always starts at the first level of the <see cref="SceneCatalog"/>.
    /// </summary>
    public sealed class MainMenuPresenter : IInitializable, IDisposable
    {
        private readonly IMainMenuView _view;
        private readonly ISceneLoader _sceneLoader;
        private readonly SceneCatalog _catalog;
        private readonly ICursorService _cursor;
        private readonly IApplicationQuitter _quitter;
        private readonly ILocaleSelector _locales;

        private CancellationTokenSource _lifetime = new CancellationTokenSource();

        public MainMenuPresenter(
            IMainMenuView view,
            ISceneLoader sceneLoader,
            SceneCatalog catalog,
            ICursorService cursor,
            IApplicationQuitter quitter,
            ILocaleSelector locales)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
            _catalog = catalog != null ? catalog : throw new ArgumentNullException(nameof(catalog));
            _cursor = cursor ?? throw new ArgumentNullException(nameof(cursor));
            _quitter = quitter ?? throw new ArgumentNullException(nameof(quitter));
            _locales = locales ?? throw new ArgumentNullException(nameof(locales));
        }

        public void Initialize()
        {
            _cursor.UseUiMode();

            _view.NewGameRequested += OnNewGameRequested;
            _view.QuitRequested += OnQuitRequested;
            _view.LocaleRequested += OnLocaleRequested;
        }

        /// <summary>Unsubscribes and cancels a pending scene load; never calls the view.</summary>
        public void Dispose()
        {
            _view.NewGameRequested -= OnNewGameRequested;
            _view.QuitRequested -= OnQuitRequested;
            _view.LocaleRequested -= OnLocaleRequested;

            Cancellation.CancelAndDispose(ref _lifetime);
        }

        private void OnNewGameRequested()
        {
            LevelDefinition firstLevel = _catalog.Levels != null && _catalog.Levels.Count > 0
                ? _catalog.Levels[0]
                : null;

            if (firstLevel == null)
            {
                Debug.LogError($"[{nameof(MainMenuPresenter)}] The scene catalog has no first level; a new game cannot start.");
                return;
            }

            _sceneLoader.LoadAndLogAsync(firstLevel.SceneName, _lifetime.Token).Forget();
        }

        private void OnQuitRequested()
        {
            _quitter.Quit();
        }

        private void OnLocaleRequested(string localeCode)
        {
            _locales.Select(localeCode);
        }
    }
}
