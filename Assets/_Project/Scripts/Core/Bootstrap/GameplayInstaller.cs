using UnityEngine;
using Zenject;

using DreamVille.Configs;
using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Weapons.Bullets;
using DreamVille.Gameplay.Weapons.Pillow;
using DreamVille.Progression;

namespace DreamVille.Core.Bootstrap
{
    /// <summary>
    /// Composition root игрового уровня. Только регистрация зависимостей;
    /// сценарий их взаимодействия живёт в <see cref="GameplayFlow"/>.
    /// </summary>
    public class GameplayInstaller : MonoInstaller
    {
        [Header("Configs")]
        [SerializeField] private ScoreRulesSO _scoreRules;
        [SerializeField] private StarRulesSO _starRules;

        [Header("Prefabs")]
        [SerializeField] private GameUIRoot _uiPrefab;

        [Header("Scene systems")]
        [SerializeField] private BulletEventRelay _bulletRelay;
        [SerializeField] private PillowThrowHandler _pillowHandler;
        [SerializeField] private EnemyRegistry _enemyRegistry;
        [SerializeField] private LevelCompletionFlow _levelCompletionFlow;

        public override void InstallBindings()
        {
            BindConfigs();
            BindServices();
            BindSceneSystems();

            Container.BindInterfacesTo<GameplayFlow>().AsSingle();
        }

        private void BindConfigs()
        {
            Container.BindInstance(_scoreRules).AsSingle();
            Container.BindInstance(_starRules).AsSingle();
        }

        private void BindServices()
        {
            Container.Bind<ScoreService>().AsSingle();
            Container.Bind<StarProgressService>().AsSingle();
        }

        private void BindSceneSystems()
        {
            Container.Bind<GameUIRoot>().FromComponentInNewPrefab(_uiPrefab).AsSingle();

            Container.BindInstance(_bulletRelay).AsSingle();
            Container.BindInstance(_pillowHandler).AsSingle();
            Container.BindInstance(_enemyRegistry).AsSingle();
            Container.BindInstance(_levelCompletionFlow).AsSingle();
        }
    }
}
