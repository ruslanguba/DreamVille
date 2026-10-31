using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

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

        // Overridden per instance in Level_02 and Lobby_01; renames follow ADR 0010.
        [SerializeField] private float _scoreToAdd = 1f;

        [SerializeField, Min(0f)] private float _growDuration = 0.3f;

        [SerializeField, Min(0f)] private float _flyDuration = 0.5f;

        private IScoreService _score;
        private EffectChain _effects;
        private Collider _collider;
        private Vector3 _originalScale;
        private bool _collected;

        [Inject]
        private void Construct(IScoreService score)
        {
            _score = score;
        }

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _originalScale = transform.localScale;
            _effects = EffectChain.FromComponents(this);
        }

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

            _collected = true;
            _collider.enabled = false;
            _score.AddScore(_scoreToAdd);
            _effects.Apply(context);
            CollectAsync(context.Instigator, this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid CollectAsync(Transform target, CancellationToken token)
        {
            try
            {
                float elapsed = 0f;
                while (elapsed < _growDuration)
                {
                    elapsed += Time.deltaTime;
                    transform.localScale = _originalScale * PickupTween.GrowScale(PickupTween.Progress(elapsed, _growDuration));
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                elapsed = 0f;
                Vector3 startPosition = transform.position;
                while (elapsed < _flyDuration)
                {
                    elapsed += Time.deltaTime;
                    float progress = PickupTween.Progress(elapsed, _flyDuration);

                    // The collector moves with the player, so its position is read every frame.
                    if (target != null)
                        transform.position = Vector3.Lerp(startPosition, target.position, progress);

                    transform.localScale = _originalScale * PickupTween.FlyScale(progress);
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

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
