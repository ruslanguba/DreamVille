using System;

using DreamVille.Core.Scenes;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>
    /// Hand-written double for <see cref="IActiveSceneTracker"/>: the test decides which
    /// scene is active and when a load "finishes", with no editor scene involved.
    /// </summary>
    public sealed class FakeActiveSceneTracker : IActiveSceneTracker
    {
        public FakeActiveSceneTracker(string activeSceneName)
        {
            ActiveSceneName = activeSceneName;
        }

        public event Action<string> SceneLoaded;

        public string ActiveSceneName { get; private set; }

        /// <summary>Simulates a completed scene load: the scene becomes active and listeners are told.</summary>
        public void Load(string sceneName)
        {
            ActiveSceneName = sceneName;
            SceneLoaded?.Invoke(sceneName);
        }
    }
}
