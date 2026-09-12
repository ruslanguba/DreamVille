using UnityEngine;

using DreamVille.Configs;

namespace DreamVille.Gameplay.Weapons.Bullets
{
    /// <summary>
    /// Facade over <see cref="BulletPool"/> and <see cref="BulletEventRelay"/>: fetches a
    /// bullet, places and initializes it, and registers it for hit relaying.
    /// </summary>
    public class BulletSpawner : MonoBehaviour, IBulletSpawner
    {
        [SerializeField] private BulletPool _pool;
        [SerializeField] private BulletEventRelay _eventRelay;

        /// <summary>The pool this spawner draws bullets from. Exposed so the installer binds the same instance.</summary>
        public BulletPool Pool => _pool;

        /// <summary>The relay every spawned bullet reports its hits to.</summary>
        public BulletEventRelay HitRelay => _eventRelay;

        /// <summary>Gets a bullet from the pool, positions it and arms it with <paramref name="bulletConfig"/>.</summary>
        public Bullet SpawnBullet(Vector3 position, Quaternion rotation, BulletConfig bulletConfig)
        {
            Bullet bullet = _pool.Get();
            bullet.transform.SetPositionAndRotation(position, rotation);

            bullet.Init(bulletConfig);
            _eventRelay.Register(bullet);
            return bullet;
        }
    }
}
