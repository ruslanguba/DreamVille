using UnityEngine;
using Zenject;
using DreamVille.UI;

namespace DreamVille.Core.Bootstrap
{
    /// <summary>Игровой HUD. Префаб инстанцирует контейнер.</summary>
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
