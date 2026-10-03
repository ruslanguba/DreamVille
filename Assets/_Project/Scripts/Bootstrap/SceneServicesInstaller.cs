using Zenject;

using DreamVille.App;
using DreamVille.Core.Players;
using DreamVille.Dialogue;
using DreamVille.Gameplay.Damage;
using DreamVille.Gameplay.Player;
using DreamVille.Gameplay.Weapons.Pillow;
using DreamVille.Input;

namespace DreamVille.Bootstrap
{
    /// <summary>
    /// Scene-scoped services: they live exactly as long as the scene.
    /// Required in every scene that has a player or dialogues.
    /// </summary>
    public class SceneServicesInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<PlayerCharacter>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesTo<PlayerInputSwitch>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesTo<PillowLauncher>().FromComponentInHierarchy().AsSingle();
            // Lazy: only a DamageVignetteView resolves it, and only scenes with a Volume have one.
            Container.Bind<IPlayerDamageState>().To<PlayerDamageReceiver>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesTo<GameInput>().FromComponentInHierarchy().AsSingle();

            Container.BindInterfacesTo<UnityLocalizedTextProvider>().AsSingle();
            Container.BindInterfacesTo<DialogueService>().AsSingle();
            Container.Bind<DialogueManager>().FromComponentInHierarchy().AsSingle();

            Container.BindInterfacesTo<PauseService>().AsSingle();
            Container.BindInterfacesTo<GameFlowService>().AsSingle();
            Container.BindInterfacesTo<InGameMenuController>().AsSingle().NonLazy();
        }
    }
}
