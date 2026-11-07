using System;

namespace DreamVille.Scoring
{
    /// <summary>Accumulates score for the current run.</summary>
    public interface IScoreService
    {
        /// <summary>Raised with the added delta whenever score is added.</summary>
        event Action<float> ScoreAdded;

        float TotalScore { get; }

        void AddScore(float score);

        /// <summary>Converts an enemy kill's hit count (1 for a direct kill) into score and adds it.</summary>
        void HandleEnemyKilled(int hitCount);
    }
}
