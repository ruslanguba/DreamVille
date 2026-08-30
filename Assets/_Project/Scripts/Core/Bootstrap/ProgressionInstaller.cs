using UnityEngine;
using Zenject;

using DreamVille.Configs;
using DreamVille.Progression;
using DreamVille.Scoring;

namespace DreamVille.Core.Bootstrap
{
    /// <summary>Очки, звёзды и завершение уровня.</summary>
    public class ProgressionInstaller : MonoInstaller
    {
        [Header("Configs")]
        [SerializeField] private ScoreRulesSO _scoreRules;
        [SerializeField] private StarRulesSO _starRules;

        public override void InstallBindings()
        {
            Container.BindInstance(_scoreRules).AsSingle();
            Container.BindInstance(_starRules).AsSingle();

            Container.Bind<ScoreService>().AsSingle();
            Container.Bind<StarProgressService>().AsSingle();
            Container.Bind<BestScoreService>().AsSingle();

            Container.Bind<LevelUnlockService>().AsSingle();
            Container.Bind<LevelActors>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesTo<LevelDirector>().AsSingle();
        }
    }
}
