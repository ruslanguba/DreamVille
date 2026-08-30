using UnityEngine;
using Zenject;

using DreamVille.Audio;
using DreamVille.Core.SceneManagement;

namespace DreamVille.Core.Bootstrap
{
    /// <summary>
    /// Сервисы уровня приложения: живут между загрузками сцен.
    /// Висит на ProjectContext, поэтому DontDestroyOnLoad вручную не нужен.
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

            Container.Bind<SceneLoader>().AsSingle();
        }
    }
}
