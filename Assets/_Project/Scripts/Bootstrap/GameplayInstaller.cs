using Zenject;

using DreamVille.UI.Hud;

namespace DreamVille.Bootstrap
{
    /// <summary>
    /// Binds the HUD presenter. Its dependencies are registered by <see cref="ProgressionInstaller"/>,
    /// <see cref="CombatInstaller"/>, <see cref="SceneServicesInstaller"/> and <see cref="UiInstaller"/>.
    /// </summary>
    public class GameplayInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<HudPresenter>().AsSingle();
        }
    }
}
