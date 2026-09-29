using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

using DreamVille.Core.Async;

namespace DreamVille.UI.Hud
{
    /// <summary>
    /// One star meter: a filled image plus an unlock effect (particles and a scale pop).
    /// A new unlock restarts the pop from the current scale.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class ScoreStar : MonoBehaviour
    {
        [SerializeField] private float _popScale = 1.3f;

        [SerializeField, Min(0f), Tooltip("Seconds of game time for each half of the pop (grow, then shrink back).")]
        private float _popTime = 0.2f;

        [SerializeField] private ParticleSystem _particle;

        private Image _image;
        private Vector3 _originalScale;
        private Vector3 _poppedScale;
        private CancellationTokenSource _popCts;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _originalScale = transform.localScale;
            _poppedScale = _originalScale * _popScale;
        }

        private void OnDestroy()
        {
            Cancellation.CancelAndDispose(ref _popCts);
        }

        /// <summary>Sets the meter's fill, clamped to 0..1.</summary>
        public void SetFill(float normalized)
        {
            _image.fillAmount = Mathf.Clamp01(normalized);
        }

        /// <summary>Plays the particles and the scale pop.</summary>
        public void PlayUnlock()
        {
            if (_particle != null)
                _particle.Play();

            PopAsync(Cancellation.Restart(ref _popCts, this.GetCancellationTokenOnDestroy())).Forget();
        }

        private async UniTaskVoid PopAsync(CancellationToken token)
        {
            try
            {
                await Tween.RunAsync(_popTime, Grow, token);
                await Tween.RunAsync(_popTime, ShrinkBack, token);

                transform.localScale = _originalScale;
            }
            catch (OperationCanceledException)
            {
                // A newer unlock took over the pop, or the HUD was destroyed.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private void Grow(float progress)
        {
            transform.localScale = Vector3.Lerp(_originalScale, _poppedScale, progress);
        }

        private void ShrinkBack(float progress)
        {
            transform.localScale = Vector3.Lerp(_poppedScale, _originalScale, progress);
        }
    }
}
