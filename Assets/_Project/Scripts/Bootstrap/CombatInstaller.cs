using UnityEngine;
using Zenject;

using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Weapons.Bullets;
using DreamVille.Gameplay.Weapons.Pillow;

namespace DreamVille.Bootstrap
{
    /// <summary>
    /// Combat systems. The bullet pool, spawner and event relay are created from a
    /// prefab, so they do not require objects pre-placed in the scene.
    /// </summary>
    public class CombatInstaller : MonoInstaller
    {
        [Header("Prefabs")]
        [SerializeField] private BulletSpawner _gameplaySystemsPrefab;

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

            // These live on scene objects: the registry on the LevelProgressSystem
            // prefab in Prefabs/Utilities, pillows on the player.
            Container.BindInterfacesTo<EnemyRegistry>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesTo<PillowThrowHandler>().FromComponentInHierarchy().AsSingle();
        }
    }
}
