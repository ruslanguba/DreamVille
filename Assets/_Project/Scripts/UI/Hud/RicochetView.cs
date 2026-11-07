using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

using DreamVille.Core.Async;

namespace DreamVille.UI.Hud
{
    /// <summary>
    /// "xN" badges drawn over a world point for a short time. Each badge owns its own hide timer, and the
    /// latest show of a badge restarts it. Badges start hidden, so every show from hidden replays their animation.
    /// </summary>
    public sealed class RicochetView : MonoBehaviour, IRicochetView
    {
        [SerializeField] private Image[] _images;

        [SerializeField, Min(0f), Tooltip("Seconds of game time a badge stays visible after its latest show.")]
        private float _visibleSeconds = 0.5f;

        private Camera _camera;
        private CancellationTokenSource[] _hideCts;

        public int BadgeCount => _images.Length;

        private void Awake()
        {
            _camera = Camera.main;
            _hideCts = new CancellationTokenSource[_images.Length];

            foreach (Image badge in _images)
                badge.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_hideCts == null)
                return;

            for (int i = 0; i < _hideCts.Length; i++)
                Cancellation.CancelAndDispose(ref _hideCts[i]);
        }

        public void ShowBadge(int badgeIndex, Vector3 worldPoint)
        {
            if (badgeIndex < 0 || badgeIndex >= _images.Length)
                return;

            // A point behind the camera projects to a mirrored screen position; such a hit is simply not shown.
            Vector3 screenPoint = _camera.WorldToScreenPoint(worldPoint);
            if (screenPoint.z < 0f)
                return;

            Image badge = _images[badgeIndex];
            badge.gameObject.SetActive(true);
            badge.rectTransform.position = screenPoint;

            CancellationToken token = Cancellation.Restart(ref _hideCts[badgeIndex], this.GetCancellationTokenOnDestroy());
            HideAfterAsync(badge, token).Forget();
        }

        private async UniTaskVoid HideAfterAsync(Image badge, CancellationToken token)
        {
            try
            {
                await GameTime.DelayAsync(_visibleSeconds, token);
                badge.gameObject.SetActive(false);
            }
            catch (OperationCanceledException)
            {
                // A newer show of this badge owns the timer, or the HUD was destroyed.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }
}
