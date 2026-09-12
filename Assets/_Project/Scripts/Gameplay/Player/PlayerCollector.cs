using UnityEngine;
using Zenject;

using DreamVille.Gameplay.Interactables;
using DreamVille.Scoring;

namespace DreamVille.Gameplay.Player
{
    public class PlayerCollector : MonoBehaviour
    {
        private IScoreService _scoreService;

        [Inject]
        public void Construct(IScoreService scoreService)
        {
            _scoreService = scoreService;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out Collectable collectable))
                return;

            collectable.Collect(transform);
            _scoreService.AddScore(collectable.ScoreToAdd);
        }
    }
}
