using System;
using Zenject;

using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Weapons.Bullets;
using DreamVille.Gameplay.Weapons.Pillows;
using DreamVille.Scoring;

namespace DreamVille.UI.Hud
{
    /// <summary>
    /// Translates level events into HUD calls. Holds no game rules: score, stars and the best score are
    /// decided by <c>ScoringCoordinator</c>; this class only shows their results.
    /// </summary>
    public sealed class HudPresenter : IInitializable, IDisposable
    {
        /// <summary>Hit count at which the first ricochet badge ("x2") appears.</summary>
        public const int FirstBadgeHitCount = 2;

        private readonly IGameHudView _view;
        private readonly IScoreService _score;
        private readonly IStarProgressService _stars;
        private readonly IEnemyRegistry _enemies;
        private readonly IBulletHitSource _bulletHits;
        private readonly IPillowThrower _pillows;

        public HudPresenter(
            IGameHudView view,
            IScoreService score,
            IStarProgressService stars,
            IEnemyRegistry enemies,
            IBulletHitSource bulletHits,
            IPillowThrower pillows)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _score = score ?? throw new ArgumentNullException(nameof(score));
            _stars = stars ?? throw new ArgumentNullException(nameof(stars));
            _enemies = enemies ?? throw new ArgumentNullException(nameof(enemies));
            _bulletHits = bulletHits ?? throw new ArgumentNullException(nameof(bulletHits));
            _pillows = pillows ?? throw new ArgumentNullException(nameof(pillows));
        }

        /// <summary>
        /// Pulls the current pillow and enemy state, then subscribes. The HUD prefab and the scene's
        /// Awake-time state exist before any kernel initializes, so the first frame is already correct.
        /// </summary>
        public void Initialize()
        {
            OnPillowsChanged();
            _view.EnemyCounter.SetRemaining(_enemies.RemainingEnemies);

            _score.ScoreAdded += OnScoreAdded;
            _stars.StarProgressChanged += OnStarProgressChanged;
            _stars.StarUnlocked += OnStarUnlocked;
            _pillows.Changed += OnPillowsChanged;
            _enemies.EnemyKilled += OnEnemyKilled;
            _bulletHits.BulletHit += OnBulletHit;
        }

        /// <summary>Unsubscribes only; never calls the views, which may already be destroyed with the scene.</summary>
        public void Dispose()
        {
            _score.ScoreAdded -= OnScoreAdded;
            _stars.StarProgressChanged -= OnStarProgressChanged;
            _stars.StarUnlocked -= OnStarUnlocked;
            _pillows.Changed -= OnPillowsChanged;
            _enemies.EnemyKilled -= OnEnemyKilled;
            _bulletHits.BulletHit -= OnBulletHit;
        }

        private void OnScoreAdded(float delta)
        {
            // A pickup worth nothing (a skin) adds no score; a "0" pop-up would only read as a bug.
            if (delta <= 0f)
                return;

            _view.Score.ShowAdded(delta);
        }

        private void OnStarProgressChanged(int starIndex, float normalized)
        {
            _view.Stars.SetProgress(starIndex, normalized);
        }

        private void OnStarUnlocked(int starIndex)
        {
            _view.Stars.PlayUnlock(starIndex);
        }

        private void OnPillowsChanged()
        {
            _view.Pillows.Show(_pillows.Available, _pillows.InHand);
        }

        // Score for the kill is added by ScoringCoordinator; the HUD only shows what is left.
        private void OnEnemyKilled(int remainingEnemies, int hitCount)
        {
            _view.EnemyCounter.SetRemaining(remainingEnemies);
        }

        private void OnBulletHit(BulletHitInfo hit)
        {
            int badgeIndex = hit.HitCount - FirstBadgeHitCount;
            if (badgeIndex < 0 || badgeIndex >= _view.Ricochet.BadgeCount)
                return;

            _view.Ricochet.ShowBadge(badgeIndex, hit.HitPoint);
        }
    }
}
