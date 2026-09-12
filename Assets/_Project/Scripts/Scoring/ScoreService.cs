using System;
using UnityEngine;

using DreamVille.Configs;

namespace DreamVille.Scoring
{
    public class ScoreService : IScoreService
    {
        public event Action<float> ScoreAdded;
        public event Action<float> TotalScoreChanged;

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
            _totalScore += score;
            ScoreAdded?.Invoke(score);
            TotalScoreChanged?.Invoke(_totalScore);
        }
    }
}
