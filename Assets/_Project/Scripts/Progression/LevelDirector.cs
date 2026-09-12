using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;

using DreamVille.Configs;
using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.NPC;

namespace DreamVille.Progression
{
    /// <summary>
    /// The level's completion scenario in one place: winning arms the final zone, the final
    /// dialogue starting and finishing drives the final-zone NPC reaction, and the dialogue
    /// finishing opens the path onward. Participants do not know about each other.
    /// </summary>
    public class LevelDirector : IInitializable, IDisposable
    {
        private readonly EnemyRegistry _enemies;
        private readonly LevelUnlockService _unlocks;
        private readonly LevelActors _actors;
        private readonly StarRulesSO _rules;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

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
        private INpcReactions FinalNpc => _actors != null ? _actors.FinalNpc : null;

        public void Initialize()
        {
            _enemies.OnAllEnemiesKilled += OnAllEnemiesKilled;

            // A hub without a final zone has no completion scenario to run.
            if (FinalZone == null)
                return;

            FinalZone.DialogueStarted += OnFinalDialogueStarted;
            FinalZone.DialogueFinished += OnFinalDialogueFinished;

            // The zone exists but there are no enemies: "all killed" will never fire, so count it now.
            if (_enemies.RemainingEnemies == 0)
                CompleteAfterSceneStartAsync(_lifetime.Token).Forget();
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();

            _enemies.OnAllEnemiesKilled -= OnAllEnemiesKilled;

            if (FinalZone == null)
                return;

            FinalZone.DialogueStarted -= OnFinalDialogueStarted;
            FinalZone.DialogueFinished -= OnFinalDialogueFinished;
        }

        /// <summary>
        /// Zenject runs <see cref="Initialize"/> before the scene's MonoBehaviours have started,
        /// so an NPC greeting applied here would be overwritten by its own Start. Wait for the
        /// first Update, when every component has initialised its authored pose.
        /// </summary>
        private async UniTaskVoid CompleteAfterSceneStartAsync(CancellationToken token)
        {
            try
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
                OnAllEnemiesKilled();
            }
            catch (OperationCanceledException)
            {
                // The scene context was disposed before the first frame; nothing to complete.
            }
        }

        private void OnAllEnemiesKilled()
        {
            _unlocks.MarkCompleted(_rules.levelId);

            if (FinalZone != null)
                FinalZone.Arm();

            FinalNpc?.SetGreeting();
        }

        private void OnFinalDialogueStarted()
        {
            FinalNpc?.SetApplause();
        }

        private void OnFinalDialogueFinished()
        {
            FinalNpc?.SetIdle();

            if (FinalZone != null)
                FinalZone.OpenPortal();
        }
    }
}
