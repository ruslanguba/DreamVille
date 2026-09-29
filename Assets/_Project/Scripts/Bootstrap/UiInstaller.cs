using UnityEngine;
using Zenject;

using DreamVille.UI.Hud;

namespace DreamVille.Bootstrap
{
    /// <summary>Gameplay HUD. The container instantiates the prefab and exposes it as <see cref="IGameHudView"/>.</summary>
    public class UiInstaller : MonoInstaller
    {
        [Header("Prefabs")]
        [SerializeField] private GameUIRoot _uiPrefab;

        public override void InstallBindings()
        {
            Container.Bind<IGameHudView>().To<GameUIRoot>().FromComponentInNewPrefab(_uiPrefab).AsSingle();
        }
    }
}
