using System.Collections.Generic;

using DreamVille.Configs.Enemies;
using DreamVille.Gameplay.Enemies.States;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Hand-written <see cref="IEnemyState"/> double for <see cref="EnemyStateMachine"/> tests: records Enter/Exit order into a shared log and counts every call.</summary>
    public sealed class RecordingState : IEnemyState
    {
        private readonly List<string> _log;

        public RecordingState(EnemyStateId id, List<string> log = null)
        {
            Id = id;
            _log = log;
        }

        public EnemyStateId Id { get; }

        public int EnterCount { get; private set; }

        public int ExitCount { get; private set; }

        public int TickCount { get; private set; }

        public int FixedTickCount { get; private set; }

        public void Enter()
        {
            EnterCount++;
            _log?.Add($"Enter:{Id}");
        }

        public void Tick(float deltaTime) => TickCount++;

        public void FixedTick(float fixedDeltaTime) => FixedTickCount++;

        public void Exit()
        {
            ExitCount++;
            _log?.Add($"Exit:{Id}");
        }
    }
}
