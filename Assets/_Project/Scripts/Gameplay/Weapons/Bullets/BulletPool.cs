using System.Collections.Generic;
using UnityEngine;

namespace DreamVille.Gameplay.Weapons.Bullets
{
    /// <summary>
    /// Queue-based object pool for <see cref="Bullet"/> instances. Lazily instantiates
    /// new bullets as demand exceeds supply.
    /// </summary>
    public class BulletPool : MonoBehaviour
    {
        [SerializeField] private Bullet _prefab;

        private readonly Queue<Bullet> _pool = new();
        private readonly HashSet<Bullet> _pooled = new();

        /// <summary>Dequeues a bullet and activates it.</summary>
        public Bullet Get()
        {
            if (_pool.Count == 0)
                Create();

            Bullet bullet = _pool.Dequeue();
            _pooled.Remove(bullet);
            bullet.gameObject.SetActive(true);
            return bullet;
        }

        /// <summary>
        /// Deactivates <paramref name="bullet"/> and enqueues it. Idempotent: a bullet that is
        /// already pooled is ignored, so a stray double-return can never duplicate a queue entry.
        /// </summary>
        public void Return(Bullet bullet)
        {
            if (!_pooled.Add(bullet))
                return;

            bullet.gameObject.SetActive(false);
            _pool.Enqueue(bullet);
        }

        private void Create()
        {
            var bullet = Instantiate(_prefab);
            bullet.SetPool(this);
            bullet.gameObject.SetActive(false);
            _pooled.Add(bullet);
            _pool.Enqueue(bullet);
        }
    }
}
