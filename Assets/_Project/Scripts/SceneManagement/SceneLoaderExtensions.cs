using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DreamVille.SceneManagement
{
    /// <summary>
    /// Fire-and-forget helpers for callers that start a transition and have nobody to report to.
    /// Cancellation is silent, any other failure (including a synchronous one such as an empty scene name) is logged.
    /// Call sites use <c>.Forget()</c>.
    /// </summary>
    public static class SceneLoaderExtensions
    {
        /// <summary>Loads <paramref name="sceneName"/>; never throws.</summary>
        public static UniTask LoadAndLogAsync(this ISceneLoader loader, string sceneName, CancellationToken token)
        {
            return RunAndLogAsync(() => loader.LoadAsync(sceneName, token));
        }

        /// <summary>Reloads the active scene; never throws.</summary>
        public static UniTask ReloadAndLogAsync(this ISceneLoader loader, CancellationToken token)
        {
            return RunAndLogAsync(() => loader.ReloadActiveAsync(token));
        }

        // The factory is invoked inside the try, so a throw before the first await is handled as well.
        private static async UniTask RunAndLogAsync(Func<UniTask> transition)
        {
            try
            {
                await transition();
            }
            catch (OperationCanceledException)
            {
                // Cancellation is not a failure; the loader keeps ownership of the native operation anyway.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
