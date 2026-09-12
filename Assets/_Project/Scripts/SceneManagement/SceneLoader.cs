using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DreamVille.SceneManagement
{
    /// <summary>
    /// Owns one scene transition at a time. Call from Unity's main thread.
    /// Requests received during a transition are ignored, not queued.
    /// </summary>
    public sealed class SceneLoader : ISceneLoader
    {
        private readonly ISceneLoadingBackend _backend;

        public SceneLoader(ISceneLoadingBackend backend)
        {
            _backend = backend ?? throw new ArgumentNullException(nameof(backend));
        }

        public event Action<float> ProgressChanged;
        public bool IsLoading { get; private set; }

        public UniTask LoadAsync(string sceneName, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                throw new ArgumentException("A scene name or path is required.", nameof(sceneName));

            return RunAsync(() => _backend.LoadAsync(sceneName, ReportProgress), token);
        }

        public UniTask ReloadActiveAsync(CancellationToken token = default)
            => LoadAsync(_backend.ActiveSceneName, token);

        private async UniTask RunAsync(Func<UniTask> start, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (IsLoading)
                return;

            IsLoading = true;
            try
            {
                ReportProgress(0f);
                // A progress listener may cancel before the native operation starts.
                token.ThrowIfCancellationRequested();
                // Unity scene loading cannot be cancelled once started. Retain ownership
                // until it finishes, even when the requesting scene has been destroyed.
                await start();
                ReportProgress(1f);
                token.ThrowIfCancellationRequested();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ReportProgress(float value)
        {
            var listeners = ProgressChanged;
            if (listeners == null)
                return;

            foreach (Action<float> listener in listeners.GetInvocationList())
            {
                try { listener(value); }
                catch (Exception exception)
                {
                    // Presentation failures must not release an in-flight transition.
                    Debug.LogException(exception);
                }
            }
        }
    }
}
