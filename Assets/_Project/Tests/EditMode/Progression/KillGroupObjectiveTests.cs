using System;
using NUnit.Framework;

using DreamVille.Gameplay.Damage;
using DreamVille.Progression.Objectives;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Progression
{
    /// <summary>Counting rules of <see cref="KillGroupObjective"/> over <see cref="FakeMortal"/> members.</summary>
    public class KillGroupObjectiveTests
    {
        private FakeMortal _first;
        private FakeMortal _second;
        private int _completedCount;

        [SetUp]
        public void SetUp()
        {
            _first = new FakeMortal();
            _second = new FakeMortal();
            _completedCount = 0;
        }

        [Test]
        public void Constructor_WithNullList_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new KillGroupObjective(null));
        }

        [Test]
        public void Constructor_WithNoMembers_Throws()
        {
            Assert.Throws<ArgumentException>(() => new KillGroupObjective(Array.Empty<IMortal>()));
        }

        [Test]
        public void Constructor_WithANullMember_Throws()
        {
            var exception = Assert.Throws<ArgumentException>(() => new KillGroupObjective(new IMortal[] { _first, null }));

            Assert.That(exception.Message, Does.Contain("index 1"));
        }

        [Test]
        public void Start_WithLivingMembers_IsNotComplete()
        {
            KillGroupObjective objective = CreateStarted(_first, _second);

            Assert.That(objective.IsComplete, Is.False);
            Assert.That(objective.Remaining, Is.EqualTo(2));
            Assert.That(_completedCount, Is.EqualTo(0));
        }

        [Test]
        public void LastMemberDies_RaisesCompletedOnce()
        {
            KillGroupObjective objective = CreateStarted(_first, _second);

            _first.Die();
            bool completeAfterFirst = objective.IsComplete;
            _second.Die();

            Assert.That(completeAfterFirst, Is.False);
            Assert.That(objective.IsComplete, Is.True);
            Assert.That(_completedCount, Is.EqualTo(1));
        }

        [Test]
        public void Start_WhenEveryMemberIsAlreadyDead_CompletesDuringStart()
        {
            _first.IsDead = true;
            _second.IsDead = true;

            KillGroupObjective objective = CreateStarted(_first, _second);

            Assert.That(objective.IsComplete, Is.True);
            Assert.That(_completedCount, Is.EqualTo(1));
        }

        [Test]
        public void Start_WithOneMemberAlreadyDead_WaitsOnlyForTheLiving()
        {
            _first.IsDead = true;
            KillGroupObjective objective = CreateStarted(_first, _second);
            int remainingAtStart = objective.Remaining;

            _second.Die();

            Assert.That(remainingAtStart, Is.EqualTo(1));
            Assert.That(objective.IsComplete, Is.True);
            Assert.That(_completedCount, Is.EqualTo(1));
        }

        [Test]
        public void DuplicateMember_CountsOnce()
        {
            KillGroupObjective objective = CreateStarted(_first, _first, _second);
            int remainingAtStart = objective.Remaining;

            _first.Die();
            _second.Die();

            Assert.That(remainingAtStart, Is.EqualTo(2));
            Assert.That(objective.Remaining, Is.EqualTo(0));
            Assert.That(objective.IsComplete, Is.True);
            Assert.That(_completedCount, Is.EqualTo(1));
        }

        [Test]
        public void DeathRaisedTwice_AfterCompletion_DoesNotRaiseAgain()
        {
            KillGroupObjective objective = CreateStarted(_first);

            _first.Die();
            _first.Die();

            Assert.That(objective.IsComplete, Is.True);
            Assert.That(_completedCount, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_UnsubscribesFromLivingMembers()
        {
            KillGroupObjective objective = CreateStarted(_first, _second);

            objective.Dispose();
            _first.Die();
            _second.Die();

            Assert.That(_first.HandlerCount, Is.EqualTo(0));
            Assert.That(_second.HandlerCount, Is.EqualTo(0));
            Assert.That(objective.IsComplete, Is.False);
            Assert.That(_completedCount, Is.EqualTo(0));
        }

        private KillGroupObjective CreateStarted(params IMortal[] members)
        {
            var objective = new KillGroupObjective(members);
            objective.Completed += () => _completedCount++;
            objective.Start();
            return objective;
        }
    }
}
