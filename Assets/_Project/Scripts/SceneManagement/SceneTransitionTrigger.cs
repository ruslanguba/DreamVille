using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

using DreamVille.Configs;
using DreamVille.Core.Players;

namespace DreamVille.SceneManagement
{
    /// <summary>Loads the target scene when the player enters this trigger volume.</summary>
    public class SceneTransitionTrigger : MonoBehaviour
    {
        [SerializeField] private SceneDefinition _target;

        private ISceneLoader _sceneLoader;

        [Inject]
        public void Construct(ISceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!PlayerDetection.IsPlayer(other))
                return;

            if (_target == null)
            {
                Debug.LogError($"[{nameof(SceneTransitionTrigger)}] '{name}' has no target scene definition assigned.", this);
                return;
            }

            _sceneLoader.LoadAsync(_target.SceneName).Forget();
        }
    }
}
