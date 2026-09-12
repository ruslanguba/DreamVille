using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

using DreamVille.Gameplay.Damage;

namespace DreamVille.Gameplay.Enemies.Combat
{
    /// <summary>
    /// Pooled projectile an enemy throws at the player. It knocks the player back on contact and
    /// returns itself to the pool on that hit or when its lifetime expires, whichever comes first.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class EnemyProjectile : MonoBehaviour
    {
        [FormerlySerializedAs("knockbackForce")]
        [SerializeField] private float _knockbackForce = 6f;

        private Rigidbody _body;
        private Pool _pool;
        private bool _isInFlight;
        private CancellationTokenSource _lifetimeCts;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
        }

        private void OnDisable()
        {
            // The pool deactivates despawned projectiles; any other disable path must also end the flight.
            _isInFlight = false;
            CancelLifetime();
        }

        private void OnDestroy()
        {
            CancelLifetime();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isInFlight || !other.TryGetComponent(out PlayerDamageReceiver player))
                return;

            Vector3 knockDirection = (other.transform.position - transform.position).normalized;
            player.TakeDamage(knockDirection, _knockbackForce);
            Despawn();
        }

        private void Launch(EnemyProjectileLaunch launch, Pool pool)
        {
            _pool = pool;
            _isInFlight = true;

            // Teleport through the body, not the transform: the physics state must not lag a frame behind.
            _body.position = launch.Position;
            _body.rotation = Quaternion.identity;
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
            _body.AddForce(launch.Direction * launch.Impulse, ForceMode.Impulse);

            RestartLifetime(launch.Lifetime);
        }

        private void Despawn()
        {
            if (!_isInFlight)
                return;

            _isInFlight = false;
            CancelLifetime();
            _pool.Despawn(this);
        }

        private void RestartLifetime(float lifetime)
        {
            CancelLifetime();
            _lifetimeCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            ExpireAsync(lifetime, _lifetimeCts.Token).Forget();
        }

        private void CancelLifetime()
        {
            if (_lifetimeCts == null)
                return;

            _lifetimeCts.Cancel();
            _lifetimeCts.Dispose();
            _lifetimeCts = null;
        }

        private async UniTaskVoid ExpireAsync(float lifetime, CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(lifetime), cancellationToken: token);
                Despawn();
            }
            catch (OperationCanceledException)
            {
                // Hit something, relaunched or destroyed before the lifetime expired.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        /// <summary>Scene-scoped pool of enemy projectiles, bound in <c>CombatInstaller</c>.</summary>
        public sealed class Pool : MonoMemoryPool<EnemyProjectileLaunch, EnemyProjectile>
        {
            protected override void Reinitialize(EnemyProjectileLaunch launch, EnemyProjectile item)
                => item.Launch(launch, this);
        }
    }
}
