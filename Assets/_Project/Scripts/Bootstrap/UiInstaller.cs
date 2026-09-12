using UnityEngine;
using Zenject;
using DreamVille.UI;

namespace DreamVille.Bootstrap
{
    /// <summary>Gameplay HUD. The container instantiates the prefab.</summary>
    public class UiInstaller : MonoInstaller
    {
        [Header("Prefabs")]
        [SerializeField] private GameUIRoot _uiPrefab;

        public override void InstallBindings()
        {
            Container.Bind<GameUIRoot>().FromComponentInNewPrefab(_uiPrefab).AsSingle();
        }
    }
}
