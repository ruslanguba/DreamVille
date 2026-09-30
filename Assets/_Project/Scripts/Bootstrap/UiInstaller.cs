using UnityEngine;
using Zenject;

using DreamVille.UI.Hud;
using DreamVille.UI.Menus;

namespace DreamVille.Bootstrap
{
    /// <summary>
    /// The gameplay scene's UI: the HUD (the container instantiates the prefab, exposes it as
    /// <see cref="IGameHudView"/> and binds the <see cref="HudPresenter"/> that drives it), the pause menu
    /// and its audio settings, whose views live in the scene's pause menu.
    /// </summary>
    public class UiInstaller : MonoInstaller
    {
        [Header("Prefabs")]
        [SerializeField] private GameUIRoot _uiPrefab;

        public override void InstallBindings()
        {
            Container.Bind<IGameHudView>().To<GameUIRoot>().FromComponentInNewPrefab(_uiPrefab).AsSingle();
            Container.BindInterfacesTo<HudPresenter>().AsSingle();

            Container.Bind<IPauseMenuView>().To<PauseMenuView>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesTo<PauseMenuPresenter>().AsSingle();

            Container.Bind<IAudioSettingsView>().To<AudioSettingsView>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesTo<AudioSettingsPresenter>().AsSingle();
        }
    }
}
