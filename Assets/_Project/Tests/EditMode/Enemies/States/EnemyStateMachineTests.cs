using System;
using System.Collections.Generic;
using NUnit.Framework;

using DreamVille.Configs.Enemies;
using DreamVille.Gameplay.Enemies.States;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Enemies.States
{
    /// <summary>
    /// Transition-table guards of the new <see cref="EnemyStateMachine"/> (namespace
    /// <c>Enemies.States</c>) against hand-written <see cref="RecordingState"/> doubles. Distinct
    /// from <see cref="DreamVille.Tests.EditMode.Enemies.EnemyStateMachineTests"/>, which still
    /// covers the old machine in <c>Enemies.StateMachine</c> until batch 2 removes it.
    /// </summary>
    public class EnemyStateMachineTests
    {
        [Test]
        public void Start_EntersInitialStateAndRaisesStateChanged()
        {
            var idle = new RecordingState(EnemyStateId.Idle);
            var machine = new EnemyStateMachine(new IEnemyState[] { idle });
            (EnemyStateId Previous, EnemyStateId Next)? raised = null;
            machine.StateChanged += (previous, next) => raised = (previous, next);

            machine.Start(EnemyStateId.Idle);

            Assert.That(machine.CurrentState, Is.SameAs(idle));
            Assert.That(idle.EnterCount, Is.EqualTo(1));
            Assert.That(raised, Is.EqualTo((EnemyStateId.Idle, EnemyStateId.Idle)));
        }

        [Test]
        public void ChangeState_ExitsOldStateThenEntersNewStateInThatOrder()
        {
            var log = new List<string>();
            var idle = new RecordingState(EnemyStateId.Idle, log);
            var patrol = new RecordingState(EnemyStateId.Patrol, log);
            var machine = new EnemyStateMachine(new IEnemyState[] { idle, patrol });
            machine.Start(EnemyStateId.Idle);
            log.Clear();

            machine.ChangeState(EnemyStateId.Patrol);

            Assert.That(log, Is.EqualTo(new[] { "Exit:Idle", "Enter:Patrol" }));
            Assert.That(machine.CurrentState, Is.SameAs(patrol));
        }

        [Test]
        public void ChangeState_ToCurrentState_ReturnsFalseAndDoesNotReEnter()
        {
            var idle = new RecordingState(EnemyStateId.Idle);
            var machine = new EnemyStateMachine(new IEnemyState[] { idle });
            machine.Start(EnemyStateId.Idle);

            bool changed = machine.ChangeState(EnemyStateId.Idle);

            Assert.That(changed, Is.False);
            Assert.That(idle.EnterCount, Is.EqualTo(1));
            Assert.That(idle.ExitCount, Is.EqualTo(0));
        }

        [Test]
        public void ChangeState_AfterDead_IsRefused()
        {
            var idle = new RecordingState(EnemyStateId.Idle);
            var dead = new RecordingState(EnemyStateId.Dead);
            var machine = new EnemyStateMachine(new IEnemyState[] { idle, dead });
            machine.Start(EnemyStateId.Idle);
            machine.ChangeState(EnemyStateId.Dead);

            bool changed = machine.ChangeState(EnemyStateId.Idle);

            Assert.That(changed, Is.False);
            Assert.That(machine.CurrentState, Is.SameAs(dead));
            Assert.That(idle.EnterCount, Is.EqualTo(1));
        }

        [Test]
        public void ChangeState_WithUnregisteredId_Throws()
        {
            var idle = new RecordingState(EnemyStateId.Idle);
            var machine = new EnemyStateMachine(new IEnemyState[] { idle });
            machine.Start(EnemyStateId.Idle);

            Assert.Throws<ArgumentOutOfRangeException>(() => machine.ChangeState(EnemyStateId.Chase));
        }

        [Test]
        public void Constructor_WithDuplicateIds_Throws()
        {
            var first = new RecordingState(EnemyStateId.Idle);
            var second = new RecordingState(EnemyStateId.Idle);

            Assert.Throws<ArgumentException>(() => new EnemyStateMachine(new IEnemyState[] { first, second }));
        }

        [Test]
        public void Tick_BeforeStart_DoesNothing()
        {
            var idle = new RecordingState(EnemyStateId.Idle);
            var machine = new EnemyStateMachine(new IEnemyState[] { idle });

            machine.Tick(0.5f);
            machine.FixedTick(0.5f);

            Assert.That(idle.TickCount, Is.EqualTo(0));
            Assert.That(idle.FixedTickCount, Is.EqualTo(0));
            Assert.That(machine.IsRunning, Is.False);
        }

        [Test]
        public void StateChanged_ReportsPreviousAndNextIds()
        {
            var idle = new RecordingState(EnemyStateId.Idle);
            var patrol = new RecordingState(EnemyStateId.Patrol);
            var machine = new EnemyStateMachine(new IEnemyState[] { idle, patrol });
            machine.Start(EnemyStateId.Idle);
            (EnemyStateId Previous, EnemyStateId Next)? raised = null;
            machine.StateChanged += (previous, next) => raised = (previous, next);

            machine.ChangeState(EnemyStateId.Patrol);

            Assert.That(raised, Is.EqualTo((EnemyStateId.Idle, EnemyStateId.Patrol)));
        }
    }
}
