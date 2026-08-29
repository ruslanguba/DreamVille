using Zenject;

using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Weapons.Bullets;
using DreamVille.Gameplay.Weapons.Pillow;

namespace DreamVille.Core.Bootstrap
{
    /// <summary>Боевые системы сцены. Компоненты ищутся в иерархии, а не назначаются вручную.</summary>
    public class CombatInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<BulletEventRelay>().FromComponentInHierarchy().AsSingle();
            Container.Bind<BulletSpawner>().FromComponentInHierarchy().AsSingle();
            Container.Bind<EnemyRegistry>().FromComponentInHierarchy().AsSingle();
            Container.Bind<PillowThrowHandler>().FromComponentInHierarchy().AsSingle();
        }
    }
}
