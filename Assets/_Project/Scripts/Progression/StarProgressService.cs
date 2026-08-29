using System;
using UnityEngine;

using DreamVille.Configs;

namespace DreamVille.Progression
{
    public class StarProgressService
    {
        public event Action<int> OnStarUnlocked;
        public event Action<int, float> OnStarProgressChanged;

        private readonly StarRulesSO _rules;
        private float _totalScore;
        private int _currentStarIndex;

        public int UnlockedStars => _currentStarIndex;

        public StarProgressService(StarRulesSO rules)
        {
            _rules = rules;
        }

        public void AddScore(float score)
        {

            _totalScore += score;
            Debug.Log("Тотал в StarProgressService " + _totalScore);
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
            Debug.Log("UpdateCurrentStarProgress " + progress);
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

        private void SaveProgress()
        {
            PlayerPrefs.SetInt(_rules.levelId + "_Stars", _currentStarIndex);
            PlayerPrefs.Save();
        }

        //private void LoadProgress()
        //{
        //    _currentStarIndex =
        //        PlayerPrefs.GetInt(_rules.levelId + "_Stars", 0);
        //}
    }
}
