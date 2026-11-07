using UnityEngine;

using DreamVille.Configs;

namespace DreamVille.Gameplay.Weapons.Bullets
{
    /// <summary>
    /// Facade over <see cref="BulletPool"/> and <see cref="BulletEventRelay"/>: fetches a
    /// bullet, places and initializes it, and registers it for hit relaying.
    /// </summary>
    public sealed class BulletSpawner : MonoBehaviour, IBulletSpawner
    {
        [SerializeField] private BulletPool _pool;
        [SerializeField] private BulletEventRelay _eventRelay;

        /// <summary>The relay every spawned bullet reports its hits to.</summary>
        public BulletEventRelay HitRelay => _eventRelay;

        /// <inheritdoc />
        public void Fire(in ShotRay shot, BulletConfig config)
        {
            Bullet bullet = _pool.Get();
            bullet.transform.SetPositionAndRotation(shot.Origin, shot.Rotation);

            bullet.Init(config);
            _eventRelay.Register(bullet);
            bullet.Launch(shot.Direction);
        }
    }
}
