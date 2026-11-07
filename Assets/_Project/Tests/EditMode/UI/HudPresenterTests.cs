using System;
using NUnit.Framework;
using UnityEngine;

using DreamVille.Tests.EditMode.Fakes;
using DreamVille.UI.Hud;

namespace DreamVille.Tests.EditMode.UI
{
    /// <summary>
    /// The gameplay HUD's behaviour as a table of "level event in, view call out", over recording fakes.
    /// The presenter holds no scoring rules; those are pinned by <c>ScoringCoordinatorTests</c>.
    /// </summary>
    public class HudPresenterTests
    {
        private FakeGameHudView _view;
        private FakeScoreService _score;
        private FakeStarProgressService _stars;
        private FakeEnemyRegistry _enemies;
        private FakeBulletHitSource _bulletHits;
        private FakePillowThrower _pillows;
        private HudPresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            _view = new FakeGameHudView();
            _score = new FakeScoreService();
            _stars = new FakeStarProgressService();
            _enemies = new FakeEnemyRegistry();
            _bulletHits = new FakeBulletHitSource();
            _pillows = new FakePillowThrower();
            _presenter = new HudPresenter(_view, _score, _stars, _enemies, _bulletHits, _pillows);
        }

        [TearDown]
        public void TearDown()
        {
            _presenter.Dispose();
        }

        [Test]
        public void Initialize_ShowsRemainingEnemies()
        {
            _enemies.RemainingEnemies = 4;

            _presenter.Initialize();

            Assert.That(_view.EnemyCounterFake.Shown, Is.EqualTo(new[] { 4 }));
        }

        [Test]
        public void Initialize_ShowsPillowStateFromThrower()
        {
            _pillows.Available = 2;
            _pillows.InHand = 1;

            _presenter.Initialize();

            Assert.That(_view.PillowsFake.Shown, Is.EqualTo(new[] { (2, 1) }));
        }

        [Test]
        public void Initialize_WithoutPillows_ShowsEmptyRow()
        {
            _presenter.Initialize();

            Assert.That(_view.PillowsFake.Shown, Is.EqualTo(new[] { (0, 0) }));
        }

        [Test]
        public void PillowsChanged_ShowsCurrentState()
        {
            _pillows.Available = 2;
            _pillows.InHand = 2;
            _presenter.Initialize();
            _pillows.InHand = 1;

            _pillows.RaiseChanged();

            Assert.That(_view.PillowsFake.Shown[_view.PillowsFake.Shown.Count - 1], Is.EqualTo((2, 1)));
        }

        [Test]
        public void EnemyKilled_ShowsRemainingCount()
        {
            _enemies.RemainingEnemies = 3;
            _presenter.Initialize();

            _enemies.RaiseEnemyKilled(2, 1);

            Assert.That(_view.EnemyCounterFake.Shown, Is.EqualTo(new[] { 3, 2 }));
        }

        [Test]
        public void EnemyKilled_DoesNotTouchScore()
        {
            _presenter.Initialize();

            _enemies.RaiseEnemyKilled(2, 3);

            Assert.That(_score.HandleEnemyKilledCalls, Is.EqualTo(0));
            Assert.That(_view.ScoreFake.Shown, Is.Empty);
        }

        [Test]
        public void ScoreAdded_ShowsDeltaInPopup()
        {
            _presenter.Initialize();

            _score.AddScore(30f);

            Assert.That(_view.ScoreFake.Shown, Is.EqualTo(new[] { 30f }));
        }

        [Test]
        public void ScoreAdded_Zero_ShowsNoPopup()
        {
            _presenter.Initialize();

            _score.AddScore(0f);

            Assert.That(_view.ScoreFake.Shown, Is.Empty);
        }

        [Test]
        public void StarProgressChanged_ForwardsIndexAndFill()
        {
            _presenter.Initialize();

            _stars.RaiseProgress(1, 0.2f);

            Assert.That(_view.StarsFake.Progress, Is.EqualTo(new[] { (1, 0.2f) }));
        }

        [Test]
        public void StarUnlocked_PlaysUnlockOnThatStar()
        {
            _presenter.Initialize();

            _stars.RaiseUnlocked(2);

            Assert.That(_view.StarsFake.Unlocked, Is.EqualTo(new[] { 2 }));
        }

        [Test]
        public void BulletHit_WithOneRicochet_ShowsNoBadge()
        {
            _presenter.Initialize();

            _bulletHits.RaiseHit(1, Vector3.one);

            Assert.That(_view.RicochetFake.Shown, Is.Empty);
        }

        [Test]
        public void BulletHit_WithTwoRicochets_ShowsFirstBadgeAtHitPoint()
        {
            var hitPoint = new Vector3(1f, 2f, 3f);
            _presenter.Initialize();

            _bulletHits.RaiseHit(2, hitPoint);

            Assert.That(_view.RicochetFake.Shown, Is.EqualTo(new[] { (0, hitPoint) }));
        }

        [Test]
        public void BulletHit_WithSixRicochets_ShowsLastOfFiveBadges()
        {
            _presenter.Initialize();

            _bulletHits.RaiseHit(6, Vector3.zero);

            Assert.That(_view.RicochetFake.Shown, Is.EqualTo(new[] { (4, Vector3.zero) }));
        }

        [Test]
        public void BulletHit_BeyondLastBadge_ShowsNothing()
        {
            _presenter.Initialize();

            _bulletHits.RaiseHit(7, Vector3.zero);

            Assert.That(_view.RicochetFake.Shown, Is.Empty);
        }

        [Test]
        public void Dispose_ThenEveryEvent_LeavesViewsUntouched()
        {
            _presenter.Initialize();
            int callsAfterInitialize = _view.CallCount;

            _presenter.Dispose();
            _score.AddScore(10f);
            _stars.RaiseProgress(0, 0.2f);
            _stars.RaiseUnlocked(0);
            _pillows.RaiseChanged();
            _enemies.RaiseEnemyKilled(0, 2);
            _bulletHits.RaiseHit(2, Vector3.zero);

            Assert.That(_view.CallCount, Is.EqualTo(callsAfterInitialize));
        }

        [Test]
        public void Constructor_WithNullView_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => new HudPresenter(null, _score, _stars, _enemies, _bulletHits, _pillows));
        }
    }
}
