using UnityEngine;
using Zenject;

using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Enemies.Combat;
using DreamVille.Gameplay.Weapons.Bullets;

namespace DreamVille.Bootstrap
{
    /// <summary>
    /// Combat systems. The bullet pool, spawner, event relay and enemy projectile pool are
    /// created from prefabs, so they do not require objects pre-placed in the scene.
    /// </summary>
    public class CombatInstaller : MonoInstaller
    {
        [Header("Prefabs")]
        [SerializeField] private BulletSpawner _gameplaySystemsPrefab;
        [SerializeField] private EnemyProjectile _enemyProjectilePrefab;

        public override void InstallBindings()
        {
            // One prefab instance per scene: the remaining components are read off it,
            // otherwise FromComponentInNewPrefab would create a separate copy per binding.
            Container
                .BindInterfacesAndSelfTo<BulletSpawner>()
                .FromComponentInNewPrefab(_gameplaySystemsPrefab)
                .AsSingle()
                .NonLazy();

            Container
                .Bind<BulletPool>()
                .FromMethod(ctx => ctx.Container.Resolve<BulletSpawner>().Pool)
                .AsSingle();

            Container
                .Bind<IBulletHitSource>()
                .FromMethod(ctx => ctx.Container.Resolve<BulletSpawner>().HitRelay)
                .AsSingle();

            Container
                .BindMemoryPool<EnemyProjectile, EnemyProjectile.Pool>()
                .WithInitialSize(8)
                .FromComponentInNewPrefab(_enemyProjectilePrefab)
                .UnderTransformGroup("EnemyProjectiles");

            // Lives on the LevelSystems prefab in Prefabs/Utilities.
            Container.BindInterfacesTo<EnemyRegistry>().FromComponentInHierarchy().AsSingle();
        }
    }
}
