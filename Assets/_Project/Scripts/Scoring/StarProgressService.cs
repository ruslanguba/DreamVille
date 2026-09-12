using System;
using UnityEngine;

using DreamVille.Configs;
using DreamVille.Persistence;

namespace DreamVille.Scoring
{
    /// <summary>
    /// Accumulates score for the current run and unlocks stars as thresholds from
    /// <see cref="StarRulesSO"/> are crossed. The best star count ever reached for a
    /// level is persisted through <see cref="IProgressRepository"/> and never regresses on replay.
    /// </summary>
    public class StarProgressService : IStarProgressService
    {
        public event Action<int> StarUnlocked;
        public event Action<int, float> StarProgressChanged;

        private readonly StarRulesSO _rules;
        private readonly IProgressRepository _progress;
        private float _totalScore;
        private int _currentStarIndex;

        public int UnlockedStars => _currentStarIndex;

        public StarProgressService(StarRulesSO rules, IProgressRepository progress)
        {
            _rules = rules;
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
            if (_currentStarIndex >= _rules.starThresholds.Length)
                return;

            float prevThreshold = _currentStarIndex == 0
                ? 0
                : _rules.starThresholds[_currentStarIndex - 1];

            float nextThreshold = _rules.starThresholds[_currentStarIndex];

            float progress =
                Mathf.Clamp01((_totalScore - prevThreshold) /
                              (nextThreshold - prevThreshold));
            StarProgressChanged?.Invoke(_currentStarIndex, progress);
        }

        private bool CanUnlockNextStar()
        {
            if (_currentStarIndex >= _rules.starThresholds.Length)
                return false;

            return _totalScore >= _rules.starThresholds[_currentStarIndex];
        }

        private void UnlockStar()
        {
            StarUnlocked?.Invoke(_currentStarIndex);
            _currentStarIndex++;
            SaveProgress();
        }

        private void SaveProgress()
        {
            _progress.SetStarsIfHigher(_rules.levelId, _currentStarIndex);
        }
    }
}
