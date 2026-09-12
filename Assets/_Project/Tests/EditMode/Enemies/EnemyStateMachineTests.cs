using System.Collections.Generic;
using NUnit.Framework;

using DreamVille.Gameplay.Enemies.StateMachine;

namespace DreamVille.Tests.EditMode.Enemies
{
    public class EnemyStateMachineTests
    {
        private sealed class FakeState : BaseState
        {
            private readonly List<string> _log;
            private readonly string _name;

            public int EnterCount;
            public int ExitCount;

            public FakeState(List<string> log = null, string name = null) : base(null, null)
            {
                _log = log;
                _name = name;
            }

            public override void Enter()
            {
                EnterCount++;
                _log?.Add($"Enter:{_name}");
            }

            public override void Exit()
            {
                ExitCount++;
                _log?.Add($"Exit:{_name}");
            }
        }

        [Test]
        public void Initialize_SetsCurrentStateAndCallsEnter()
        {
            var stateMachine = new EnemyStateMachine();
            var state = new FakeState();

            stateMachine.Initialize(state);

            Assert.That(stateMachine.CurrentState, Is.SameAs(state));
            Assert.That(state.EnterCount, Is.EqualTo(1));
        }

        [Test]
        public void ChangeState_ExitsOldStateThenEntersNewStateInThatOrder()
        {
            var log = new List<string>();
            var stateMachine = new EnemyStateMachine();
            var oldState = new FakeState(log, "old");
            var newState = new FakeState(log, "new");
            stateMachine.Initialize(oldState);
            log.Clear();

            stateMachine.ChangeState(newState);

            Assert.That(log, Is.EqualTo(new[] { "Exit:old", "Enter:new" }));
            Assert.That(stateMachine.CurrentState, Is.SameAs(newState));
        }
    }
}
