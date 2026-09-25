using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

using DreamVille.Configs;
using DreamVille.Input;

namespace DreamVille.Gameplay.Weapons.Pillow
{
    /// <summary>
    /// Scene-facing pillow component on the player root: owns the scene references, the input subscription and
    /// the recall loop. The throw and recall rules live in <see cref="PillowRack"/>.
    /// </summary>
    public sealed class PillowLauncher : MonoBehaviour, IPillowThrower
    {
        [SerializeField] private PillowConfig _config;

        [Tooltip("Throw origin and the point recalled pillows return to.")]
        [SerializeField] private Transform _spawnPoint;

        [SerializeField] private Pillow[] _pillows;

        private IPlayerInput _input;
        private Transform _body;
        private PillowRack _rack;
        private CancellationTokenSource _returnCts;

        /// <inheritdoc />
        public event Action Changed;

        /// <inheritdoc />
        public int Available => _rack?.Available ?? 0;

        /// <inheritdoc />
        public int InHand => _rack?.InHand ?? 0;

        [Inject]
        public void Construct(IPlayerInput input, Transform body)
        {
            _input = input;
            _body = body;
        }

        private void Awake()
        {
            if (!TryValidate(out string error))
            {
                Debug.LogError($"[{nameof(PillowLauncher)}] {error} Pillows are disabled.", this);
                enabled = false;
                return;
            }

            _rack = new PillowRack(_pillows, _config);
            _rack.SetAvailable(_rack.Capacity);
        }

        private void OnEnable()
        {
            if (_rack == null)
                return;   // Awake rejected the setup; stay inert even if re-enabled

            _input.Throw += OnThrow;
            _input.Stop += OnStop;
        }

        private void OnDisable()
        {
            _input.Throw -= OnThrow;
            _input.Stop -= OnStop;
            CancelReturn();
        }

        private void OnThrow()
        {
            Vector3 spawnPosition = _spawnPoint.position;

            switch (_rack.ThrowOrRecall(new Pose(spawnPosition, _body.rotation), spawnPosition))
            {
                case PillowCommand.Thrown:
                    Changed?.Invoke();
                    break;

                case PillowCommand.ReturnStarted:
                    StartReturn();
                    break;
            }
        }

        private void OnStop()
        {
            _rack.FreezeFlying();
        }

        private void StartReturn()
        {
            DisposeReturnSource();
            _returnCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            ReturnAsync(_returnCts.Token).Forget();
        }

        private async UniTaskVoid ReturnAsync(CancellationToken token)
        {
            try
            {
                while (!_rack.StepReturn(_spawnPoint.position, Time.deltaTime))
                    await UniTask.Yield(PlayerLoopTiming.Update, token);

                if (_returnCts != null && _returnCts.Token == token)
                    DisposeReturnSource();

                Changed?.Invoke();
            }
            catch (OperationCanceledException)
            {
                // CancelReturn (OnDisable, which Unity always runs before OnDestroy) has already stowed this pillow.
                // Completing here could stow a newer return started after a re-enable, so the task just ends.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        // A return interrupted by disable or teardown completes at once, so a slot is never stranded mid-way.
        private void CancelReturn()
        {
            if (_returnCts == null)
                return;

            if (_rack.CompleteReturn())
                Changed?.Invoke();

            DisposeReturnSource();
        }

        private void DisposeReturnSource()
        {
            if (_returnCts == null)
                return;

            _returnCts.Cancel();
            _returnCts.Dispose();
            _returnCts = null;
        }

        private bool TryValidate(out string error)
        {
            if (_config == null)
            {
                error = $"{nameof(_config)} is not assigned.";
                return false;
            }

            if (!_config.IsValid(out string configError))
            {
                error = $"{nameof(_config)} is invalid: {configError}";
                return false;
            }

            if (_spawnPoint == null)
            {
                error = $"{nameof(_spawnPoint)} is not assigned.";
                return false;
            }

            if (_pillows == null)
            {
                error = $"{nameof(_pillows)} is not assigned.";
                return false;
            }

            for (int i = 0; i < _pillows.Length; i++)
            {
                if (_pillows[i] == null)
                {
                    error = $"{nameof(_pillows)}[{i}] is not assigned.";
                    return false;
                }
            }

            error = null;
            return true;
        }
    }
}
