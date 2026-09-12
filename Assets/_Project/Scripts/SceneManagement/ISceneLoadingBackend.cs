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
        /// <summary>Name of the scene that is currently active, used to reload it by name.</summary>
        string ActiveSceneName { get; }
        UniTask LoadAsync(string sceneName, Action<float> progress);
    }
}
