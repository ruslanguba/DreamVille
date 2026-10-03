using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DreamVille.UI
{
    /// <summary>
    /// Вступительная заставка. Только показывает картинки —
    /// решение о переходе принимает точка входа игры.
    /// </summary>
    public class GameStartUI : MonoBehaviour
    {
        [Header("Main Images")]
        [SerializeField] private Image firstImage;
        [SerializeField] private Image[] collageImages;

        [Header("Settings")]
        [SerializeField] private float fadeTime = 1.2f;
        [SerializeField] private float appearScale = 1.12f;
        [SerializeField] private float firstScale = 1.08f;

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

        /// <summary>Проигрывает заставку целиком. Завершается, когда показан последний кадр.</summary>
        public async UniTask PlayAsync(CancellationToken token = default)
        {
            InitImages();

            await FadeAndGrowFirstAsync(token);

            foreach (Image image in collageImages)
            {
                _skipRequested = false;
                await ShowCollageImageAsync(image, token);
            }
        }

        private void InitImages()
        {
            firstImage.SetAlpha(1f);
            firstImage.transform.localScale = Vector3.one;

            foreach (Image image in collageImages)
            {
                image.SetAlpha(0f);
                image.transform.localScale = Vector3.one * appearScale;
            }
        }

        private async UniTask FadeAndGrowFirstAsync(CancellationToken token)
        {
            await AnimateAsync(
                progress =>
                {
                    firstImage.SetAlpha(Mathf.Lerp(1f, 0.5f, progress));
                    firstImage.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * firstScale, progress);
                },
                token);
        }

        private async UniTask ShowCollageImageAsync(Image image, CancellationToken token)
        {
            Vector3 startScale = Vector3.one * appearScale;

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

        /// <summary>Общий цикл анимации: прерывается по Skip или по отмене.</summary>
        private async UniTask AnimateAsync(Action<float> apply, CancellationToken token)
        {
            float elapsed = 0f;

            while (elapsed < fadeTime && !_skipRequested)
            {
                elapsed += Time.deltaTime;
                apply(Mathf.Clamp01(elapsed / fadeTime));
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
    }
}
