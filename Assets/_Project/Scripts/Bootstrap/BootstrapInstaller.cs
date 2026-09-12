using Zenject;

using DreamVille.UI;

namespace DreamVille.Bootstrap
{
    /// <summary>Start scene: nothing but the intro and the transition into the game.</summary>
    public class BootstrapInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<GameStartUI>().FromComponentInHierarchy().AsSingle().Lazy();

            Container.BindInterfacesTo<GameBootstrap>().AsSingle();
        }
    }
}
