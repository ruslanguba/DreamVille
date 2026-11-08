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
    public sealed class StarProgressService : IStarProgressService
    {
        public event Action<int> StarUnlocked;
        public event Action<int, float> StarProgressChanged;

        private readonly IReadOnlyList<float> _thresholds;
        private readonly string _levelId;
        private readonly IProgressRepository _progress;
        private float _totalScore;
        private int _currentStarIndex;

        /// <exception cref="ArgumentException">The level's star thresholds are not positive and strictly ascending.</exception>
        public StarProgressService(LevelDefinition level, IProgressRepository progress)
        {
            _thresholds = level.StarRules.Thresholds;
            _levelId = level.LevelId;
            _progress = progress;

            ValidateThresholds(level);
        }

        /// <summary>
        /// Adds <paramref name="score"/>, raises progress for the current star and unlocks every star
        /// whose threshold is now reached. When a star unlocks, progress is raised again for the new
        /// current star, so a score that crosses a threshold shows its overflow on the next meter at once.
        /// </summary>
        public void AddScore(float score)
        {
            _totalScore += score;
            UpdateCurrentStarProgress();

            bool unlockedAny = false;
            while (CanUnlockNextStar())
            {
                UnlockStar();
                unlockedAny = true;
            }

            if (unlockedAny)
                UpdateCurrentStarProgress();
        }

        // A threshold at or below its predecessor would divide by zero (or go negative) in the progress ratio.
        private void ValidateThresholds(LevelDefinition level)
        {
            float previous = 0f;
            for (int i = 0; i < _thresholds.Count; i++)
            {
                if (!(_thresholds[i] > previous))
                {
                    throw new ArgumentException(
                        $"Star thresholds of level '{level.LevelId}' must be positive and strictly ascending, but #{i} is {_thresholds[i]} after {previous}.",
                        nameof(level));
                }

                previous = _thresholds[i];
            }
        }

        private void UpdateCurrentStarProgress()
        {
            if (_currentStarIndex >= _thresholds.Count)
                return;

            float previousThreshold = _currentStarIndex == 0
                ? 0
                : _thresholds[_currentStarIndex - 1];

            float nextThreshold = _thresholds[_currentStarIndex];

            float progress =
                Mathf.Clamp01((_totalScore - previousThreshold) /
                              (nextThreshold - previousThreshold));
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
