using UnityEngine;
using Zenject;

using DreamVille.Gameplay.Interactables;
using DreamVille.Scoring;

namespace DreamVille.Gameplay.Player
{
    public class PlayerCollector : MonoBehaviour
    {
        private ScoreService _scoreService;

        // Очки настроены не на всех уровнях: там, где нет ProgressionInstaller, сбор просто не начисляет.
        [Inject]
        public void Construct([InjectOptional] ScoreService scoreService)
        {
            _scoreService = scoreService;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out Collectable collectable))
                return;

            collectable.Collect(transform);

            if (_scoreService == null)
            {
                Debug.LogWarning($"{nameof(PlayerCollector)} на '{name}': нет {nameof(ScoreService)} — в сцене отсутствует SceneContext.", this);
                return;
            }

            _scoreService.AddScore(collectable.ScoreToAdd);
        }
    }
}
