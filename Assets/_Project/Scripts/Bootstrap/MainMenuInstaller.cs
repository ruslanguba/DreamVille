using Zenject;

using DreamVille.UI.Menus;

namespace DreamVille.Bootstrap
{
    /// <summary>
    /// The main menu's views and presenters: menu commands, level select and audio settings.
    /// The views are scene components; the presenters are plain classes the container drives.
    /// </summary>
    public sealed class MainMenuInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IMainMenuView>().To<MainMenuView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<ILevelSelectView>().To<LevelSelectView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<IAudioSettingsView>().To<AudioSettingsView>().FromComponentInHierarchy().AsSingle();

            Container.BindInterfacesTo<LocalizationLocaleSelector>().AsSingle();

            Container.BindInterfacesTo<MainMenuPresenter>().AsSingle();
            Container.BindInterfacesTo<LevelSelectPresenter>().AsSingle();
            Container.BindInterfacesTo<AudioSettingsPresenter>().AsSingle();
        }
    }
}
