using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using DreamVille.Core.Async;

namespace DreamVille.UI
{
    /// <summary>
    /// Intro splash. It only shows the pictures; the game's entry point decides what comes next.
    /// Each picture animates in scaled game time; the skip action jumps the current picture to its end.
    /// </summary>
    public sealed class GameStartUI : MonoBehaviour, IGameIntro
    {
        [Header("Main Images")]
        [SerializeField] private Image _firstImage;
        [SerializeField] private Image[] _collageImages;

        [Header("Settings")]
        [SerializeField, Min(0f), Tooltip("Seconds of game time each picture takes to animate.")]
        private float _fadeTime = 1.2f;

        [SerializeField, Min(0f), Tooltip("Scale a collage picture starts from; it settles at 1.")]
        private float _appearScale = 1.12f;

        [SerializeField, Min(0f), Tooltip("Scale the first picture grows to while it fades.")]
        private float _firstScale = 1.08f;

        [SerializeField, Range(0f, 1f)] private float _firstFadeAlpha = 0.5f;

        [Header("Input")]
        [SerializeField] private InputActionReference _skipAction;

        private InputAction _skip;

        // The running picture's own token: the skip cancels only this step, never the whole intro.
        private CancellationTokenSource _stepCts;

        private void Awake()
        {
            // Resolved once: InputActionReference is a ScriptableObject, so it is compared with Unity's null, never ?.
            _skip = _skipAction == null ? null : _skipAction.action;
            if (_skip == null)
                Debug.LogError($"[{nameof(GameStartUI)}] '{name}': {nameof(_skipAction)} is not assigned; the intro cannot be skipped.", this);
        }

        private void OnEnable()
        {
            if (_skip == null)
                return;

            _skip.Enable();
            _skip.performed += OnSkipPerformed;
        }

        private void OnDisable()
        {
            if (_skip == null)
                return;

            _skip.performed -= OnSkipPerformed;
            _skip.Disable();
        }

        private void OnDestroy()
        {
            Cancellation.CancelAndDispose(ref _stepCts);
        }

        private void OnSkipPerformed(InputAction.CallbackContext _) => _stepCts?.Cancel();

        /// <inheritdoc />
        public async UniTask PlayAsync(CancellationToken token)
        {
            InitImages();

            await RunStepAsync(ApplyFirst, token);

            foreach (Image image in _collageImages)
                await RunStepAsync(progress => ApplyCollage(image, progress), token);
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

        private void ApplyFirst(float progress)
        {
            _firstImage.SetAlpha(Mathf.Lerp(1f, _firstFadeAlpha, progress));
            _firstImage.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * _firstScale, progress);
        }

        private void ApplyCollage(Image image, float progress)
        {
            image.SetAlpha(progress);
            image.transform.localScale = Vector3.Lerp(Vector3.one * _appearScale, Vector3.one, progress);
        }

        /// <summary>Plays one picture; a skip ends it early. Either way the picture finishes in its end state.</summary>
        private async UniTask RunStepAsync(Action<float> apply, CancellationToken token)
        {
            CancellationToken stepToken = Cancellation.Restart(ref _stepCts, token);
            try
            {
                await Tween.RunAsync(_fadeTime, apply, stepToken);
            }
            catch (OperationCanceledException) when (!token.IsCancellationRequested)
            {
                // Skipped: the picture jumps to its end below.
            }
            finally
            {
                Cancellation.CancelAndDispose(ref _stepCts);
            }

            apply(1f);
        }
    }
}
