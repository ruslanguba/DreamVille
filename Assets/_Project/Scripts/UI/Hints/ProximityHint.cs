using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

using DreamVille.Core.Async;
using DreamVille.Core.Players;

namespace DreamVille.UI.Hints
{
    /// <summary>
    /// A screen hint for a place in the level. When the player enters this trigger, every graphic under the target is
    /// shown fully opaque, stays for a while, fades out in scaled game time and is hidden. Entering again restarts the
    /// show: the newest show owns the timer. A show-once hint destroys its trigger after the first show has faded out.
    /// Leaving the trigger does not hide the hint.
    /// </summary>
    public sealed class ProximityHint : MonoBehaviour
    {
        // Serialized names are frozen: ProximityHint.prefab instances in seven scenes override them by property path.
        [SerializeField, Tooltip("UI root the hint shows. Starts inactive and is hidden again after the fade.")]
        private GameObject _object;

        [SerializeField, Min(0f), Tooltip("Seconds of game time the fade-out takes.")]
        private float _fadeDuration = 1f;

        [SerializeField, Min(0f), Tooltip("Seconds of game time the hint stays fully visible before the fade.")]
        private float _showDuration = 2f;

        [SerializeField, Tooltip("Destroy this trigger after the first show has faded out, so the hint never shows again.")]
        private bool _showOnce;

        private Graphic[] _graphics;
        private CancellationTokenSource _showCts;

        private void Awake()
        {
            if (_object == null)
            {
                Debug.LogError($"[{nameof(ProximityHint)}] '{name}' has no hint object assigned.", this);
                return;
            }

            _graphics = _object.GetComponentsInChildren<Graphic>(true);
        }

        private void OnDestroy()
        {
            Cancellation.CancelAndDispose(ref _showCts);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (PlayerDetection.IsPlayer(other))
                Show();
        }

        private void Show()
        {
            if (_object == null)
                return;

            SetAlpha(1f);
            _object.SetActive(true);
            ShowAsync(Cancellation.Restart(ref _showCts, this.GetCancellationTokenOnDestroy())).Forget();
        }

        private async UniTaskVoid ShowAsync(CancellationToken token)
        {
            try
            {
                await GameTime.DelayAsync(_showDuration, token);
                await Tween.RunAsync(_fadeDuration, ApplyFade, token);

                _object.SetActive(false);

                if (_showOnce)
                    Destroy(gameObject);
            }
            catch (OperationCanceledException)
            {
                // A newer show owns the timer, or the hint was destroyed with its scene.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private void ApplyFade(float progress)
        {
            SetAlpha(1f - progress);
        }

        private void SetAlpha(float alpha)
        {
            foreach (Graphic graphic in _graphics)
                graphic.SetAlpha(alpha);
        }
    }
}
