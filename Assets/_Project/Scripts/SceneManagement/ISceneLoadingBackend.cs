using System;
using Cysharp.Threading.Tasks;

namespace DreamVille.SceneManagement
{
    /// <summary>
    /// Engine boundary. The returned task completes only after the actual load completes
    /// or fails. Implementations must not return early on caller cancellation.
    /// </summary>
    public interface ISceneLoadingBackend
    {
        int ActiveSceneBuildIndex { get; }
        UniTask LoadAsync(string sceneName, Action<float> progress);
        UniTask LoadAsync(int buildIndex, Action<float> progress);
    }
}
