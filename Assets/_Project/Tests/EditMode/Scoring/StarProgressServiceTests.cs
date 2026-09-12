using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using DreamVille.Configs;
using DreamVille.Persistence;
using DreamVille.Scoring;

namespace DreamVille.Tests.EditMode.Scoring
{
    public class StarProgressServiceTests
    {
        private const string LevelId = "__test_stars";

        private StarRulesSO _rules;
        private InMemorySaveStorage _storage;
        private ProgressRepository _progress;
        private StarProgressService _service;

        [SetUp]
        public void SetUp()
        {
            _rules = ScriptableObject.CreateInstance<StarRulesSO>();
            _rules.levelId = LevelId;
            _rules.starThresholds = new[] { 50f, 100f, 150f };

            _storage = new InMemorySaveStorage();
            _progress = new ProgressRepository(_storage);
            _service = new StarProgressService(_rules, _progress);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_rules);
        }

        [Test]
        public void AddScore_BelowFirstThreshold_RaisesNormalizedProgressAndNoUnlock()
        {
            var unlocked = new List<int>();
            int progressStarIndex = -1;
            float progressValue = -1f;
            _service.StarUnlocked += unlocked.Add;
            _service.StarProgressChanged += (index, value) =>
            {
                progressStarIndex = index;
                progressValue = value;
            };

            _service.AddScore(25f);

            Assert.That(progressStarIndex, Is.EqualTo(0));
            Assert.That(progressValue, Is.EqualTo(0.5f));
            Assert.That(unlocked, Is.Empty);
        }

        [Test]
        public void AddScore_ReachingThreshold_RaisesStarUnlockedWithStarIndex()
        {
            var unlocked = new List<int>();
            _service.StarUnlocked += unlocked.Add;

            _service.AddScore(50f);

            Assert.That(unlocked, Is.EqualTo(new[] { 0 }));
        }

        [Test]
        public void AddScore_OneBigScore_UnlocksSeveralStarsInOrder()
        {
            var unlocked = new List<int>();
            _service.StarUnlocked += unlocked.Add;

            _service.AddScore(200f);

            Assert.That(unlocked, Is.EqualTo(new[] { 0, 1, 2 }));
            Assert.That(_service.UnlockedStars, Is.EqualTo(3));
        }

        [Test]
        public void UnlockStar_PersistsStarCountThroughRepository()
        {
            _service.AddScore(50f);

            Assert.That(_progress.GetStars(LevelId), Is.EqualTo(1));
        }
    }
}
