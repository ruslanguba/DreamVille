using UnityEngine;
using Zenject;

using DreamVille.UI.Hud;

namespace DreamVille.Bootstrap
{
    /// <summary>
    /// Gameplay HUD: the container instantiates the prefab, exposes it as <see cref="IGameHudView"/>
    /// and binds the <see cref="HudPresenter"/> that drives it.
    /// </summary>
    public class UiInstaller : MonoInstaller
    {
        [Header("Prefabs")]
        [SerializeField] private GameUIRoot _uiPrefab;

        public override void InstallBindings()
        {
            Container.Bind<IGameHudView>().To<GameUIRoot>().FromComponentInNewPrefab(_uiPrefab).AsSingle();
            Container.BindInterfacesTo<HudPresenter>().AsSingle();
        }
    }
}
