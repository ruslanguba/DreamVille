using UnityEngine;

namespace DreamVille.Core.SceneManagement
{
    public class SceneTransitionRequest
    {
        public string SceneName { get; }

        public SceneTransitionRequest(string sceneName)
        {
            SceneName = sceneName;
        }
    }
}
