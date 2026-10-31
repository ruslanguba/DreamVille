using System;
using NUnit.Framework;

using DreamVille.Gameplay;
using DreamVille.Gameplay.Npc;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Npc
{
    /// <summary>Mood flags and start-pose precedence of <see cref="NpcAnimationState"/> against a <see cref="FakeNpcAnimatorParameters"/>.</summary>
    public class NpcAnimationStateTests
    {
        private FakeNpcAnimatorParameters _parameters;
        private NpcAnimationState _state;

        [SetUp]
        public void SetUp()
        {
            _parameters = new FakeNpcAnimatorParameters();
            _state = new NpcAnimationState(_parameters);
        }

        [TestCase(NpcState.Idle)]
        [TestCase(NpcState.Talk)]
        [TestCase(NpcState.Greeting)]
        [TestCase(NpcState.Applause)]
        public void Set_EachState_RaisesOnlyItsOwnMoodFlag(NpcState state)
        {
            _state.Set(state);

            Assert.That(_parameters.WasWritten(AnimParams.IsTalk), Is.True);
            Assert.That(_parameters.WasWritten(AnimParams.IsGreeting), Is.True);
            Assert.That(_parameters.WasWritten(AnimParams.IsApplause), Is.True);
            Assert.That(_parameters.ValueOf(AnimParams.IsTalk), Is.EqualTo(state == NpcState.Talk));
            Assert.That(_parameters.ValueOf(AnimParams.IsGreeting), Is.EqualTo(state == NpcState.Greeting));
            Assert.That(_parameters.ValueOf(AnimParams.IsApplause), Is.EqualTo(state == NpcState.Applause));
        }

        [Test]
        public void Set_AnyState_DoesNotWriteSwim()
        {
            _state.Set(NpcState.Idle);
            _state.Set(NpcState.Talk);
            _state.Set(NpcState.Greeting);
            _state.Set(NpcState.Applause);

            Assert.That(_parameters.WasWritten(AnimParams.IsSwim), Is.False);
        }

        [Test]
        public void ApplyStartPose_WhenNothingRequested_WritesStartStateAndSwim()
        {
            _state.ApplyStartPose(NpcState.Greeting, swim: true);

            Assert.That(_parameters.ValueOf(AnimParams.IsGreeting), Is.True);
            Assert.That(_parameters.ValueOf(AnimParams.IsTalk), Is.False);
            Assert.That(_parameters.ValueOf(AnimParams.IsApplause), Is.False);
            Assert.That(_parameters.WasWritten(AnimParams.IsSwim), Is.True);
            Assert.That(_parameters.ValueOf(AnimParams.IsSwim), Is.True);
            Assert.That(_state.Current, Is.EqualTo(NpcState.Greeting));
        }

        [Test]
        public void ApplyStartPose_AfterSet_KeepsRequestedStateButStillWritesSwim()
        {
            _state.Set(NpcState.Applause);
            _parameters.ClearWrites();

            _state.ApplyStartPose(NpcState.Greeting, swim: true);

            Assert.That(_parameters.WasWritten(AnimParams.IsGreeting), Is.False);
            Assert.That(_parameters.WasWritten(AnimParams.IsTalk), Is.False);
            Assert.That(_parameters.WasWritten(AnimParams.IsApplause), Is.False);
            Assert.That(_parameters.ValueOf(AnimParams.IsSwim), Is.True);
            Assert.That(_state.Current, Is.EqualTo(NpcState.Applause));
        }

        [Test]
        public void Current_BeforeAnyWrite_IsNull()
        {
            NpcState? current = _state.Current;

            Assert.That(current, Is.Null);
        }

        [Test]
        public void Current_AfterStartPoseThenSet_IsLastRequestedState()
        {
            _state.ApplyStartPose(NpcState.Greeting, swim: false);

            _state.Set(NpcState.Talk);

            Assert.That(_state.Current, Is.EqualTo(NpcState.Talk));
        }

        [Test]
        public void Constructor_WithNullParameters_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new NpcAnimationState(null));
        }
    }
}
