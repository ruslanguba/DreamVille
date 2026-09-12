using Zenject;

namespace DreamVille.Bootstrap
{
    /// <summary>
    /// Level script. The dependencies themselves are registered by
    /// <see cref="ProgressionInstaller"/>, <see cref="CombatInstaller"/> and <see cref="UiInstaller"/>.
    /// </summary>
    public class GameplayInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<GameplayPresenter>().AsSingle();
        }
    }
}
