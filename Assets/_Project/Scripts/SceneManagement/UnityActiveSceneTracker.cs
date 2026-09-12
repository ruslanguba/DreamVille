using System;
using UnityEngine.SceneManagement;
using Zenject;

using DreamVille.Core.Scenes;

namespace DreamVille.SceneManagement
{
    /// <summary>
    /// Adapter over <see cref="SceneManager"/> events. Project-scoped, so it lives as long
    /// as the application and unsubscribes exactly once on shutdown.
    /// </summary>
    public sealed class UnityActiveSceneTracker : IActiveSceneTracker, IInitializable, IDisposable
    {
        public event Action<string> SceneLoaded;

        public string ActiveSceneName => SceneManager.GetActiveScene().name;

        public void Initialize()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => SceneLoaded?.Invoke(scene.name);
    }
}
