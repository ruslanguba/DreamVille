using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using DreamVille.Configs;
using DreamVille.Tests.EditMode.Fakes;
using DreamVille.UI.Menus;

namespace DreamVille.Tests.EditMode.UI
{
    /// <summary>The main menu's service commands: new game, quit, language, and the cursor it needs.</summary>
    public class MainMenuPresenterTests
    {
        private const string FirstLevelScene = "Lobby_01";
        private const string SecondLevelScene = "Level_01";

        private LevelDefinition _firstLevel;
        private LevelDefinition _secondLevel;
        private SceneCatalog _catalog;
        private FakeMainMenuView _view;
        private FakeSceneLoader _loader;
        private FakeCursorService _cursor;
        private FakeApplicationQuitter _quitter;
        private FakeLocaleSelector _locales;
        private MainMenuPresenter _presenter;
        private SceneCatalog _emptyCatalog;
        private MainMenuPresenter _emptyCatalogPresenter;

        [SetUp]
        public void SetUp()
        {
            _firstLevel = LevelDefinition.CreateForTests(FirstLevelScene, "1", unlockedByDefault: true);
            _secondLevel = LevelDefinition.CreateForTests(SecondLevelScene, "2");
            _catalog = SceneCatalog.CreateForTests(null, null, _firstLevel, _secondLevel);
            _view = new FakeMainMenuView();
            _loader = new FakeSceneLoader();
            _cursor = new FakeCursorService();
            _quitter = new FakeApplicationQuitter();
            _locales = new FakeLocaleSelector();
            _presenter = CreatePresenter(_catalog);
        }

        [TearDown]
        public void TearDown()
        {
            _presenter.Dispose();
            _emptyCatalogPresenter?.Dispose();
            Object.DestroyImmediate(_emptyCatalog);
            Object.DestroyImmediate(_catalog);
            Object.DestroyImmediate(_secondLevel);
            Object.DestroyImmediate(_firstLevel);
        }

        [Test]
        public void Initialize_SwitchesCursorToUiMode()
        {
            _presenter.Initialize();

            Assert.That(_cursor.LastMode, Is.EqualTo(FakeCursorService.Mode.Ui));
        }

        [Test]
        public void NewGameRequested_LoadsFirstCatalogLevel()
        {
            _presenter.Initialize();

            _view.PressNewGame();

            Assert.That(_loader.Calls, Is.EqualTo(new[] { "Load:" + FirstLevelScene }));
        }

        [Test]
        public void NewGameRequested_EmptyCatalog_LogsErrorAndDoesNotLoad()
        {
            _emptyCatalog = SceneCatalog.CreateForTests(null, null);
            _emptyCatalogPresenter = CreatePresenter(_emptyCatalog);
            _emptyCatalogPresenter.Initialize();
            LogAssert.Expect(LogType.Error, $"[{nameof(MainMenuPresenter)}] The scene catalog has no first level; a new game cannot start.");

            _view.PressNewGame();

            Assert.That(_loader.Calls, Is.Empty);
        }

        [Test]
        public void Dispose_CancelsAPendingLoad()
        {
            _presenter.Initialize();
            _view.PressNewGame();
            CancellationToken token = _loader.LastToken;

            _presenter.Dispose();

            Assert.That(token.IsCancellationRequested, Is.True);
        }

        [Test]
        public void QuitRequested_QuitsApplication()
        {
            _presenter.Initialize();

            _view.PressQuit();

            Assert.That(_quitter.QuitCount, Is.EqualTo(1));
        }

        [Test]
        public void LocaleRequested_SelectsThatLocale()
        {
            _presenter.Initialize();

            _view.PressLocale("ru");

            Assert.That(_locales.Selected, Is.EqualTo(new[] { "ru" }));
        }

        [Test]
        public void Dispose_ThenViewEvents_DoNothing()
        {
            _presenter.Initialize();
            _presenter.Dispose();

            _view.PressNewGame();
            _view.PressQuit();
            _view.PressLocale("en");

            Assert.That(_loader.Calls, Is.Empty);
            Assert.That(_quitter.QuitCount, Is.Zero);
            Assert.That(_locales.Selected, Is.Empty);
        }

        private MainMenuPresenter CreatePresenter(SceneCatalog catalog)
        {
            return new MainMenuPresenter(_view, _loader, catalog, _cursor, _quitter, _locales);
        }
    }
}
