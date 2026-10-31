using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

using DreamVille.Core.Async;

namespace DreamVille.Gameplay.Environment
{
    /// <summary>
    /// Ambient drift for floating props (balloons): flies to a random point within a radius at a random speed,
    /// rests, picks the next point. Height stays within the authored limits. Runs while the component is enabled,
    /// in scaled game time, so pause freezes it.
    /// </summary>
    public sealed class RandomFloating : MonoBehaviour
    {
        private const float ArriveDistance = 0.1f;

        [Header("Movement")]
        [SerializeField, Tooltip("Maximum distance of the next target from the current position, m.")]
        private float _radius = 5f;

        [SerializeField] private float _minSpeed = 2f;
        [SerializeField] private float _maxSpeed = 4f;

        [Header("Height Limit")]
        [SerializeField] private float _minY = -50f;
        [SerializeField] private float _maxY = 50f;

        [Header("Pause")]
        [SerializeField, Min(0f)]
        [Tooltip("Seconds of game time the prop rests at each target.")]
        private float _stopTime = 2f;

        private float _speed;
        private CancellationTokenSource _loopCts;
        private bool _started;

        private void Start()
        {
            _speed = Random.Range(_minSpeed, _maxSpeed);
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
                    Vector3 target = FloatingPath.PickTarget(transform.position, Random.insideUnitSphere * _radius, _minY, _maxY);

                    while (Vector3.Distance(transform.position, target) > ArriveDistance)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, target, _speed * Time.deltaTime);
                        await GameTime.NextFrameAsync(token);
                    }

                    await GameTime.DelayAsync(_stopTime, token);
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
    }
}
