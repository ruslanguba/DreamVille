using NUnit.Framework;
using UnityEngine;

using DreamVille.Configs;
using DreamVille.Gameplay.Player;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Player
{
    /// <summary>
    /// <see cref="PlayerMovement"/>: the body accelerates when walking starts and stops at once on release, and the
    /// animation, footsteps and turning stop with it. The camera stands behind the body, so "forward" is +Z.
    /// </summary>
    public class PlayerMovementTests
    {
        private const float Step = 0.02f;

        private PlayerConfig _config;
        private GameObject _body;
        private GameObject _camera;
        private FakePlayerInput _input;
        private FakeGroundSensor _ground;
        private FakePlayerAnimator _animator;
        private FakeActorAudio _audio;
        private PlayerMovement _movement;

        [SetUp]
        public void SetUp()
        {
            _config = PlayerConfig.CreateForTests(walkSpeed: 6f, acceleration: 20f, rotationSpeed: 12f);
            _body = new GameObject("Body");
            _camera = new GameObject("Camera", typeof(Camera));
            _camera.transform.position = new Vector3(0f, 2f, -5f);
            _input = new FakePlayerInput();
            _ground = new FakeGroundSensor { IsGrounded = true };
            _animator = new FakePlayerAnimator();
            _audio = new FakeActorAudio();
            _movement = new PlayerMovement(_config, _input, _body.transform, _camera.GetComponent<Camera>(), _ground, _animator, _audio);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_camera);
            Object.DestroyImmediate(_body);
            Object.DestroyImmediate(_config);
        }

        private void WalkForOneSecond()
        {
            _input.Move = new Vector2(0f, 1f);
            for (int i = 0; i < 50; i++)
                _movement.Tick(Step);
        }

        [Test]
        public void Tick_AfterRelease_ReportsZeroSpeed()
        {
            WalkForOneSecond();
            _input.Move = Vector2.zero;

            _movement.Tick(Step);

            Assert.That(_animator.LastSpeed, Is.EqualTo(0f));
            Assert.That(_movement.HorizontalVelocity, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void Tick_AfterRelease_StopsFootsteps()
        {
            WalkForOneSecond();
            _input.Move = Vector2.zero;

            _movement.Tick(Step);

            Assert.That(_audio.LastFootstepsMoving, Is.False);
        }

        [Test]
        public void Tick_WhileHeld_AcceleratesTowardsWalkSpeed()
        {
            _input.Move = new Vector2(0f, 1f);

            _movement.Tick(0.1f);

            // 20 m/s² for 0.1 s from rest: 2 m/s, still short of the 6 m/s walk speed.
            Assert.That(_animator.LastSpeed, Is.EqualTo(2f).Within(1e-4f));
            Assert.That(_movement.HorizontalVelocity.magnitude, Is.EqualTo(2f).Within(1e-4f));
        }

        [Test]
        public void Tick_Standing_DoesNotTurn()
        {
            WalkForOneSecond();
            _input.Move = Vector2.zero;
            _body.transform.rotation = Quaternion.Euler(0f, 60f, 0f);

            _movement.Tick(Step);

            Assert.That(Quaternion.Angle(_body.transform.rotation, Quaternion.Euler(0f, 60f, 0f)), Is.LessThan(0.01f));
        }
    }
}
