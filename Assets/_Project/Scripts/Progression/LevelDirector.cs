using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

using DreamVille.Configs;
using DreamVille.Core.Async;
using DreamVille.Core.Scenes;
using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Npc;
using DreamVille.Persistence;

namespace DreamVille.Progression
{
    /// <summary>
    /// The level's completion scenario in one place: winning arms the final zone, the final
    /// dialogue starting and finishing drives the final-zone NPC reaction, and the dialogue
    /// finishing opens the path onward before the NPC reacts. Participants do not know about each other.
    /// A level without enemies to beat (a hub) is completed from outside through <see cref="ILevelCompletion"/>.
    /// </summary>
    public sealed class LevelDirector : ILevelCompletion, IInitializable, IDisposable
    {
        private readonly IEnemyRegistry _enemies;
        private readonly LevelDefinition _level;
        private readonly IProgressRepository _progress;
        private readonly ILevelCast _cast;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        public LevelDirector(
            IEnemyRegistry enemies,
            LevelDefinition level,
            IProgressRepository progress,
            ILevelCast cast)
        {
            _enemies = enemies;
            _level = level;
            _progress = progress;

            // Required: without the cast the finale would silently never run.
            _cast = cast ?? throw new ArgumentNullException(nameof(cast));
        }

        private IFinalZone FinalZone => _cast.FinalZone;
        private INpcReactions FinalNpc => _cast.FinalNpc;

        public void Initialize()
        {
            _enemies.AllEnemiesKilled += OnAllEnemiesKilled;

            // A hub without a final zone has no completion scenario to run.
            if (FinalZone == null)
                return;

            FinalZone.DialogueStarted += OnFinalDialogueStarted;
            FinalZone.DialogueEnded += OnFinalDialogueEnded;

            // The zone exists but there are no enemies: "all killed" will never fire, so count it now.
            if (_enemies.RemainingEnemies == 0)
                CompleteAfterSceneStartAsync(_lifetime.Token).Forget();
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();

            _enemies.AllEnemiesKilled -= OnAllEnemiesKilled;

            if (FinalZone == null)
                return;

            FinalZone.DialogueStarted -= OnFinalDialogueStarted;
            FinalZone.DialogueEnded -= OnFinalDialogueEnded;
        }

        /// <inheritdoc />
        public void CompleteLevel()
        {
            _progress.MarkCompleted(_level.LevelId);
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
                await GameTime.NextFrameAsync(token);
                OnAllEnemiesKilled();
            }
            catch (OperationCanceledException)
            {
                // The scene context was disposed before the first frame; nothing to complete.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private void OnAllEnemiesKilled()
        {
            CompleteLevel();

            if (FinalZone != null)
                FinalZone.Arm();

            FinalNpc?.SetGreeting();
        }

        private void OnFinalDialogueStarted()
        {
            FinalNpc?.SetApplause();
        }

        private void OnFinalDialogueEnded()
        {
            // The path onward first: the NPC's reaction is decoration and must never block it.
            if (FinalZone != null)
                FinalZone.OpenPortal();

            FinalNpc?.SetIdle();
        }
    }
}
