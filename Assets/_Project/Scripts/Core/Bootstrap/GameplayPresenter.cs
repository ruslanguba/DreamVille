using System;
using Zenject;

using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Weapons.Bullets;
using DreamVille.Gameplay.Weapons.Pillow;
using DreamVille.Progression;
using DreamVille.Scoring;
using DreamVille.UI;

namespace DreamVille.Core.Bootstrap
{
    /// <summary>
    /// Presenter уровня: единственное место, где мир связывается с интерфейсом.
    /// Ни одна система и ни одна вью не знают друг о друге напрямую.
    /// </summary>
    public class GameplayPresenter : IInitializable, IDisposable
    {
        private readonly ScoreService _scoreService;
        private readonly StarProgressService _starProgressService;
        private readonly BestScoreService _bestScore;
        private readonly GameUIRoot _ui;
        private readonly BulletEventRelay _bulletRelay;
        private readonly PillowThrowHandler _pillowHandler;
        private readonly EnemyRegistry _enemyRegistry;

        public GameplayPresenter(
            ScoreService scoreService,
            StarProgressService starProgressService,
            BestScoreService bestScore,
            GameUIRoot ui,
            BulletEventRelay bulletRelay,
            PillowThrowHandler pillowHandler,
            EnemyRegistry enemyRegistry)
        {
            _scoreService = scoreService;
            _starProgressService = starProgressService;
            _bestScore = bestScore;
            _ui = ui;
            _bulletRelay = bulletRelay;
            _pillowHandler = pillowHandler;
            _enemyRegistry = enemyRegistry;
        }

        public void Initialize()
        {
            SubscribeUi();
            SubscribeServices();

            _pillowHandler.Initialize();

            _ui.EnemyCounter.SetEnemiesRemaining(_enemyRegistry.TotalEnemies);
            _enemyRegistry.OnEnemyKilled += OnEnemyKilled;
        }

        public void Dispose()
        {
            _enemyRegistry.OnEnemyKilled -= OnEnemyKilled;

            UnsubscribeServices();
            UnsubscribeUi();
        }

        private void SubscribeUi()
        {
            _scoreService.OnScoreAdded += _ui.ScorePopup.ShowScore;
            _starProgressService.OnStarProgressChanged += _ui.StarsUI.SetStarProgress;
            _starProgressService.OnStarUnlocked += _ui.StarsUI.UnlockStar;

            _pillowHandler.OnPillowAdded += _ui.PillowUI.SpawnIcons;
            _pillowHandler.OnThrow += _ui.PillowUI.HidePillow;
            _pillowHandler.OnRecall += _ui.PillowUI.ShowPillow;

            _bulletRelay.BulletHit += _ui.RicochetUI.OnBulletHit;
        }

        private void UnsubscribeUi()
        {
            _scoreService.OnScoreAdded -= _ui.ScorePopup.ShowScore;
            _starProgressService.OnStarProgressChanged -= _ui.StarsUI.SetStarProgress;
            _starProgressService.OnStarUnlocked -= _ui.StarsUI.UnlockStar;

            _pillowHandler.OnPillowAdded -= _ui.PillowUI.SpawnIcons;
            _pillowHandler.OnThrow -= _ui.PillowUI.HidePillow;
            _pillowHandler.OnRecall -= _ui.PillowUI.ShowPillow;

            _bulletRelay.BulletHit -= _ui.RicochetUI.OnBulletHit;
        }

        private void SubscribeServices()
        {
            _scoreService.OnScoreAdded += _starProgressService.AddScore;
            _enemyRegistry.OnAllEnemiesKilled += SaveBestScore;
        }

        private void UnsubscribeServices()
        {
            _scoreService.OnScoreAdded -= _starProgressService.AddScore;
            _enemyRegistry.OnAllEnemiesKilled -= SaveBestScore;
        }

        // Восстановленная механика: рекорд очков за уровень (был в ScoreSaveHandler).
        private void SaveBestScore()
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
