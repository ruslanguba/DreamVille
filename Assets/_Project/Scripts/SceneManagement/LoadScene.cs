using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace DreamVille.SceneManagement
{
    /// <summary>Menu button entry point: loads a scene by its Build Settings index.</summary>
    public class LoadScene : MonoBehaviour
    {
        private ISceneLoader _sceneLoader;

        [Inject]
        public void Construct(ISceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        public void LoadLevel(int level)
        {
            _sceneLoader.LoadAsync(level).Forget();
        }
    }
}
