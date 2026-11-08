using NUnit.Framework;
using UnityEngine;

using DreamVille.Configs;
using DreamVille.Persistence;
using DreamVille.Progression;
using DreamVille.Progression.Pillows;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Progression
{
    /// <summary>
    /// A hub has no enemies and no final zone, so its exit portal completes it through
    /// <see cref="DreamVille.Core.Scenes.ILevelCompletion"/>.
    /// </summary>
    public class LevelCompletionTests
    {
        private LevelDefinition _hub;
        private LevelDefinition _nextLevel;
        private ProgressRepository _progress;
        private LevelDirector _director;

        [SetUp]
        public void SetUp()
        {
            _hub = LevelDefinition.CreateForTests("Lobby_Test", "hub", maxPillows: 2);
            _nextLevel = LevelDefinition.CreateForTests("Level_Test", "next", maxPillows: 2);
            _progress = new ProgressRepository(new InMemorySaveStorage());
            _director = new LevelDirector(new FakeEnemyRegistry(), _hub, _progress, new FakeLevelCast());
        }

        [TearDown]
        public void TearDown()
        {
            _director.Dispose();
            Object.DestroyImmediate(_hub);
            Object.DestroyImmediate(_nextLevel);
        }

        [Test]
        public void CompleteLevel_MarksTheLevelCompleted()
        {
            _director.Initialize();

            _director.CompleteLevel();

            Assert.That(_progress.IsCompleted("hub"), Is.True);
        }

        [Test]
        public void CompleteLevel_WithoutTheHubReward_NextLevelStillGetsTheHubPillows()
        {
            _director.Initialize();
            var catalog = SceneCatalog.CreateForTests(null, null, _hub, _nextLevel);

            _director.CompleteLevel();
            var inventory = new PillowInventory(_progress, _nextLevel, catalog);

            Assert.That(inventory.Available, Is.EqualTo(2));
            Object.DestroyImmediate(catalog);
        }
    }
}
