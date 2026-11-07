using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DreamVille.UI
{
    /// <summary>
    /// Intro splash. It only shows the pictures; the game's entry point decides what comes next.
    /// </summary>
    public class GameStartUI : MonoBehaviour, IGameIntro
    {
        [Header("Main Images")]
        [SerializeField] private Image _firstImage;
        [SerializeField] private Image[] _collageImages;

        [Header("Settings")]
        [SerializeField] private float _fadeTime = 1.2f;
        [SerializeField] private float _appearScale = 1.12f;
        [SerializeField] private float _firstScale = 1.08f;
        [SerializeField, Range(0f, 1f)] private float _firstFadeAlpha = 0.5f;

        [Header("Input")]
        [SerializeField] private InputActionReference _skipAction;

        private bool _skipRequested;

        private void OnEnable()
        {
            if (_skipAction?.action == null)
                return;

            _skipAction.action.Enable();
            _skipAction.action.performed += OnSkipPerformed;
        }

        private void OnDisable()
        {
            if (_skipAction?.action == null)
                return;

            _skipAction.action.performed -= OnSkipPerformed;
            _skipAction.action.Disable();
        }

        private void OnSkipPerformed(InputAction.CallbackContext _) => _skipRequested = true;

        /// <inheritdoc />
        public async UniTask PlayAsync(CancellationToken token)
        {
            InitImages();

            await FadeAndGrowFirstAsync(token);

            foreach (Image image in _collageImages)
            {
                _skipRequested = false;
                await ShowCollageImageAsync(image, token);
            }
        }

        private void InitImages()
        {
            _firstImage.SetAlpha(1f);
            _firstImage.transform.localScale = Vector3.one;

            foreach (Image image in _collageImages)
            {
                image.SetAlpha(0f);
                image.transform.localScale = Vector3.one * _appearScale;
            }
        }

        private async UniTask FadeAndGrowFirstAsync(CancellationToken token)
        {
            await AnimateAsync(
                progress =>
                {
                    _firstImage.SetAlpha(Mathf.Lerp(1f, _firstFadeAlpha, progress));
                    _firstImage.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * _firstScale, progress);
                },
                token);
        }

        private async UniTask ShowCollageImageAsync(Image image, CancellationToken token)
        {
            Vector3 startScale = Vector3.one * _appearScale;

            await AnimateAsync(
                progress =>
                {
                    image.SetAlpha(progress);
                    image.transform.localScale = Vector3.Lerp(startScale, Vector3.one, progress);
                },
                token);

            image.SetAlpha(1f);
            image.transform.localScale = Vector3.one;
        }

        /// <summary>Shared animation loop: stops early on Skip or cancellation.</summary>
        private async UniTask AnimateAsync(Action<float> apply, CancellationToken token)
        {
            float elapsed = 0f;

            while (elapsed < _fadeTime && !_skipRequested)
            {
                elapsed += Time.deltaTime;
                apply(Mathf.Clamp01(elapsed / _fadeTime));
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
    }
}
