using UnityEngine;
using Zenject;

using DreamVille.Configs;
using DreamVille.Progression;
using DreamVille.Scoring;

namespace DreamVille.Bootstrap
{
    /// <summary>Score, stars and level completion.</summary>
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
