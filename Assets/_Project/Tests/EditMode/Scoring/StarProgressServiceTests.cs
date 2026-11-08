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

        private StarRules _rules;
        private LevelDefinition _level;
        private InMemorySaveStorage _storage;
        private ProgressRepository _progress;
        private StarProgressService _service;

        [SetUp]
        public void SetUp()
        {
            _rules = StarRules.CreateForTests(50f, 100f, 150f);
            _level = LevelDefinition.CreateForTests("__test_scene", LevelId, _rules);

            _storage = new InMemorySaveStorage();
            _progress = new ProgressRepository(_storage);
            _service = new StarProgressService(_level, _progress);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_level);
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
        }

        [Test]
        public void AddScore_CrossingThresholdWithOverflow_RaisesProgressForNextStar()
        {
            _service.AddScore(40f);
            var progress = new List<(int Index, float Value)>();
            _service.StarProgressChanged += (index, value) => progress.Add((index, value));

            _service.AddScore(20f);

            Assert.That(progress[progress.Count - 1].Index, Is.EqualTo(1));
            Assert.That(progress[progress.Count - 1].Value, Is.EqualTo(0.2f).Within(1e-5f));
        }

        [Test]
        public void AddScore_CrossingLastThreshold_RaisesNoProgressBeyondLastStar()
        {
            var progressIndices = new List<int>();
            _service.StarProgressChanged += (index, _) => progressIndices.Add(index);

            _service.AddScore(160f);

            Assert.That(progressIndices, Has.None.GreaterThanOrEqualTo(3));
            Assert.That(_progress.GetStars(LevelId), Is.EqualTo(3));
        }

        [Test]
        public void AddScore_ReachingFirstThreshold_PersistsOneStar()
        {
            _service.AddScore(50f);

            Assert.That(_progress.GetStars(LevelId), Is.EqualTo(1));
        }

        [Test]
        public void Constructor_WithNonAscendingThresholds_Throws()
        {
            float[][] invalidThresholds =
            {
                new[] { 50f, 50f, 150f },
                new[] { 100f, 50f },
                new[] { 0f, 50f },
                new[] { -10f, 50f },
            };

            foreach (float[] thresholds in invalidThresholds)
            {
                StarRules rules = StarRules.CreateForTests(thresholds);
                LevelDefinition level = LevelDefinition.CreateForTests("__test_scene", LevelId, rules);

                try
                {
                    Assert.Throws<System.ArgumentException>(() => new StarProgressService(level, _progress), string.Join(", ", thresholds));
                }
                finally
                {
                    Object.DestroyImmediate(level);
                    Object.DestroyImmediate(rules);
                }
            }
        }
    }
}
