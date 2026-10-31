using UnityEngine;
using Zenject;

using DreamVille.App;
using DreamVille.Audio;
using DreamVille.Configs;
using DreamVille.Persistence;
using DreamVille.SceneManagement;

namespace DreamVille.Bootstrap
{
    /// <summary>
    /// Application-scoped services: they survive scene loads.
    /// Installed on the ProjectContext, so no manual DontDestroyOnLoad is needed.
    /// </summary>
    public class ProjectInstaller : MonoInstaller
    {
        [Header("Configs")]
        [SerializeField] private SceneCatalog _sceneCatalog;
        [SerializeField] private AudioConfig _audioConfig;

        [Header("Prefabs")]
        [SerializeField] private AudioPlayer _audioPlayerPrefab;

        public override void InstallBindings()
        {
            Container.BindInstance(_sceneCatalog);
            Container.BindInstance(_audioConfig);

            Container
                .BindInterfacesTo<AudioPlayer>()
                .FromComponentInNewPrefab(_audioPlayerPrefab)
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesTo<AudioService>().AsSingle().NonLazy();

            Container.Bind<ISceneLoadingBackend>().To<UnitySceneLoadingBackend>().AsSingle();
            Container.BindInterfacesTo<SceneLoader>().AsSingle();
            Container.BindInterfacesTo<UnityActiveSceneTracker>().AsSingle();

            Container.BindInterfacesTo<PlayerPrefsSaveStorage>().AsSingle();
            Container.BindInterfacesTo<ProgressRepository>().AsSingle();
            Container.BindInterfacesTo<AudioSettingsRepository>().AsSingle();

            Container.BindInterfacesTo<CursorService>().AsSingle();
            Container.BindInterfacesTo<UnityApplicationQuitter>().AsSingle();
            Container.BindInterfacesTo<FrameRateLimiter>().AsSingle();
        }
    }
}
