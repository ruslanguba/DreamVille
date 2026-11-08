using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;

using DreamVille.Configs;
using DreamVille.Gameplay.Weapons.Pillows;
using DreamVille.Tests.EditMode.Fakes;
using DreamVille.Tests.Support;

namespace DreamVille.Tests.EditMode.Weapons
{
    /// <summary>The F/R rules of <see cref="PillowRack"/> over two <see cref="FakePillowBody"/> slots.</summary>
    public class PillowRackTests
    {
        private static readonly Pose Launch = new Pose(Vector3.zero, Quaternion.identity);
        private static readonly Vector3 ReturnPoint = Vector3.zero;

        private FakePillowBody _first;
        private FakePillowBody _second;
        private PillowConfig _config;
        private PillowRack _rack;

        [SetUp]
        public void SetUp()
        {
            _first = new FakePillowBody();
            _second = new FakePillowBody();
            _config = PillowConfig.CreateForTests(throwImpulse: 10f, recallSpeed: 12f, recallSnapDistance: 10f, arrivalThreshold: 0.1f);
            _rack = new PillowRack(new[] { _first, _second }, _config);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void Constructor_StowsEveryBodyWithNothingAvailable()
        {
            Assert.That(_first.StowCount, Is.EqualTo(1));
            Assert.That(_second.StowCount, Is.EqualTo(1));
            Assert.That(_rack.Capacity, Is.EqualTo(2));
            Assert.That(_rack.Available, Is.EqualTo(0));
            Assert.That(_rack.InHand, Is.EqualTo(0));
        }

        [Test]
        public void ThrowOrRecall_NoAvailable_ReturnsNone()
        {
            _first.ResetCalls();

            PillowCommand command = _rack.ThrowOrRecall(Launch, ReturnPoint);

            Assert.That(command, Is.EqualTo(PillowCommand.None));
            Assert.That(_first.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void ThrowOrRecall_FirstPress_ThrowsFirstSlot()
        {
            _rack.SetAvailable(2);

            PillowCommand command = _rack.ThrowOrRecall(Launch, ReturnPoint);

            Assert.That(command, Is.EqualTo(PillowCommand.Thrown));
            Assert.That(_first.LastCall, Is.EqualTo(nameof(IPillowBody.Launch)));
            Assert.That(_first.LaunchImpulse, Is.EqualTo(Vector3.forward * 10f).Using(Vector3EqualityComparer.Instance));
            Assert.That(_second.LastCall, Is.EqualTo(nameof(IPillowBody.Stow)));
            Assert.That(_rack.InHand, Is.EqualTo(1));
        }

        [Test]
        public void ThrowOrRecall_WhileFlying_StartsReturnOfFlyingPillow()
        {
            _rack.SetAvailable(2);
            _rack.ThrowOrRecall(Launch, ReturnPoint);

            PillowCommand command = _rack.ThrowOrRecall(Launch, ReturnPoint);

            Assert.That(command, Is.EqualTo(PillowCommand.ReturnStarted));
            Assert.That(_first.LastCall, Is.EqualTo(nameof(IPillowBody.BeginReturn)));
            Assert.That(_second.LastCall, Is.EqualTo(nameof(IPillowBody.Stow)));
            Assert.That(_rack.IsReturning, Is.True);
        }

        [Test]
        public void FreezeFlying_ThenPress_ThrowsNextSlot()
        {
            _rack.SetAvailable(2);
            _rack.ThrowOrRecall(Launch, ReturnPoint);

            bool frozen = _rack.FreezeFlying();
            PillowCommand command = _rack.ThrowOrRecall(Launch, ReturnPoint);

            Assert.That(frozen, Is.True);
            Assert.That(command, Is.EqualTo(PillowCommand.Thrown));
            Assert.That(_first.LastCall, Is.EqualTo(nameof(IPillowBody.Freeze)));
            Assert.That(_second.LastCall, Is.EqualTo(nameof(IPillowBody.Launch)));
        }

        [Test]
        public void ThrowOrRecall_AllAvailablePlaced_RecallsMostRecent()
        {
            _rack.SetAvailable(2);
            _rack.ThrowOrRecall(Launch, ReturnPoint);
            _rack.FreezeFlying();
            _rack.ThrowOrRecall(Launch, ReturnPoint);
            _rack.FreezeFlying();

            PillowCommand command = _rack.ThrowOrRecall(Launch, ReturnPoint);

            Assert.That(command, Is.EqualTo(PillowCommand.ReturnStarted));
            Assert.That(_second.LastCall, Is.EqualTo(nameof(IPillowBody.BeginReturn)));
            Assert.That(_first.LastCall, Is.EqualTo(nameof(IPillowBody.Freeze)));
        }

        [Test]
        public void ThrowOrRecall_WhileReturning_ReturnsNone()
        {
            _rack.SetAvailable(2);
            _rack.ThrowOrRecall(Launch, ReturnPoint);
            _rack.ThrowOrRecall(Launch, ReturnPoint);
            _second.ResetCalls();

            PillowCommand command = _rack.ThrowOrRecall(Launch, ReturnPoint);

            Assert.That(command, Is.EqualTo(PillowCommand.None));
            Assert.That(_second.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void ThrowOrRecall_OnlyUsesFirstAvailableSlots()
        {
            _rack.SetAvailable(1);
            _rack.ThrowOrRecall(Launch, ReturnPoint);
            _rack.FreezeFlying();
            _second.ResetCalls();

            PillowCommand command = _rack.ThrowOrRecall(Launch, ReturnPoint);

            Assert.That(command, Is.EqualTo(PillowCommand.ReturnStarted));
            Assert.That(_first.LastCall, Is.EqualTo(nameof(IPillowBody.BeginReturn)));
            Assert.That(_second.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void SetAvailable_ClampsToCapacityAndNeverShrinks()
        {
            _rack.SetAvailable(5);
            int afterGrow = _rack.Available;

            _rack.SetAvailable(1);

            Assert.That(afterGrow, Is.EqualTo(2));
            Assert.That(_rack.Available, Is.EqualTo(2));
        }

        [Test]
        public void CompleteReturn_WhenReturning_RestoresInHand()
        {
            _rack.SetAvailable(2);
            _rack.ThrowOrRecall(Launch, ReturnPoint);
            _first.Position = new Vector3(0f, 0f, 5f);
            _rack.ThrowOrRecall(Launch, ReturnPoint);

            bool completed = _rack.CompleteReturn();

            Assert.That(completed, Is.True);
            Assert.That(_rack.IsReturning, Is.False);
            Assert.That(_rack.InHand, Is.EqualTo(2));
            Assert.That(_first.LastCall, Is.EqualTo(nameof(IPillowBody.Stow)));
        }

        [Test]
        public void StepReturn_UntilArrival_ClearsReturningAndRestoresInHand()
        {
            _rack.SetAvailable(2);
            _rack.ThrowOrRecall(Launch, ReturnPoint);
            _first.Position = new Vector3(0f, 0f, 0.05f);
            _rack.ThrowOrRecall(Launch, ReturnPoint);

            bool arrived = _rack.StepReturn(ReturnPoint, deltaTime: 1f);

            Assert.That(arrived, Is.True);
            Assert.That(_rack.IsReturning, Is.False);
            Assert.That(_rack.InHand, Is.EqualTo(2));
        }

        [Test]
        public void StepReturn_NothingReturning_ReturnsTrue()
        {
            bool result = _rack.StepReturn(ReturnPoint, deltaTime: 1f);

            Assert.That(result, Is.True);
        }

        [Test]
        public void StepReturn_PartialStep_MovesByRecallSpeedTimesDeltaTime()
        {
            _rack.SetAvailable(2);
            _rack.ThrowOrRecall(Launch, ReturnPoint);
            _first.Position = new Vector3(0f, 0f, 5f);
            _rack.ThrowOrRecall(Launch, ReturnPoint);

            bool arrived = _rack.StepReturn(ReturnPoint, deltaTime: 0.1f);

            Assert.That(arrived, Is.False);
            Assert.That(_first.LastCall, Is.EqualTo(nameof(IPillowBody.MoveTo)));
            Assert.That(_first.Position, Is.EqualTo(new Vector3(0f, 0f, 3.8f)).Using(Vector3EqualityComparer.Instance));
        }

        [Test]
        public void Constructor_DestroyedUnityBody_Throws()
        {
            var go = new GameObject("DestroyedPillowBody");
            var body = go.AddComponent<FakePillowBodyBehaviour>();
            Assert.That(body, Is.Not.Null, "component must be added before it can be destroyed");
            Object.DestroyImmediate(body);

            Assert.Throws<System.ArgumentException>(() => new PillowRack(new IPillowBody[] { body }, _config));

            Object.DestroyImmediate(go);
        }
    }
}
