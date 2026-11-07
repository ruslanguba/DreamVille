using System;

using DreamVille.Scoring;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>A score service without rules: it counts kill calls and raises events when a test adds score.</summary>
    public sealed class FakeScoreService : IScoreService
    {
        public event Action<float> ScoreAdded;

        public float TotalScore { get; private set; }

        public int HandleEnemyKilledCalls { get; private set; }

        public void AddScore(float score)
        {
            TotalScore += score;
            ScoreAdded?.Invoke(score);
        }

        public void HandleEnemyKilled(int hitCount)
        {
            HandleEnemyKilledCalls++;
        }
    }
}
