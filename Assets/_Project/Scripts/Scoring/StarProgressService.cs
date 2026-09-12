using System;
using System.Collections.Generic;
using UnityEngine;

using DreamVille.Configs;
using DreamVille.Persistence;

namespace DreamVille.Scoring
{
    /// <summary>
    /// Accumulates score for the current run and unlocks stars as the thresholds of the
    /// level's <see cref="StarRules"/> are crossed. The best star count ever reached for a
    /// level is persisted through <see cref="IProgressRepository"/> and never regresses on replay.
    /// </summary>
    public class StarProgressService : IStarProgressService
    {
        public event Action<int> StarUnlocked;
        public event Action<int, float> StarProgressChanged;

        private readonly IReadOnlyList<float> _thresholds;
        private readonly string _levelId;
        private readonly IProgressRepository _progress;
        private float _totalScore;
        private int _currentStarIndex;

        public int UnlockedStars => _currentStarIndex;

        public StarProgressService(LevelDefinition level, IProgressRepository progress)
        {
            _thresholds = level.StarRules.Thresholds;
            _levelId = level.LevelId;
            _progress = progress;
        }

        public void AddScore(float score)
        {
            _totalScore += score;
            UpdateCurrentStarProgress();

            while (CanUnlockNextStar())
            {
                UnlockStar();
            }
        }

        private void UpdateCurrentStarProgress()
        {
            if (_currentStarIndex >= _thresholds.Count)
                return;

            float prevThreshold = _currentStarIndex == 0
                ? 0
                : _thresholds[_currentStarIndex - 1];

            float nextThreshold = _thresholds[_currentStarIndex];

            float progress =
                Mathf.Clamp01((_totalScore - prevThreshold) /
                              (nextThreshold - prevThreshold));
            StarProgressChanged?.Invoke(_currentStarIndex, progress);
        }

        private bool CanUnlockNextStar()
        {
            if (_currentStarIndex >= _thresholds.Count)
                return false;

            return _totalScore >= _thresholds[_currentStarIndex];
        }

        private void UnlockStar()
        {
            StarUnlocked?.Invoke(_currentStarIndex);
            _currentStarIndex++;
            SaveProgress();
        }

        private void SaveProgress()
        {
            _progress.SetStarsIfHigher(_levelId, _currentStarIndex);
        }
    }
}
