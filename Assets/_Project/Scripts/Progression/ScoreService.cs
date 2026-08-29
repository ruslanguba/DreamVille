using System;
using UnityEngine;

using DreamVille.Configs;

namespace DreamVille.Progression
{
    public class ScoreService
    {
        public event Action<float> OnScoreAdded;
        public event Action<float> OnTotalScoreChanged;

        private readonly ScoreRulesSO _rules;
        private float _totalScore;

        public float TotalScore => _totalScore;

        public ScoreService(ScoreRulesSO rules)
        {
            _rules = rules;
        }

        public void HandleEnemyKilled(int ricochetCount)
        {
            float score = CalculateScore(ricochetCount);
            AddScore(score);
        }

        private float CalculateScore(int ricochets)
        {
            float score = _rules.scorePerKill * ricochets;
            return score;
        }

        public void AddScore(float score)
        {
            Debug.Log("ScoreService " + score);
            _totalScore += score;
            OnScoreAdded?.Invoke(score);
            OnTotalScoreChanged?.Invoke(_totalScore);
        }
    }
}
