using NUnit.Framework;
using UnityEngine;

using DreamVille.Gameplay.Weapons.Pillow;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Weapons
{
    public class PillowSlotTests
    {
        private static readonly Pose Launch = new Pose(new Vector3(0f, 1f, 2f), Quaternion.Euler(0f, 90f, 0f));
        private static readonly Vector3 Impulse = new Vector3(10f, 0f, 0f);
        private static readonly Vector3 ReturnPoint = Vector3.zero;

        private FakePillowBody _body;
        private PillowSlot _slot;

        [SetUp]
        public void SetUp()
        {
            _body = new FakePillowBody();
            _slot = new PillowSlot(_body);
        }

        [Test]
        public void NewSlot_IsStowedAndStowsBody()
        {
            Assert.That(_slot.State, Is.EqualTo(PillowState.Stowed));
            Assert.That(_body.StowCount, Is.EqualTo(1));
        }

        [Test]
        public void TryThrow_FromStowed_LaunchesWithImpulseAndFlies()
        {
            bool thrown = _slot.TryThrow(Launch, Impulse);

            Assert.That(thrown, Is.True);
            Assert.That(_slot.State, Is.EqualTo(PillowState.Flying));
            Assert.That(_body.LastCall, Is.EqualTo(nameof(IPillowBody.Launch)));
            Assert.That(_body.LaunchPosition, Is.EqualTo(Launch.position));
            Assert.That(_body.LaunchRotation, Is.EqualTo(Launch.rotation));
            Assert.That(_body.LaunchImpulse, Is.EqualTo(Impulse));
        }

        [Test]
        public void TryThrow_WhenNotStowed_ReturnsFalseAndTouchesNothing()
        {
            _slot.TryThrow(Launch, Impulse);
            _body.ResetCalls();

            bool thrown = _slot.TryThrow(Launch, Impulse);

            Assert.That(thrown, Is.False);
            Assert.That(_slot.State, Is.EqualTo(PillowState.Flying));
            Assert.That(_body.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void TryFreeze_FromFlying_Places()
        {
            _slot.TryThrow(Launch, Impulse);

            bool frozen = _slot.TryFreeze();

            Assert.That(frozen, Is.True);
            Assert.That(_slot.State, Is.EqualTo(PillowState.Placed));
            Assert.That(_body.LastCall, Is.EqualTo(nameof(IPillowBody.Freeze)));
        }

        [Test]
        public void TryBeginReturn_BeyondSnapDistance_StartsOnSnapRadius()
        {
            _slot.TryThrow(Launch, Impulse);
            _body.Position = new Vector3(0f, 0f, 50f);

            bool started = _slot.TryBeginReturn(ReturnPoint, snapDistance: 10f);

            Assert.That(started, Is.True);
            Assert.That(_slot.State, Is.EqualTo(PillowState.Returning));
            Assert.That(Vector3.Distance(_body.ReturnStart, new Vector3(0f, 0f, 10f)), Is.LessThan(1e-4f));
        }

        [Test]
        public void StepReturn_WithinThreshold_StowsAndReturnsTrue()
        {
            _slot.TryThrow(Launch, Impulse);
            _body.Position = new Vector3(0f, 0f, 1f);
            _slot.TryBeginReturn(ReturnPoint, snapDistance: 10f);

            bool arrived = _slot.StepReturn(ReturnPoint, maxDistanceDelta: 0.95f, arrivalThreshold: 0.1f);

            Assert.That(arrived, Is.True);
            Assert.That(_slot.State, Is.EqualTo(PillowState.Stowed));
            Assert.That(_body.Position, Is.EqualTo(ReturnPoint));
            Assert.That(_body.LastCall, Is.EqualTo(nameof(IPillowBody.Stow)));
        }

        [Test]
        public void StepReturn_PartialStep_MovesAndReturnsFalse()
        {
            _slot.TryThrow(Launch, Impulse);
            _body.Position = new Vector3(0f, 0f, 5f);
            _slot.TryBeginReturn(ReturnPoint, snapDistance: 10f);

            bool arrived = _slot.StepReturn(ReturnPoint, maxDistanceDelta: 1.2f, arrivalThreshold: 0.1f);

            Assert.That(arrived, Is.False);
            Assert.That(_slot.State, Is.EqualTo(PillowState.Returning));
            Assert.That(_body.LastCall, Is.EqualTo(nameof(IPillowBody.MoveTo)));
            Assert.That(_body.Position, Is.EqualTo(new Vector3(0f, 0f, 3.8f)));
        }

        [Test]
        public void TryBeginReturn_WithinSnapRadius_KeepsStartPosition()
        {
            _slot.TryThrow(Launch, Impulse);
            _body.Position = new Vector3(0f, 0f, 5f);

            bool started = _slot.TryBeginReturn(ReturnPoint, snapDistance: 10f);

            Assert.That(started, Is.True);
            Assert.That(_body.ReturnStart, Is.EqualTo(new Vector3(0f, 0f, 5f)));
        }

        [Test]
        public void TryBeginReturn_FromPlaced_StartsReturning()
        {
            _slot.TryThrow(Launch, Impulse);
            _slot.TryFreeze();

            bool started = _slot.TryBeginReturn(ReturnPoint, snapDistance: 10f);

            Assert.That(started, Is.True);
            Assert.That(_slot.State, Is.EqualTo(PillowState.Returning));
        }

        [Test]
        public void CompleteReturn_FromReturning_StowsImmediately()
        {
            _slot.TryThrow(Launch, Impulse);
            _body.Position = new Vector3(0f, 0f, 5f);
            _slot.TryBeginReturn(ReturnPoint, snapDistance: 10f);

            bool completed = _slot.CompleteReturn();

            Assert.That(completed, Is.True);
            Assert.That(_slot.State, Is.EqualTo(PillowState.Stowed));
            Assert.That(_body.LastCall, Is.EqualTo(nameof(IPillowBody.Stow)));
        }
    }
}
