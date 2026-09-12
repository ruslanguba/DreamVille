using System;
using UnityEngine;

using DreamVille.Configs;

namespace DreamVille.Scoring
{
    /// <summary>
    /// Accumulates score for the current run and unlocks stars as thresholds from
    /// <see cref="StarRulesSO"/> are crossed. The best star count ever reached for a
    /// level is persisted to PlayerPrefs and never regresses on replay.
    /// </summary>
    public class StarProgressService
    {
        private const string StarsKeySuffix = "_Stars";

        public event Action<int> OnStarUnlocked;
        public event Action<int, float> OnStarProgressChanged;

        private readonly StarRulesSO _rules;
        private float _totalScore;
        private int _currentStarIndex;

        public int UnlockedStars => _currentStarIndex;

        private string SaveKey => _rules.levelId + StarsKeySuffix;

        public StarProgressService(StarRulesSO rules)
        {
            _rules = rules;
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
            OnStarProgressChanged?.Invoke(_currentStarIndex, progress);
        }

        private bool CanUnlockNextStar()
        {
            if (_currentStarIndex >= _rules.starThresholds.Length)
                return false;

            return _totalScore >= _rules.starThresholds[_currentStarIndex];
        }

        private void UnlockStar()
        {
            OnStarUnlocked?.Invoke(_currentStarIndex);
            _currentStarIndex++;
            SaveProgress();
        }

        /// <summary>Persists the star count only when it improves on the previously saved best.</summary>
        private void SaveProgress()
        {
            int stored = PlayerPrefs.GetInt(SaveKey, 0);
            if (_currentStarIndex > stored)
            {
                PlayerPrefs.SetInt(SaveKey, _currentStarIndex);
                PlayerPrefs.Save();
            }
        }
    }
}
