using UnityEngine;
using Zenject;

using DreamVille.UI;

namespace DreamVille.Core.Bootstrap
{
    /// <summary>Стартовая сцена: только заставка и переход в игру.</summary>
    public class BootstrapInstaller : MonoInstaller
    {
        [SerializeField] private string _firstScene = "Main menu 1";

        public override void InstallBindings()
        {
            Container.Bind<GameStartUI>().FromComponentInHierarchy().AsSingle().Lazy();

            Container
                .BindInterfacesTo<GameBootstrap>()
                .AsSingle()
                .WithArguments(_firstScene);
        }
    }
}
