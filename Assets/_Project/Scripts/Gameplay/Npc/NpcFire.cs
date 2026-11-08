using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

using DreamVille.Core.Async;

namespace DreamVille.Gameplay.Npc
{
    /// <summary>
    /// Ambient shooter: one reused visual bullet is launched at a random point around the target, lives for a
    /// while and hides, then the cycle repeats. Runs while the component is enabled; deals no damage.
    /// </summary>
    public sealed class NpcFire : MonoBehaviour
    {
        [SerializeField] private GameObject _bulletPrefab;
        [SerializeField] private Transform _spawn;
        [SerializeField] private Transform _target;

        [SerializeField, Tooltip("Impulse per metre to the target: the launch vector is not normalized.")]
        private float _launchImpulsePerMetre = 2;

        [SerializeField, Min(0f), Tooltip("Seconds of game time before each shot.")]
        private float _shotInterval = 3;

        [SerializeField, Min(0f), Tooltip("Seconds of game time a bullet stays visible.")]
        private float _bulletLifetime = 2;

        private GameObject _bullet;
        private Rigidbody _bulletBody;
        private CancellationTokenSource _loopCts;
        private bool _started;

        private void Awake()
        {
            if (_bulletPrefab == null || _spawn == null || _target == null)
            {
                Debug.LogError($"[{nameof(NpcFire)}] '{name}' needs a bullet prefab, a spawn point and a target.", this);
                enabled = false;
            }
        }

        private void Start()
        {
            _bullet = Instantiate(_bulletPrefab, _spawn.position, _spawn.rotation);
            _bulletBody = _bullet.GetComponent<Rigidbody>();
            _bullet.SetActive(false);

            if (_bulletBody == null)
            {
                Debug.LogError($"[{nameof(NpcFire)}] Bullet prefab '{_bulletPrefab.name}' has no Rigidbody.", this);
                enabled = false;
                return;
            }

            _started = true;
            StartLoop();
        }

        private void OnEnable()
        {
            if (_started)
                StartLoop();
        }

        private void OnDisable()
        {
            Cancellation.CancelAndDispose(ref _loopCts);

            // The bullet is a scene root and may already be unloaded when the scene closes.
            if (_bullet != null)
                _bullet.SetActive(false);
        }

        private void StartLoop()
        {
            Cancellation.CancelAndDispose(ref _loopCts);
            _loopCts = new CancellationTokenSource();
            RunAsync(_loopCts.Token).Forget();
        }

        private async UniTaskVoid RunAsync(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    await GameTime.DelayAsync(_shotInterval, token);
                    Launch();

                    await GameTime.DelayAsync(_bulletLifetime, token);
                    _bullet.SetActive(false);
                }
            }
            catch (OperationCanceledException)
            {
                // Disabled or destroyed; the loop restarts on the next OnEnable.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private void Launch()
        {
            _bulletBody.linearVelocity = Vector3.zero;
            _bullet.transform.position = _spawn.position;
            _bullet.SetActive(true);
            _bulletBody.AddForce(GetRandomDirection() * _launchImpulsePerMetre, ForceMode.Impulse);
        }

        private Vector3 GetRandomDirection()
        {
            Vector3 randomTarget = _target.position + Random.insideUnitSphere;
            return randomTarget - _spawn.position;
        }
    }
}
