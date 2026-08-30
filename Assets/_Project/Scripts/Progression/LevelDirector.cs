using System;
using Zenject;

using DreamVille.Configs;
using DreamVille.Gameplay.Enemies;

namespace DreamVille.Progression
{
    /// <summary>
    /// Сценарий завершения уровня целиком в одном месте: победа взводит финальную зону,
    /// конец финального диалога открывает проход. Участники друг о друге не знают.
    /// </summary>
    public class LevelDirector : IInitializable, IDisposable
    {
        private readonly EnemyRegistry _enemies;
        private readonly LevelUnlockService _unlocks;
        private readonly LevelActors _actors;
        private readonly StarRulesSO _rules;

        public LevelDirector(
            EnemyRegistry enemies,
            LevelUnlockService unlocks,
            StarRulesSO rules,
            [InjectOptional] LevelActors actors)
        {
            _enemies = enemies;
            _unlocks = unlocks;
            _rules = rules;
            _actors = actors;
        }

        private FinalZone FinalZone => _actors != null ? _actors.FinalZone : null;

        public void Initialize()
        {
            _enemies.OnAllEnemiesKilled += OnAllEnemiesKilled;

            // Хаб без финальной зоны сценария завершения не имеет — придумывать его не нужно.
            if (FinalZone == null)
                return;

            FinalZone.DialogueFinished += OnDialogueFinished;

            // Зона есть, а врагов нет: события «все убиты» уже не будет, засчитываем сразу.
            if (_enemies.RemainingEnemies == 0)
                OnAllEnemiesKilled();
        }

        public void Dispose()
        {
            _enemies.OnAllEnemiesKilled -= OnAllEnemiesKilled;

            if (FinalZone != null)
                FinalZone.DialogueFinished -= OnDialogueFinished;
        }

        private void OnAllEnemiesKilled()
        {
            _unlocks.MarkCompleted(_rules.levelId);

            if (FinalZone != null)
                FinalZone.Arm();
        }

        private void OnDialogueFinished()
        {
            if (FinalZone != null)
                FinalZone.OpenPortal();
        }
    }
}
