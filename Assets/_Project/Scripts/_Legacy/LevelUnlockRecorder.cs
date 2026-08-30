using System;
using Zenject;

using DreamVille.Configs;
using DreamVille.Progression;

namespace DreamVille.Legacy
{
    /// <summary>
    /// Фиксирует прохождение уровня. Отдельный слушатель, чтобы сервис финала
    /// не знал ни про PlayerPrefs, ни про идентификаторы уровней.
    /// </summary>
    public class LevelUnlockRecorder : IInitializable, IDisposable
    {
        private readonly LevelOutroService _outro;
        private readonly LevelUnlockService _unlocks;
        private readonly StarRulesSO _rules;

        public LevelUnlockRecorder(LevelOutroService outro, LevelUnlockService unlocks, StarRulesSO rules)
        {
            _outro = outro;
            _unlocks = unlocks;
            _rules = rules;
        }

        public void Initialize() => _outro.Started += OnOutroStarted;

        public void Dispose() => _outro.Started -= OnOutroStarted;

        private void OnOutroStarted() => _unlocks.MarkCompleted(_rules.levelId);
    }
}
