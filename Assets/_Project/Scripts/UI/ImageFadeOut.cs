using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

using DreamVille.Core.Async;

namespace DreamVille.UI
{
    /// <summary>
    /// Shows an image (the main-menu logo) for a while, fades its alpha to zero and deactivates it.
    /// Runs once from Start in scaled game time; stops if this component is destroyed.
    /// </summary>
    public sealed class ImageFadeOut : MonoBehaviour
    {
        [SerializeField] private Image _image;

        [SerializeField, Min(0f), Tooltip("Seconds of game time the fade takes.")]
        private float _fadeDuration = 3f;

        [SerializeField, Min(0f), Tooltip("Seconds of game time the image stays opaque before the fade.")]
        private float _showDuration = 2f;

        private float _startAlpha;

        private void Awake()
        {
            if (_image != null)
                _startAlpha = _image.color.a;
        }

        private void Start()
        {
            if (_image == null)
            {
                Debug.LogError($"[{nameof(ImageFadeOut)}] '{name}' has no image to fade.", this);
                return;
            }

            FadeOutAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid FadeOutAsync(CancellationToken token)
        {
            try
            {
                await GameTime.DelayAsync(_showDuration, token);
                await Tween.RunAsync(_fadeDuration, ApplyFade, token);

                _image.gameObject.SetActive(false);
            }
            catch (OperationCanceledException)
            {
                // Destroyed before the fade finished (scene unloaded).
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private void ApplyFade(float progress)
        {
            _image.SetAlpha(Mathf.Lerp(_startAlpha, 0f, progress));
        }
    }
}
