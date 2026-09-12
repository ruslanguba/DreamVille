using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace DreamVille.SceneManagement
{
    /// <summary>
    /// Single owner of scene transitions. Requests received while a transition is in
    /// flight are ignored, not queued.
    /// </summary>
    public interface ISceneLoader
    {
        /// <summary>Reports 0 when a transition starts and 1 when it completes.</summary>
        event Action<float> ProgressChanged;

        bool IsLoading { get; }

        UniTask LoadAsync(string sceneName, CancellationToken token = default);

        UniTask LoadAsync(int buildIndex, CancellationToken token = default);

        UniTask ReloadActiveAsync(CancellationToken token = default);
    }
}
