using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using DreamVille.Configs;
using DreamVille.Gameplay.Player;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Player
{
    /// <summary><see cref="PlayerJump"/> launch conditions and input subscription lifetime.</summary>
    public class PlayerJumpTests
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object created in _createdObjects)
                Object.DestroyImmediate(created);

            _createdObjects.Clear();
        }

        private T Track<T>(T asset) where T : Object
        {
            _createdObjects.Add(asset);
            return asset;
        }

        private PlayerJump CreateJump(
            out FakeGroundSensor ground,
            out PlayerGravity gravity,
            out FakePlayerAnimator animator,
            out FakeActorAudio audio,
            out FakePlayerInput input,
            float jumpHeight = 5f,
            float gravityValue = -20f)
        {
            var config = Track(PlayerConfig.CreateForTests(jumpHeight: jumpHeight, gravity: gravityValue));
            ground = new FakeGroundSensor();
            gravity = new PlayerGravity(config, ground);
            animator = new FakePlayerAnimator();
            audio = new FakeActorAudio();
            input = new FakePlayerInput();
            return new PlayerJump(config, input, ground, gravity, animator, audio);
        }

        [Test]
        public void TryJump_WhenGrounded_LaunchesWithSqrtOfTwoGH()
        {
            var jump = CreateJump(out FakeGroundSensor ground, out PlayerGravity gravity, out _, out _, out _);
            ground.IsGrounded = true;

            bool result = jump.TryJump();

            Assert.That(result, Is.True);
            Assert.That(gravity.VerticalVelocity.y, Is.EqualTo(14.142f).Within(1e-3f));
        }

        [Test]
        public void TryJump_WhenAirborne_ReturnsFalseAndTouchesNothing()
        {
            var jump = CreateJump(out FakeGroundSensor ground, out PlayerGravity gravity, out FakePlayerAnimator animator, out FakeActorAudio audio, out _);
            ground.IsGrounded = false;

            bool result = jump.TryJump();

            Assert.That(result, Is.False);
            Assert.That(gravity.VerticalVelocity, Is.EqualTo(Vector3.zero));
            Assert.That(animator.JumpTriggeredCount, Is.EqualTo(0));
            Assert.That(audio.JumpCount, Is.EqualTo(0));
        }

        [Test]
        public void TryJump_WhenGrounded_TriggersJumpAnimationAndSound()
        {
            var jump = CreateJump(out FakeGroundSensor ground, out _, out FakePlayerAnimator animator, out FakeActorAudio audio, out _);
            ground.IsGrounded = true;

            jump.TryJump();

            Assert.That(animator.JumpTriggeredCount, Is.EqualTo(1));
            Assert.That(audio.JumpCount, Is.EqualTo(1));
        }

        [Test]
        public void Initialize_ThenJumpInput_Jumps()
        {
            var jump = CreateJump(out FakeGroundSensor ground, out _, out FakePlayerAnimator animator, out _, out FakePlayerInput input);
            ground.IsGrounded = true;
            jump.Initialize();

            input.RaiseJumpPressed();

            Assert.That(animator.JumpTriggeredCount, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_ThenJumpInput_DoesNotJump()
        {
            var jump = CreateJump(out FakeGroundSensor ground, out _, out FakePlayerAnimator animator, out _, out FakePlayerInput input);
            ground.IsGrounded = true;
            jump.Initialize();
            jump.Dispose();

            input.RaiseJumpPressed();

            Assert.That(animator.JumpTriggeredCount, Is.EqualTo(0));
        }
    }
}
