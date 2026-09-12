using UnityEngine;
using Zenject;

using DreamVille.Audio;
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
        [Header("Prefabs")]
        [SerializeField] private AudioManager _audioManagerPrefab;

        public override void InstallBindings()
        {
            Container
                .Bind<AudioManager>()
                .FromComponentInNewPrefab(_audioManagerPrefab)
                .AsSingle()
                .NonLazy();

            Container.Bind<ISceneLoadingBackend>().To<UnitySceneLoadingBackend>().AsSingle();
            Container.BindInterfacesTo<SceneLoader>().AsSingle();

            Container.BindInterfacesTo<PlayerPrefsSaveStorage>().AsSingle();
            Container.BindInterfacesTo<ProgressRepository>().AsSingle();
            Container.BindInterfacesTo<AudioSettingsRepository>().AsSingle();
        }
    }
}
