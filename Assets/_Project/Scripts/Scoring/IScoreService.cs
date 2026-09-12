using System;

namespace DreamVille.Scoring
{
    /// <summary>Accumulates score for the current run.</summary>
    public interface IScoreService
    {
        /// <summary>Raised with the added delta whenever score is added.</summary>
        event Action<float> ScoreAdded;

        /// <summary>Raised with the running total whenever score is added.</summary>
        event Action<float> TotalScoreChanged;

        float TotalScore { get; }

        void AddScore(float score);

        /// <summary>Converts an enemy kill's ricochet count into score and adds it.</summary>
        void HandleEnemyKilled(int ricochetCount);
    }
}
