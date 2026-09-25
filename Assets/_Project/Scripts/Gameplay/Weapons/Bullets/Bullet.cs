using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

using DreamVille.Configs;

namespace DreamVille.Gameplay.Weapons.Bullets
{
    /// <summary>
    /// Pooled projectile. Flies until it either exhausts <see cref="BulletConfig.MaxCollisionCount"/>
    /// collisions or its lifetime expires, then returns itself to the owning <see cref="BulletPool"/>.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class Bullet : MonoBehaviour
    {
        /// <summary>Raised on every collision while the bullet is in flight.</summary>
        public event Action<BulletHitInfo> Hit;

        /// <summary>Raised once, right before the bullet is handed back to the pool.</summary>
        public event Action<Bullet> ReturnedToPool;

        private BulletPool _pool;
        private Rigidbody _rb;
        private SphereCollider _collider;
        private int _hitCount;
        private BulletConfig _config;
        private bool _isInFlight;
        private CancellationTokenSource _lifetimeCts;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<SphereCollider>();
        }

        private void OnDisable()
        {
            // The pool deactivates returned bullets; any other disable path must also end the flight.
            _isInFlight = false;
            CancelLifetime();
        }

        private void OnDestroy()
        {
            CancelLifetime();
        }

        /// <summary>Binds the pool this bullet must return itself to.</summary>
        public void SetPool(BulletPool pool)
        {
            _pool = pool;
        }

        /// <summary>
        /// Prepares a freshly pooled bullet for a new shot. The collider takes the config's radius, the one the aim
        /// preview sweeps. The lifetime starts in <see cref="Launch"/>.
        /// </summary>
        public void Init(BulletConfig config)
        {
            _config = config;
            _collider.radius = config.CollisionRadius;
            _hitCount = 0;
            _isInFlight = true;
        }

        /// <summary>Sets the bullet in motion along <paramref name="direction"/> and arms its lifetime.</summary>
        public void Launch(Vector3 direction)
        {
            _rb.linearVelocity = direction.normalized * _config.Speed;
            _rb.angularVelocity = Vector3.zero;
            RestartLifetime();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_isInFlight)
                return;

            _hitCount++;
            Hit?.Invoke(new BulletHitInfo
            {
                HitPoint = collision.GetContact(0).point,
                Target = collision.gameObject,
                RicochetCount = _hitCount,
                Damage = _config.Damage
            });

            if (_hitCount >= _config.MaxCollisionCount)
                ReturnToPool();
        }

        private void ReturnToPool()
        {
            if (!_isInFlight)
                return;

            _isInFlight = false;
            CancelLifetime();
            ReturnedToPool?.Invoke(this);
            _pool.Return(this);
        }

        private void RestartLifetime()
        {
            CancelLifetime();
            _lifetimeCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            ExpireAsync(_config.LifeTime, _lifetimeCts.Token).Forget();
        }

        private void CancelLifetime()
        {
            if (_lifetimeCts == null)
                return;

            _lifetimeCts.Cancel();
            _lifetimeCts.Dispose();
            _lifetimeCts = null;
        }

        private async UniTaskVoid ExpireAsync(float lifeTime, CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(lifeTime), cancellationToken: token);
                ReturnToPool();
            }
            catch (OperationCanceledException)
            {
                // Relaunched, returned to the pool, or destroyed before the lifetime expired.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }
}
