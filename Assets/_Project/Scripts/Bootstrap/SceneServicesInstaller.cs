using UnityEngine;
using Zenject;

using DreamVille.App;
using DreamVille.Dialogue;
using DreamVille.Gameplay.Player;
using DreamVille.Input;

namespace DreamVille.Bootstrap
{
    /// <summary>
    /// Scene-scoped services: they live exactly as long as the scene.
    /// Required in every scene that has a player or dialogues.
    /// </summary>
    public class SceneServicesInstaller : MonoInstaller
    {
        /// <remarks>Replaced by a lookup in SceneCatalog once the catalog exists.</remarks>
        [SerializeField] private string _mainMenuScene = "Main menu 1";

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PlayerContext>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesTo<PlayerInputSwitch>().FromComponentInHierarchy().AsSingle();
            Container.Bind<GameInput>().FromComponentInHierarchy().AsSingle();

            Container.BindInterfacesTo<DialogueService>().AsSingle();
            Container.Bind<DialogueManager>().FromComponentInHierarchy().AsSingle();

            Container.BindInterfacesTo<CursorService>().AsSingle();
            Container.BindInterfacesTo<PauseService>().AsSingle();
            Container.BindInterfacesTo<GameFlowService>().AsSingle().WithArguments(_mainMenuScene);
            Container.BindInterfacesTo<InGameMenuController>().AsSingle().NonLazy();
        }
    }
}
