using UnityEngine;
using Zenject;

using DreamVille.Gameplay.Player;

namespace DreamVille.Core.SceneManagement
{
    public class SceneTransitionTrigger : MonoBehaviour
    {
        [SerializeField] private string _sceneToLoad;

        private SceneTransitionManager _transitions;

        [Inject]
        public void Construct(SceneTransitionManager transitions)
        {
            _transitions = transitions;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerSafePosition>() != null)
            {
                var request = new SceneTransitionRequest(_sceneToLoad);
                _transitions.Transition(request);
            }
        }
    }
}
