using NUnit.Framework;
using UnityEngine;

using DreamVille.Configs;
using DreamVille.Persistence;
using DreamVille.Tests.EditMode.Fakes;
using DreamVille.UI.Menus;

namespace DreamVille.Tests.EditMode.UI
{
    /// <summary>
    /// Level select over a real <see cref="ProgressRepository"/>: saved stars are always shown, and a level
    /// is playable only when unlocked by default or completed once. A locked level never loads.
    /// </summary>
    public class LevelSelectPresenterTests
    {
        private const string LobbyScene = "Lobby_01";
        private const string FirstLevelScene = "Level_01";
        private const string SecondLevelScene = "Level_02";

        private LevelDefinition _lobby;
        private LevelDefinition _firstLevel;
        private LevelDefinition _secondLevel;
        private ProgressRepository _progress;
        private FakeLevelSelectView _view;
        private FakeSceneLoader _loader;
        private LevelSelectPresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            _lobby = LevelDefinition.CreateForTests(LobbyScene, "1", unlockedByDefault: true);
            _firstLevel = LevelDefinition.CreateForTests(FirstLevelScene, "2");
            _secondLevel = LevelDefinition.CreateForTests(SecondLevelScene, "3");
            _progress = new ProgressRepository(new InMemorySaveStorage());
            _view = new FakeLevelSelectView(_lobby, _firstLevel, _secondLevel);
            _loader = new FakeSceneLoader();
            _presenter = new LevelSelectPresenter(_view, _progress, _loader);
        }

        [TearDown]
        public void TearDown()
        {
            _presenter.Dispose();
            Object.DestroyImmediate(_secondLevel);
            Object.DestroyImmediate(_firstLevel);
            Object.DestroyImmediate(_lobby);
        }

        [Test]
        public void Initialize_ShowsSavedStarsForEveryLevel()
        {
            _progress.SetStarsIfHigher("1", 3);
            _progress.SetStarsIfHigher("2", 1);

            _presenter.Initialize();

            Assert.That(_view.Shown.Count, Is.EqualTo(3));
            Assert.That(_view.Shown[0].Stars, Is.EqualTo(3));
            Assert.That(_view.Shown[1].Stars, Is.EqualTo(1));
            Assert.That(_view.Shown[2].Stars, Is.EqualTo(0));
        }

        [Test]
        public void Initialize_LevelUnlockedByDefault_IsPlayable()
        {
            _presenter.Initialize();

            Assert.That(_view.Shown[0], Is.EqualTo((0, 0, true)));
        }

        [Test]
        public void Initialize_CompletedLevel_IsPlayable()
        {
            _progress.MarkCompleted("2");

            _presenter.Initialize();

            Assert.That(_view.Shown[1].IsPlayable, Is.True);
        }

        [Test]
        public void Initialize_UncompletedLevelWithStars_IsLockedButShowsStars()
        {
            _progress.SetStarsIfHigher("3", 3);

            _presenter.Initialize();

            Assert.That(_view.Shown[2], Is.EqualTo((2, 3, false)));
        }

        [Test]
        public void LevelChosen_Playable_LoadsItsScene()
        {
            _progress.MarkCompleted("2");
            _presenter.Initialize();

            _view.Choose(_firstLevel);

            Assert.That(_loader.Calls, Is.EqualTo(new[] { "Load:" + FirstLevelScene }));
        }

        [Test]
        public void LevelChosen_Locked_DoesNotLoad()
        {
            _presenter.Initialize();

            _view.Choose(_secondLevel);

            Assert.That(_loader.Calls, Is.Empty);
        }

        [Test]
        public void Dispose_ThenLevelChosen_DoesNotLoad()
        {
            _presenter.Initialize();
            _presenter.Dispose();

            _view.Choose(_lobby);

            Assert.That(_loader.Calls, Is.Empty);
        }
    }
}
