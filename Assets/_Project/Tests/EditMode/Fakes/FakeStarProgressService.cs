using System;
using System.Collections.Generic;

using DreamVille.Scoring;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>A star meter that records added scores and raises its events only when a test asks.</summary>
    public sealed class FakeStarProgressService : IStarProgressService
    {
        private readonly List<float> _addedScores = new List<float>();

        public event Action<int> StarUnlocked;
        public event Action<int, float> StarProgressChanged;

        public int UnlockedStars { get; set; }

        public IReadOnlyList<float> AddedScores => _addedScores;

        public void AddScore(float score)
        {
            _addedScores.Add(score);
        }

        public void RaiseProgress(int starIndex, float normalized)
        {
            StarProgressChanged?.Invoke(starIndex, normalized);
        }

        public void RaiseUnlocked(int starIndex)
        {
            StarUnlocked?.Invoke(starIndex);
        }
    }
}
