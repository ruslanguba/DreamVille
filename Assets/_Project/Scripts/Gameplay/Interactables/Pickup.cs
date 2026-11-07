using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

using DreamVille.Core.Async;
using DreamVille.Scoring;

namespace DreamVille.Gameplay.Interactables
{
    /// <summary>
    /// A one-time pickup: on the player's touch it adds its score, applies the effects on this GameObject,
    /// then grows, flies into the collector and destroys itself.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class Pickup : MonoBehaviour, IInteractable
    {
        private const float IdleSpinDegreesPerSecond = 60f;

        [SerializeField] private float _scoreToAdd = 1f;

        [SerializeField, Min(0f)] private float _growDuration = 0.3f;

        [SerializeField, Min(0f)] private float _flyDuration = 0.5f;

        private IScoreService _score;
        private EffectChain _effects;
        private Collider _collider;
        private Vector3 _originalScale;
        private bool _collected;
        private bool _isInitialized;

        [Inject]
        private void Construct(IScoreService score)
        {
            _score = score;
        }

        private void Awake() => EnsureInitialized();

        private void Update()
        {
            if (!_collected)
                transform.Rotate(0f, IdleSpinDegreesPerSecond * Time.deltaTime, 0f);
        }

        public void OnInteract(in InteractionContext context)
        {
            // A bullet flying through a pickup does not collect it.
            if (context.Source != InteractionSource.Touch || _collected)
                return;

            EnsureInitialized();
            _collected = true;
            _collider.enabled = false;
            _score.AddScore(_scoreToAdd);
            _effects.Apply(context);
            CollectAsync(context.Instigator, this.GetCancellationTokenOnDestroy()).Forget();
        }

        // Lazy so that a touch arriving before Awake (or in an EditMode test, where Awake does not run) still works.
        private void EnsureInitialized()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            _collider = GetComponent<Collider>();
            _originalScale = transform.localScale;
            _effects = EffectChain.FromComponents(this);
        }

        private async UniTaskVoid CollectAsync(Transform target, CancellationToken token)
        {
            try
            {
                await Tween.RunAsync(_growDuration,
                    progress => transform.localScale = _originalScale * PickupTween.GrowScale(progress), token);

                Vector3 startPosition = transform.position;
                await Tween.RunAsync(_flyDuration, progress =>
                {
                    // The collector moves with the player, so its position is read every frame.
                    if (target != null)
                        transform.position = Vector3.Lerp(startPosition, target.position, progress);

                    transform.localScale = _originalScale * PickupTween.FlyScale(progress);
                }, token);

                Destroy(gameObject);
            }
            catch (OperationCanceledException)
            {
                // Destroyed (or the scene unloaded) mid-flight.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }
}
