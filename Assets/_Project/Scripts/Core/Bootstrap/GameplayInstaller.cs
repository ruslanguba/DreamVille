using Zenject;

namespace DreamVille.Core.Bootstrap
{
    /// <summary>
    /// Сценарий уровня. Сами зависимости регистрируют
    /// <see cref="ProgressionInstaller"/>, <see cref="CombatInstaller"/> и <see cref="UiInstaller"/>.
    /// </summary>
    public class GameplayInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<GameplayFlow>().AsSingle();
        }
    }
}
