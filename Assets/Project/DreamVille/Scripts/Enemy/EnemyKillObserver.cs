using UnityEngine;

public class EnemyKillObserver : MonoBehaviour
{
    private ScoreService _scoreService;

    public void Bind(ScoreService scoreService)
    {
        _scoreService = scoreService;
    }

    private void OnEnable()
    {
        foreach (var enemy in FindObjectsByType<EnemyHealth>(0))
        {
            enemy.OnDeath += OnEnemyKilled;
        }
    }

    private void OnEnemyKilled(EnemyHealth enemy, int collisionsCount)
    {
        _scoreService.AddScore(collisionsCount);
    }
}
