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
        public void HandleEnemyKilled_WithRicochets_MultipliesScorePerKillByRicochetCount()
        {
            _service.HandleEnemyKilled(3);

            Assert.That(_service.TotalScore, Is.EqualTo(30f));
        }

        [Test]
        public void AddScore_RaisesScoreAddedWithDeltaAndTotalScoreChangedWithRunningTotal()
        {
            float? addedDelta = null;
            float? totalAfterChange = null;
            _service.ScoreAdded += delta => addedDelta = delta;
            _service.TotalScoreChanged += total => totalAfterChange = total;

            _service.AddScore(15f);

            Assert.That(addedDelta, Is.EqualTo(15f));
            Assert.That(totalAfterChange, Is.EqualTo(15f));
        }

        [Test]
        public void HandleEnemyKilled_CalledTwice_AccumulatesTotalScore()
        {
            var totals = new List<float>();
            _service.TotalScoreChanged += totals.Add;

            _service.HandleEnemyKilled(1);
            _service.HandleEnemyKilled(2);

            Assert.That(totals, Is.EqualTo(new[] { 10f, 30f }));
            Assert.That(_service.TotalScore, Is.EqualTo(30f));
        }
    }
}
