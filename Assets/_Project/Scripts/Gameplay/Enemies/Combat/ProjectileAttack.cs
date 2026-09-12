using UnityEngine;

using DreamVille.Configs.Enemies;

namespace DreamVille.Gameplay.Enemies.Combat
{
    /// <summary>Throws pooled <see cref="EnemyProjectile"/>s from a spawn point on the enemy.</summary>
    public sealed class ProjectileAttack : IEnemyAttack
    {
        private readonly EnemyProjectile.Pool _pool;
        private readonly Transform _origin;
        private readonly EnemyConfig _config;

        public ProjectileAttack(EnemyProjectile.Pool pool, Transform origin, EnemyConfig config)
        {
            _pool = pool;
            _origin = origin;
            _config = config;
        }

        public Vector3 Origin => _origin.position;

        public void Throw(Vector3 direction)
            => _pool.Spawn(new EnemyProjectileLaunch(_origin.position, direction, _config.ThrowImpulse, _config.ProjectileLifetime));
    }
}
