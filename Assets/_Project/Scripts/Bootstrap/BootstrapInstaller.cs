using UnityEngine;
using Zenject;

using DreamVille.UI;

namespace DreamVille.Bootstrap
{
    /// <summary>Start scene: nothing but the intro and the transition into the game.</summary>
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
