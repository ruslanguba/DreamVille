using System;

using DreamVille.Configs;
using DreamVille.Core.Events;

namespace DreamVille.Scoring
{
    /// <summary>The run's score: a kill is worth ScorePerKill times its hit count (a direct kill counts 1), other sources add points directly; raises the added amount.</summary>
    public sealed class ScoreService : IScoreService
    {
        public event Action<float> ScoreAdded;

        private readonly ScoreRules _rules;
        private float _totalScore;

        public float TotalScore => _totalScore;

        public ScoreService(ScoreRules rules)
        {
            _rules = rules;
        }

        public void HandleEnemyKilled(int hitCount)
        {
            float score = CalculateScore(hitCount);
            AddScore(score);
        }

        private float CalculateScore(int hitCount)
        {
            float score = _rules.ScorePerKill * hitCount;
            return score;
        }

        public void AddScore(float score)
        {
            _totalScore += score;
            // Isolated: the HUD and the star progress listen independently.
            IsolatedEvent.Raise(ScoreAdded, score);
        }
    }
}
