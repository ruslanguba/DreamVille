using System;
using Zenject;

using DreamVille.Gameplay.Enemies;
using DreamVille.Scoring;

namespace DreamVille.Progression
{
    /// <summary>
    /// The level's scoring rules: a kill is worth score by its ricochet count, every score feeds the star
    /// meter, and clearing the level records the best score. Knows nothing about the HUD.
    /// </summary>
    /// <remarks>
    /// <see cref="IEnemyRegistry"/> raises the last <c>EnemyKilled</c> before <c>AllEnemiesKilled</c>, so the
    /// saved best score already includes the final kill.
    /// </remarks>
    public sealed class ScoringCoordinator : IInitializable, IDisposable
    {
        private readonly IEnemyRegistry _enemies;
        private readonly IScoreService _score;
        private readonly IStarProgressService _stars;
        private readonly IBestScoreService _bestScore;

        public ScoringCoordinator(
            IEnemyRegistry enemies,
            IScoreService score,
            IStarProgressService stars,
            IBestScoreService bestScore)
        {
            _enemies = enemies ?? throw new ArgumentNullException(nameof(enemies));
            _score = score ?? throw new ArgumentNullException(nameof(score));
            _stars = stars ?? throw new ArgumentNullException(nameof(stars));
            _bestScore = bestScore ?? throw new ArgumentNullException(nameof(bestScore));
        }

        public void Initialize()
        {
            _enemies.EnemyKilled += OnEnemyKilled;
            _enemies.AllEnemiesKilled += OnAllEnemiesKilled;
            _score.ScoreAdded += OnScoreAdded;
        }

        public void Dispose()
        {
            _enemies.EnemyKilled -= OnEnemyKilled;
            _enemies.AllEnemiesKilled -= OnAllEnemiesKilled;
            _score.ScoreAdded -= OnScoreAdded;
        }

        private void OnEnemyKilled(int remainingEnemies, int ricochetCount)
        {
            _score.HandleEnemyKilled(ricochetCount);
        }

        // Every score (kills, pickups, paint) feeds the stars, not only kills.
        private void OnScoreAdded(float delta)
        {
            _stars.AddScore(delta);
        }

        private void OnAllEnemiesKilled()
        {
            _bestScore.TrySave(_score.TotalScore);
        }
    }
}
