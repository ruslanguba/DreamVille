using System;
using UnityEngine;
using Zenject;

using DreamVille.Configs;
using DreamVille.Progression;
using DreamVille.Progression.Pillows;
using DreamVille.Scoring;

namespace DreamVille.Bootstrap
{
    /// <summary>Score, stars, level completion and pillow unlocks. The level itself comes from the catalog.</summary>
    public class ProgressionInstaller : MonoInstaller
    {
        [Header("Configs")]
        [SerializeField] private SceneCatalog _catalog;
        [SerializeField] private ScoreRules _scoreRules;

        public override void InstallBindings()
        {
            string sceneName = gameObject.scene.name;

            // Fail fast: a gameplay scene missing from the catalog is a content error, and a half-installed
            // scene would only surface it later as unrelated resolve failures.
            if (_catalog.FindBySceneName(sceneName) is not LevelDefinition level)
                throw new InvalidOperationException($"[{nameof(ProgressionInstaller)}] Scene '{sceneName}' has no LevelDefinition in the catalog.");

            Container.BindInstance(level).AsSingle();
            Container.BindInstance(level.StarRules).AsSingle();
            Container.BindInstance(_scoreRules).AsSingle();

            Container.BindInterfacesTo<ScoreService>().AsSingle();
            Container.BindInterfacesTo<StarProgressService>().AsSingle();
            Container.BindInterfacesTo<BestScoreService>().AsSingle();
            Container.BindInterfacesTo<ScoringCoordinator>().AsSingle();

            // Scene-scoped: availability is capped by this scene's LevelDefinition; the unlock count itself is persisted.
            Container.BindInterfacesTo<PillowInventory>().AsSingle();

            Container.Bind<ILevelCast>().To<LevelActors>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesTo<LevelDirector>().AsSingle();
        }
    }
}
