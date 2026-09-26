using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using DreamVille.Configs;
using DreamVille.Persistence;
using DreamVille.Progression;
using DreamVille.Scoring;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Progression
{
    /// <summary>
    /// The level's scoring rules end to end over the real scoring services and an in-memory save:
    /// kills become score, every score feeds the stars once, clearing the level records the best score.
    /// </summary>
    public class ScoringCoordinatorTests
    {
        private const string LevelId = "__test_scoring";
        private const float ScorePerKill = 10f;

        private ScoreRules _scoreRules;
        private StarRules _starRules;
        private LevelDefinition _level;
        private InMemorySaveStorage _storage;
        private ProgressRepository _progress;
        private FakeEnemyRegistry _enemies;
        private ScoreService _score;
        private StarProgressService _stars;
        private BestScoreService _bestScore;
        private ScoringCoordinator _coordinator;

        [SetUp]
        public void SetUp()
        {
            _scoreRules = ScoreRules.CreateForTests(ScorePerKill);
            _starRules = StarRules.CreateForTests(50f, 100f, 150f);
            _level = LevelDefinition.CreateForTests("__test_scene", LevelId, _starRules);

            _storage = new InMemorySaveStorage();
            _progress = new ProgressRepository(_storage);
            _enemies = new FakeEnemyRegistry();
            _score = new ScoreService(_scoreRules);
            _stars = new StarProgressService(_level, _progress);
            _bestScore = new BestScoreService(_level, _progress);

            _coordinator = new ScoringCoordinator(_enemies, _score, _stars, _bestScore);
            _coordinator.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _coordinator.Dispose();
            Object.DestroyImmediate(_level);
            Object.DestroyImmediate(_starRules);
            Object.DestroyImmediate(_scoreRules);
        }

        [Test]
        public void EnemyKilled_WithRicochets_AddsScorePerKillTimesRicochets()
        {
            _enemies.RaiseEnemyKilled(3, 3);

            Assert.That(_score.TotalScore, Is.EqualTo(30f));
        }

        [Test]
        public void ScoreAdded_FromAPickup_FeedsStarProgress()
        {
            var progress = new List<(int Index, float Value)>();
            _stars.StarProgressChanged += (index, value) => progress.Add((index, value));

            _score.AddScore(25f);

            Assert.That(progress, Is.EqualTo(new[] { (0, 0.5f) }));
        }

        [Test]
        public void EnemyKilled_ReachingThreshold_UnlocksFirstStar()
        {
            var unlocked = new List<int>();
            _stars.StarUnlocked += unlocked.Add;

            _enemies.RaiseEnemyKilled(2, 3);
            _enemies.RaiseEnemyKilled(1, 2);

            Assert.That(unlocked, Is.EqualTo(new[] { 0 }));
            Assert.That(_stars.UnlockedStars, Is.EqualTo(1));
        }

        [Test]
        public void AllEnemiesKilled_SavesTotalScoreAsBest()
        {
            _score.AddScore(40f);

            _enemies.RaiseAllEnemiesKilled();

            Assert.That(_progress.GetBestScore(LevelId), Is.EqualTo(40f));
        }

        [Test]
        public void AllEnemiesKilled_AfterFinalKill_BestIncludesFinalKill()
        {
            _enemies.RaiseEnemyKilled(0, 2);

            _enemies.RaiseAllEnemiesKilled();

            Assert.That(_progress.GetBestScore(LevelId), Is.EqualTo(20f));
        }

        [Test]
        public void AllEnemiesKilled_BelowStoredBest_KeepsStoredBest()
        {
            _progress.TrySetBestScore(LevelId, 100f);
            _score.AddScore(40f);

            _enemies.RaiseAllEnemiesKilled();

            Assert.That(_progress.GetBestScore(LevelId), Is.EqualTo(100f));
        }

        [Test]
        public void Dispose_ThenEvents_ChangeNothing()
        {
            _coordinator.Dispose();

            _enemies.RaiseEnemyKilled(0, 2);
            _enemies.RaiseAllEnemiesKilled();

            Assert.That(_score.TotalScore, Is.EqualTo(0f));
            Assert.That(_storage.HasKey(SaveKeys.BestScore(LevelId)), Is.False);
        }

        [Test]
        public void ScoreAdded_IsForwardedToStarsExactlyOnce()
        {
            var progress = new List<(int Index, float Value)>();
            _stars.StarProgressChanged += (index, value) => progress.Add((index, value));

            _score.AddScore(10f);

            Assert.That(progress, Is.EqualTo(new[] { (0, 0.2f) }));
        }
    }
}
