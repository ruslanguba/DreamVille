using UnityEngine;

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
