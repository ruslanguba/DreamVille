using NUnit.Framework;

using DreamVille.Tests.EditMode.Fakes;
using DreamVille.UI.Menus;

namespace DreamVille.Tests.EditMode.UI
{
    /// <summary>
    /// The pause menu as a table of "menu event in, page out" and "button in, command out".
    /// The pause rules themselves are pinned by <c>InGameMenuControllerTests</c>.
    /// </summary>
    public class PauseMenuPresenterTests
    {
        private FakePauseMenuView _view;
        private FakeInGameMenu _menu;
        private PauseMenuPresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            _view = new FakePauseMenuView();
            _menu = new FakeInGameMenu();
            _presenter = new PauseMenuPresenter(_view, _menu);
        }

        [TearDown]
        public void TearDown()
        {
            _presenter.Dispose();
        }

        [Test]
        public void MenuOpened_ShowsMenu()
        {
            _presenter.Initialize();

            _menu.RaiseMenuOpened();

            Assert.That(_view.Calls, Is.EqualTo(new[] { "ShowMenu" }));
        }

        [Test]
        public void ComicsOpened_ShowsComics()
        {
            _presenter.Initialize();

            _menu.RaiseComicsOpened();

            Assert.That(_view.Calls, Is.EqualTo(new[] { "ShowComics" }));
        }

        [Test]
        public void MenuClosed_HidesMenu()
        {
            _presenter.Initialize();

            _menu.RaiseMenuClosed();

            Assert.That(_view.Calls, Is.EqualTo(new[] { "Hide" }));
        }

        [Test]
        public void ContinueRequested_ContinuesGame()
        {
            _presenter.Initialize();

            _view.PressContinue();

            Assert.That(_menu.Calls, Is.EqualTo(new[] { "ContinueGame" }));
        }

        [Test]
        public void RestartRequested_RestartsLevel()
        {
            _presenter.Initialize();

            _view.PressRestart();

            Assert.That(_menu.Calls, Is.EqualTo(new[] { "RestartLevel" }));
        }

        [Test]
        public void ExitToMenuRequested_ExitsToMenu()
        {
            _presenter.Initialize();

            _view.PressExitToMenu();

            Assert.That(_menu.Calls, Is.EqualTo(new[] { "ExitToMenu" }));
        }

        [Test]
        public void Dispose_ThenEveryEvent_LeavesViewAndMenuUntouched()
        {
            _presenter.Initialize();
            _presenter.Dispose();

            _menu.RaiseMenuOpened();
            _menu.RaiseComicsOpened();
            _menu.RaiseMenuClosed();
            _view.PressContinue();
            _view.PressRestart();
            _view.PressExitToMenu();

            Assert.That(_view.Calls, Is.Empty);
            Assert.That(_menu.Calls, Is.Empty);
        }
    }
}
