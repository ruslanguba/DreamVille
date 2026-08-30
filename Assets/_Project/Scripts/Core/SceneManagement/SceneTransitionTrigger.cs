using UnityEngine;
using Cysharp.Threading.Tasks;
using Zenject;

using DreamVille.Gameplay.Player;

namespace DreamVille.Core.SceneManagement
{
    public class SceneTransitionTrigger : MonoBehaviour
    {
        [SerializeField] private string _sceneToLoad;

        private SceneLoader _sceneLoader;

        [Inject]
        public void Construct(SceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerSafePosition>() != null)
            {
                _sceneLoader.LoadAsync(_sceneToLoad).Forget();
            }
        }
    }
}
