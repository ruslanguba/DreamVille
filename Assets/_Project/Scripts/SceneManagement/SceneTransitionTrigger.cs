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

        [Tooltip("Deprecated fallback for triggers that still name their scene; removed once every trigger has a target.")]
        [SerializeField] private string _sceneToLoad;

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

            if (_target != null)
            {
                _sceneLoader.LoadAsync(_target.SceneName).Forget();
                return;
            }

            Debug.LogWarning($"[{nameof(SceneTransitionTrigger)}] '{name}' has no target scene definition; falling back to the scene name '{_sceneToLoad}'.", this);

            if (!string.IsNullOrEmpty(_sceneToLoad))
                _sceneLoader.LoadAsync(_sceneToLoad).Forget();
        }
    }
}
