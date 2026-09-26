using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace DreamVille.Core.Async
{
    /// <summary>
    /// Waits in scaled game time: <c>Time.timeScale = 0</c> (pause) freezes them, exactly like
    /// <c>WaitForSeconds</c> and <c>Time.deltaTime</c>-driven loops. Every wait yields at least one frame,
    /// so a zero or negative duration inside an endless loop can never spin synchronously.
    /// </summary>
    public static class GameTime
    {
        /// <summary>Resumes at the start of the next frame's Update phase.</summary>
        public static UniTask NextFrameAsync(CancellationToken token)
            => UniTask.Yield(PlayerLoopTiming.Update, token);

        /// <summary>Waits <paramref name="seconds"/> of scaled game time; a non-positive value waits one frame.</summary>
        public static UniTask DelayAsync(float seconds, CancellationToken token)
            => seconds > 0f
                // DelayType is the default, passed explicitly: this is the one place that owns the time policy.
                ? UniTask.Delay(TimeSpan.FromSeconds(seconds), DelayType.DeltaTime, PlayerLoopTiming.Update, token)
                : NextFrameAsync(token);
    }
}
