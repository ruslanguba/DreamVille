using System;
using Zenject;

using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Weapons.Bullets;
using DreamVille.Gameplay.Weapons.Pillow;
using DreamVille.Progression;
using DreamVille.Scoring;

namespace DreamVille.Core.Bootstrap
{
    /// <summary>
    /// Сценарий игрового уровня: связывает сервисы, боевые системы и UI.
    /// В отличие от прежнего GameplaySceneEntryPoint снимает все подписки в Dispose.
    /// </summary>
    public class GameplayFlow : IInitializable, IDisposable
    {
        private readonly ScoreService _scoreService;
        private readonly StarProgressService _starProgressService;
        private readonly GameUIRoot _ui;
        private readonly BulletEventRelay _bulletRelay;
        private readonly PillowThrowHandler _pillowHandler;
        private readonly EnemyRegistry _enemyRegistry;
        private readonly LevelCompletionFlow _levelCompletionFlow;

        public GameplayFlow(
            ScoreService scoreService,
            StarProgressService starProgressService,
            GameUIRoot ui,
            BulletEventRelay bulletRelay,
            PillowThrowHandler pillowHandler,
            EnemyRegistry enemyRegistry,
            LevelCompletionFlow levelCompletionFlow)
        {
            _scoreService = scoreService;
            _starProgressService = starProgressService;
            _ui = ui;
            _bulletRelay = bulletRelay;
            _pillowHandler = pillowHandler;
            _enemyRegistry = enemyRegistry;
            _levelCompletionFlow = levelCompletionFlow;
        }

        public void Initialize()
        {
            SubscribeUi();
            SubscribeServices();

            _pillowHandler.Initialize();

            _enemyRegistry.Initialize();
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
            _enemyRegistry.OnAllEnemiesKilled += _levelCompletionFlow.LevelComplete;
        }

        private void UnsubscribeServices()
        {
            _scoreService.OnScoreAdded -= _starProgressService.AddScore;
            _enemyRegistry.OnAllEnemiesKilled -= _levelCompletionFlow.LevelComplete;
        }

        private void OnEnemyKilled(int remainingEnemies, int collisionsCount)
        {
            _scoreService.HandleEnemyKilled(collisionsCount);
            _ui.EnemyCounter.SetEnemiesRemaining(remainingEnemies);
        }
    }
}
