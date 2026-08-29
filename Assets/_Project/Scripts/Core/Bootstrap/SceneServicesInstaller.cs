using Zenject;

using DreamVille.Dialogue;
using DreamVille.Gameplay.Player;

namespace DreamVille.Core.Bootstrap
{
    /// <summary>
    /// Менеджеры уровня сцены: живут ровно столько, сколько сцена.
    /// Нужен в каждой сцене, где есть игрок или диалоги.
    /// </summary>
    public class SceneServicesInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<PlayerContext>().FromComponentInHierarchy().AsSingle();
            Container.Bind<PlayerInputSwitch>().FromComponentInHierarchy().AsSingle();
            Container.Bind<DialogueService>().AsSingle();
            Container.Bind<DialogueManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<GameManager>().FromComponentInHierarchy().AsSingle();
        }
    }
}
