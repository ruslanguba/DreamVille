using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using DreamVille.Core.Async;

namespace DreamVille.UI.Hud
{
    /// <summary>
    /// Pop-up with the score just added. It hides a fixed time after the latest score; an earlier score's
    /// timer never hides a newer one.
    /// </summary>
    public sealed class ScorePopupView : MonoBehaviour, IScoreView
    {
        [SerializeField] private TextMeshProUGUI _addedScore;
        [SerializeField] private Image _addedScoreImage;

        [SerializeField, Min(0f), Tooltip("Seconds of game time the pop-up stays visible after the latest score.")]
        private float _visibleSeconds = 3f;

        private CancellationTokenSource _hideCts;

        private void Awake()
        {
            _addedScoreImage.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            Cancellation.CancelAndDispose(ref _hideCts);
        }

        public void ShowAdded(float delta)
        {
            _addedScoreImage.gameObject.SetActive(true);
            _addedScore.text = delta.ToString();

            HideAfterAsync(Cancellation.Restart(ref _hideCts, this.GetCancellationTokenOnDestroy())).Forget();
        }

        private async UniTaskVoid HideAfterAsync(CancellationToken token)
        {
            try
            {
                await GameTime.DelayAsync(_visibleSeconds, token);
                _addedScoreImage.gameObject.SetActive(false);
            }
            catch (OperationCanceledException)
            {
                // A newer score owns the timer, or the HUD was destroyed.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }
}
