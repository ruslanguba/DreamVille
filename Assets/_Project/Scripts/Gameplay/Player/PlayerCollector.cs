using UnityEngine;

using DreamVille.Gameplay.Interactables;
using DreamVille.Progression;

namespace DreamVille.Gameplay.Player
{
    public class PlayerCollector : MonoBehaviour
    {
        // TODO Refactor
        ScoreService _scoreService;

        public void Bind(ScoreService scoreService)
        {
            _scoreService = scoreService;
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.TryGetComponent(out Collectable collectable))
            {
                collectable.Collect(transform);
                _scoreService.AddScore(collectable.ScoreToAdd);    
            }
        }
    }
}
