using UnityEngine;

namespace DreamVille.Legacy
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
