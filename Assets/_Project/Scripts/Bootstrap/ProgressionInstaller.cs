using UnityEngine;
using Zenject;

using DreamVille.Configs;
using DreamVille.Progression;
using DreamVille.Scoring;

namespace DreamVille.Bootstrap
{
    /// <summary>Score, stars and level completion. The level itself comes from the catalog.</summary>
    public class ProgressionInstaller : MonoInstaller
    {
        [Header("Configs")]
        [SerializeField] private SceneCatalog _catalog;
        [SerializeField] private ScoreRules _scoreRules;

        public override void InstallBindings()
        {
            var level = _catalog.FindBySceneName(gameObject.scene.name) as LevelDefinition;

            if (level == null)
            {
                Debug.LogError($"[{nameof(ProgressionInstaller)}] Scene '{gameObject.scene.name}' has no LevelDefinition in the catalog.", this);
                return;
            }

            Container.BindInstance(level).AsSingle();
            Container.BindInstance(level.StarRules).AsSingle();
            Container.BindInstance(_scoreRules).AsSingle();

            Container.BindInterfacesTo<ScoreService>().AsSingle();
            Container.BindInterfacesTo<StarProgressService>().AsSingle();
            Container.BindInterfacesTo<BestScoreService>().AsSingle();

            Container.Bind<LevelActors>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesTo<LevelDirector>().AsSingle();
        }
    }
}
