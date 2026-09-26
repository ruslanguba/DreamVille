using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DreamVille.Core.Async
{
    /// <summary>Frame-driven interpolation in scaled game time.</summary>
    public static class Tween
    {
        /// <summary>elapsed / duration clamped to [0, 1]; a non-positive duration is complete at once.</summary>
        public static float Progress(float elapsed, float duration)
            => duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);

        /// <summary>
        /// Calls <paramref name="apply"/> once per frame with progress in (0, 1] for <paramref name="duration"/>
        /// seconds of game time. The first step runs synchronously with the current frame's delta, like the first
        /// iteration of a coroutine loop. A non-positive duration applies 1 once and returns without waiting.
        /// Cancellation surfaces as <see cref="OperationCanceledException"/>.
        /// </summary>
        public static async UniTask RunAsync(float duration, Action<float> apply, CancellationToken token)
        {
            if (apply == null)
                throw new ArgumentNullException(nameof(apply));

            if (duration <= 0f)
            {
                apply(1f);
                return;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();
                elapsed += Time.deltaTime;
                apply(Progress(elapsed, duration));
                await GameTime.NextFrameAsync(token);
            }
        }
    }
}
