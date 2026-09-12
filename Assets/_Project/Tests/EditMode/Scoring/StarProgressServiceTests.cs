using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using DreamVille.Configs;
using DreamVille.Scoring;

namespace DreamVille.Tests.EditMode.Scoring
{
    public class StarProgressServiceTests
    {
        private const string LevelId = "__test_stars";

        private StarRulesSO _rules;
        private StarProgressService _service;

        [SetUp]
        public void SetUp()
        {
            _rules = ScriptableObject.CreateInstance<StarRulesSO>();
            _rules.levelId = LevelId;
            _rules.starThresholds = new[] { 50f, 100f, 150f };
            _service = new StarProgressService(_rules);
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(LevelId + "_Stars");
            Object.DestroyImmediate(_rules);
        }

        [Test]
        public void AddScore_BelowFirstThreshold_RaisesNormalizedProgressAndNoUnlock()
        {
            var unlocked = new List<int>();
            int progressStarIndex = -1;
            float progressValue = -1f;
            _service.OnStarUnlocked += unlocked.Add;
            _service.OnStarProgressChanged += (index, value) =>
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
        public void AddScore_ReachingThreshold_RaisesOnStarUnlockedWithStarIndex()
        {
            var unlocked = new List<int>();
            _service.OnStarUnlocked += unlocked.Add;

            _service.AddScore(50f);

            Assert.That(unlocked, Is.EqualTo(new[] { 0 }));
        }

        [Test]
        public void AddScore_OneBigScore_UnlocksSeveralStarsInOrder()
        {
            var unlocked = new List<int>();
            _service.OnStarUnlocked += unlocked.Add;

            _service.AddScore(200f);

            Assert.That(unlocked, Is.EqualTo(new[] { 0, 1, 2 }));
            Assert.That(_service.UnlockedStars, Is.EqualTo(3));
        }
    }
}
