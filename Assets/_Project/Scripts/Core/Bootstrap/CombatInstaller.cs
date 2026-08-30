using UnityEngine;
using Zenject;

using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Weapons.Bullets;
using DreamVille.Gameplay.Weapons.Pillow;

namespace DreamVille.Core.Bootstrap
{
    /// <summary>
    /// Боевые системы. Пул пуль, спавнер и релей событий создаются из префаба,
    /// поэтому не требуют заранее разложенных по сценам объектов.
    /// </summary>
    public class CombatInstaller : MonoInstaller
    {
        [Header("Prefabs")]
        [SerializeField] private BulletSpawner _gameplaySystemsPrefab;

        public override void InstallBindings()
        {
            // Один экземпляр префаба на сцену: остальные компоненты берём с него же,
            // иначе FromComponentInNewPrefab создал бы по копии на каждый биндинг.
            Container
                .Bind<BulletSpawner>()
                .FromComponentInNewPrefab(_gameplaySystemsPrefab)
                .AsSingle()
                .NonLazy();

            Container
                .Bind<BulletPool>()
                .FromMethod(ctx => ctx.Container.Resolve<BulletSpawner>().GetComponent<BulletPool>())
                .AsSingle();

            Container
                .Bind<BulletEventRelay>()
                .FromMethod(ctx => ctx.Container.Resolve<BulletSpawner>().GetComponent<BulletEventRelay>())
                .AsSingle();

            // Эти живут на объектах сцены: реестр — на LevelProgressSystem, подушки — на игроке.
            Container.Bind<EnemyRegistry>().FromComponentInHierarchy().AsSingle();
            Container.Bind<PillowThrowHandler>().FromComponentInHierarchy().AsSingle();
        }
    }
}
