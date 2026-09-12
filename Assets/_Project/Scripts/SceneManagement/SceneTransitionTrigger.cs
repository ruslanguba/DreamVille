using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

using DreamVille.Core.Players;

namespace DreamVille.SceneManagement
{
    /// <summary>Loads the configured scene when the player enters this trigger volume.</summary>
    public class SceneTransitionTrigger : MonoBehaviour
    {
        [SerializeField] private string _sceneToLoad;

        private ISceneLoader _sceneLoader;

        [Inject]
        public void Construct(ISceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (PlayerDetection.IsPlayer(other))
                _sceneLoader.LoadAsync(_sceneToLoad).Forget();
        }
    }
}
