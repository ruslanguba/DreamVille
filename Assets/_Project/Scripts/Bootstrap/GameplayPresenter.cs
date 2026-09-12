using System;
using Zenject;

using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Weapons.Bullets;
using DreamVille.Gameplay.Weapons.Pillow;
using DreamVille.Progression;
using DreamVille.Scoring;
using DreamVille.UI;

namespace DreamVille.Bootstrap
{
    /// <summary>
    /// Level presenter: the only place where the world is wired to the UI.
    /// No system and no view knows about the other directly.
    /// </summary>
    public class GameplayPresenter : IInitializable, IDisposable
    {
        private readonly IScoreService _scoreService;
        private readonly IStarProgressService _starProgressService;
        private readonly IBestScoreService _bestScore;
        private readonly GameUIRoot _ui;
        private readonly IBulletHitSource _bulletHitSource;
        private readonly IPillowThrower _pillowThrower;
        private readonly IEnemyRegistry _enemyRegistry;

        public GameplayPresenter(
            IScoreService scoreService,
            IStarProgressService starProgressService,
            IBestScoreService bestScore,
            GameUIRoot ui,
            IBulletHitSource bulletHitSource,
            IPillowThrower pillowThrower,
            IEnemyRegistry enemyRegistry)
        {
            _scoreService = scoreService;
            _starProgressService = starProgressService;
            _bestScore = bestScore;
            _ui = ui;
            _bulletHitSource = bulletHitSource;
            _pillowThrower = pillowThrower;
            _enemyRegistry = enemyRegistry;
        }

        public void Initialize()
        {
            SubscribeUi();
            SubscribeServices();

            _pillowThrower.Initialize();

            _ui.EnemyCounter.SetEnemiesRemaining(_enemyRegistry.TotalEnemies);
            _enemyRegistry.EnemyKilled += OnEnemyKilled;
        }

        public void Dispose()
        {
            _enemyRegistry.EnemyKilled -= OnEnemyKilled;

            UnsubscribeServices();
            UnsubscribeUi();
        }

        private void SubscribeUi()
        {
            _scoreService.ScoreAdded += _ui.ScorePopup.ShowScore;
            _starProgressService.StarProgressChanged += _ui.StarsUI.SetStarProgress;
            _starProgressService.StarUnlocked += _ui.StarsUI.UnlockStar;

            _pillowThrower.PillowCountChanged += _ui.PillowUI.SpawnIcons;
            _pillowThrower.Thrown += _ui.PillowUI.HidePillow;
            _pillowThrower.Recalled += _ui.PillowUI.ShowPillow;

            _bulletHitSource.BulletHit += _ui.RicochetUI.OnBulletHit;
        }

        private void UnsubscribeUi()
        {
            _scoreService.ScoreAdded -= _ui.ScorePopup.ShowScore;
            _starProgressService.StarProgressChanged -= _ui.StarsUI.SetStarProgress;
            _starProgressService.StarUnlocked -= _ui.StarsUI.UnlockStar;

            _pillowThrower.PillowCountChanged -= _ui.PillowUI.SpawnIcons;
            _pillowThrower.Thrown -= _ui.PillowUI.HidePillow;
            _pillowThrower.Recalled -= _ui.PillowUI.ShowPillow;

            _bulletHitSource.BulletHit -= _ui.RicochetUI.OnBulletHit;
        }

        private void SubscribeServices()
        {
            _scoreService.ScoreAdded += _starProgressService.AddScore;
            _enemyRegistry.AllEnemiesKilled += OnAllEnemiesKilled;
        }

        private void UnsubscribeServices()
        {
            _scoreService.ScoreAdded -= _starProgressService.AddScore;
            _enemyRegistry.AllEnemiesKilled -= OnAllEnemiesKilled;
        }

        /// <summary>Records the level's best score once every enemy is cleared.</summary>
        private void OnAllEnemiesKilled()
        {
            _bestScore.TrySave(_scoreService.TotalScore);
        }

        private void OnEnemyKilled(int remainingEnemies, int collisionsCount)
        {
            _scoreService.HandleEnemyKilled(collisionsCount);
            _ui.EnemyCounter.SetEnemiesRemaining(remainingEnemies);
        }
    }
}
