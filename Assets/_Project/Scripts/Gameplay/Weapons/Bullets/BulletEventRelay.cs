using System;
using System.Collections.Generic;
using UnityEngine;

namespace DreamVille.Gameplay.Weapons.Bullets
{
    /// <summary>
    /// Collapses the <see cref="Bullet.Hit"/> event of every bullet spawned this scene into
    /// a single <see cref="BulletHit"/> event for UI consumers.
    /// </summary>
    public sealed class BulletEventRelay : MonoBehaviour, IBulletHitSource
    {
        /// <summary>Raised whenever any registered bullet reports a collision.</summary>
        public event Action<BulletHitInfo> BulletHit;

        private readonly HashSet<Bullet> _registered = new();

        /// <summary>
        /// Subscribes to a freshly spawned bullet. Idempotent: a bullet already registered
        /// (for example a stray double-spawn from the pool) is not subscribed twice.
        /// </summary>
        public void Register(Bullet bullet)
        {
            if (!_registered.Add(bullet))
                return;

            bullet.Hit += OnBulletHit;
            bullet.ReturnedToPool += Unregister;
        }

        private void OnBulletHit(BulletHitInfo info)
        {
            BulletHit?.Invoke(info);
        }

        private void Unregister(Bullet bullet)
        {
            if (!_registered.Remove(bullet))
                return;

            bullet.Hit -= OnBulletHit;
            bullet.ReturnedToPool -= Unregister;
        }
    }
}
