using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using DreamVille.Configs;
using DreamVille.Scoring;

namespace DreamVille.Tests.EditMode.Scoring
{
    public class ScoreServiceTests
    {
        private ScoreRules _rules;
        private ScoreService _service;

        [SetUp]
        public void SetUp()
        {
            _rules = ScoreRules.CreateForTests(10f);
            _service = new ScoreService(_rules);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_rules);
        }

        [Test]
        public void HandleEnemyKilled_WithHitCount_MultipliesScorePerKillByHitCount()
        {
            _service.HandleEnemyKilled(3);

            Assert.That(_service.TotalScore, Is.EqualTo(30f));
        }

        [Test]
        public void AddScore_RaisesScoreAddedWithDelta()
        {
            float? addedDelta = null;
            _service.ScoreAdded += delta => addedDelta = delta;

            _service.AddScore(15f);

            Assert.That(addedDelta, Is.EqualTo(15f));
            Assert.That(_service.TotalScore, Is.EqualTo(15f));
        }

        [Test]
        public void HandleEnemyKilled_CalledTwice_AccumulatesTotalScore()
        {
            var deltas = new List<float>();
            _service.ScoreAdded += deltas.Add;

            _service.HandleEnemyKilled(1);
            _service.HandleEnemyKilled(2);

            Assert.That(deltas, Is.EqualTo(new[] { 10f, 20f }));
            Assert.That(_service.TotalScore, Is.EqualTo(30f));
        }
    }
}
