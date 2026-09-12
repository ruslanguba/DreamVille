using System;

namespace DreamVille.Core.Scenes
{
    /// <summary>
    /// Engine-free view of the active scene for services that react to scene changes
    /// without depending on UnityEngine.SceneManagement (music, cursor rules, analytics).
    /// </summary>
    public interface IActiveSceneTracker
    {
        /// <summary>Name of the scene that is active right now.</summary>
        string ActiveSceneName { get; }

        /// <summary>Raised with the scene name after a scene finishes loading.</summary>
        event Action<string> SceneLoaded;
    }
}
