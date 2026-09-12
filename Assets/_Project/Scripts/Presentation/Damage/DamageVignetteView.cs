using System;
using System.Threading;

using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Zenject;

using DreamVille.Core.Players;

namespace DreamVille.Presentation.Damage
{
    /// <summary>Fades a scene's global-volume vignette in and out as the player's hit count changes.</summary>
    [RequireComponent(typeof(Volume))]
    public sealed class DamageVignetteView : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _hitFadeDuration = 2f;
        [SerializeField, Min(0f)] private float _recoveryFadeDuration = 5f;
        [SerializeField, Range(0f, 1f)] private float _firstHitIntensity = 0.6f;

        private IPlayerDamageState _damage;
        private Vignette _vignette;
        private CancellationTokenSource _fadeCts;

        [Inject]
        private void Construct(IPlayerDamageState damage)
        {
            _damage = damage;
        }

        private void Awake()
        {
            if (!GetComponent<Volume>().profile.TryGet(out _vignette))
            {
                Debug.LogError($"[{nameof(DamageVignetteView)}] Volume profile has no Vignette override.", this);
                enabled = false;
                return;
            }

            _vignette.active = true;
            _vignette.intensity.overrideState = true;
            _vignette.intensity.value = 0f;
        }

        private void OnEnable()
        {
            _damage.HitsChanged += OnHitsChanged;
            _damage.Recovered += OnRecovered;
        }

        private void OnDisable()
        {
            _damage.HitsChanged -= OnHitsChanged;
            _damage.Recovered -= OnRecovered;

            // A disabled view must not keep writing into the profile.
            CancelFade();
        }

        private void OnDestroy()
        {
            CancelFade();
        }

        private void CancelFade()
        {
            if (_fadeCts == null)
                return;

            _fadeCts.Cancel();
            _fadeCts.Dispose();
            _fadeCts = null;
        }

        private void OnHitsChanged(int hits)
        {
            float intensity = DamageVignetteIntensity.Calculate(hits, _damage.MaxHits, _firstHitIntensity);
            FadeTo(intensity, _hitFadeDuration);
        }

        private void OnRecovered()
        {
            FadeTo(0f, _recoveryFadeDuration);
        }

        private void FadeTo(float target, float duration)
        {
            CancelFade();
            _fadeCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

            FadeAsync(target, duration, _fadeCts.Token).Forget();
        }

        private async UniTaskVoid FadeAsync(float target, float duration, CancellationToken token)
        {
            try
            {
                float start = _vignette.intensity.value;
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    _vignette.intensity.value = Mathf.Lerp(start, target, elapsed / duration);
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                _vignette.intensity.value = target;
            }
            catch (OperationCanceledException)
            {
                // A newer fade cancelled this one, or the view went away; nothing left to do.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }
}
