using NUnit.Framework;
using UnityEngine;

using DreamVille.Gameplay.Player.Animation;

namespace DreamVille.Tests.EditMode.Player
{
    /// <summary><see cref="LocomotionMapper"/> against the exact stick-to-animator-code mapping, quirks included.</summary>
    public class LocomotionMapperTests
    {
        [Test]
        public void Map_InsideDeadZone_IsNotMoving()
        {
            var input = new Vector2(0.01f, 0.02f);

            LocomotionState state = LocomotionMapper.Map(input);

            Assert.That(state.IsMoving, Is.False);
            Assert.That(state.Direction, Is.EqualTo(MoveDirection.None));
            Assert.That(state.SpeedMultiplier, Is.EqualTo(1f));
        }

        [Test]
        public void Map_Forward_ForwardWithPositiveMultiplier()
        {
            var input = new Vector2(0f, 1f);

            LocomotionState state = LocomotionMapper.Map(input);

            Assert.That(state.IsMoving, Is.True);
            Assert.That(state.Direction, Is.EqualTo(MoveDirection.Forward));
            Assert.That(state.SpeedMultiplier, Is.EqualTo(1f));
        }

        [Test]
        public void Map_ForwardRight_StrafeRight()
        {
            var input = new Vector2(1f, 1f);

            LocomotionState state = LocomotionMapper.Map(input);

            Assert.That(state.Direction, Is.EqualTo(MoveDirection.StrafeRight));
            Assert.That(state.SpeedMultiplier, Is.EqualTo(1f));
        }

        [Test]
        public void Map_ForwardLeft_StrafeLeft()
        {
            var input = new Vector2(-1f, 1f);

            LocomotionState state = LocomotionMapper.Map(input);

            Assert.That(state.Direction, Is.EqualTo(MoveDirection.StrafeLeft));
            Assert.That(state.SpeedMultiplier, Is.EqualTo(1f));
        }

        [Test]
        public void Map_Backward_ForwardWithNegativeMultiplier()
        {
            var input = new Vector2(0f, -1f);

            LocomotionState state = LocomotionMapper.Map(input);

            Assert.That(state.Direction, Is.EqualTo(MoveDirection.Forward));
            Assert.That(state.SpeedMultiplier, Is.EqualTo(-1f));
        }

        [Test]
        public void Map_BackwardRight_StrafeLeftWithNegativeMultiplier()
        {
            var input = new Vector2(1f, -1f);

            LocomotionState state = LocomotionMapper.Map(input);

            Assert.That(state.Direction, Is.EqualTo(MoveDirection.StrafeLeft));
            Assert.That(state.SpeedMultiplier, Is.EqualTo(-1f));
        }

        [Test]
        public void Map_BackwardLeft_StrafeRightWithNegativeMultiplier()
        {
            var input = new Vector2(-1f, -1f);

            LocomotionState state = LocomotionMapper.Map(input);

            Assert.That(state.Direction, Is.EqualTo(MoveDirection.StrafeRight));
            Assert.That(state.SpeedMultiplier, Is.EqualTo(-1f));
        }

        [Test]
        public void Map_PureRight_StrafeRight()
        {
            var input = new Vector2(1f, 0f);

            LocomotionState state = LocomotionMapper.Map(input);

            Assert.That(state.Direction, Is.EqualTo(MoveDirection.StrafeRight));
            Assert.That(state.SpeedMultiplier, Is.EqualTo(1f));
        }

        [Test]
        public void Map_PureLeft_StrafeLeft()
        {
            var input = new Vector2(-1f, 0f);

            LocomotionState state = LocomotionMapper.Map(input);

            Assert.That(state.Direction, Is.EqualTo(MoveDirection.StrafeLeft));
            Assert.That(state.SpeedMultiplier, Is.EqualTo(1f));
        }
    }
}
