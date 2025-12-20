using System;
using UnityEngine;

public class ScoreService
{
    public event Action<float> OnScoreAdded;
    public event Action<float> OnScoreChanged;

    private float _totalScore;
    private readonly float _scorePerKill = 10f;

    public void AddScore(int collisionsCount)
    {
        float score = collisionsCount * _scorePerKill;
        _totalScore += score;

        OnScoreAdded?.Invoke(score);
        OnScoreChanged?.Invoke(_totalScore);
    }
}
