using System;

namespace DreamVille.Scoring
{
    /// <summary>Unlocks stars as the current run's score crosses the level's thresholds.</summary>
    public interface IStarProgressService
    {
        /// <summary>Raised with the 0-based index of the star that was just unlocked, before the unlocked count and the save are updated.</summary>
        event Action<int> StarUnlocked;

        /// <summary>Raised with the current star index and its normalized progress (0..1) toward the next threshold.</summary>
        event Action<int, float> StarProgressChanged;

        int UnlockedStars { get; }

        void AddScore(float score);
    }
}
