using System;
using UnityEngine;

using DreamVille.Configs;

namespace DreamVille.Scoring
{
    /// <summary>The run's score: a kill is worth ScorePerKill times its ricochets, other sources add points directly; raises the added amount and the new total.</summary>
    public class ScoreService : IScoreService
    {
        public event Action<float> ScoreAdded;
        public event Action<float> TotalScoreChanged;

        private readonly ScoreRules _rules;
        private float _totalScore;

        public float TotalScore => _totalScore;

        public ScoreService(ScoreRules rules)
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
            float score = _rules.ScorePerKill * ricochets;
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
