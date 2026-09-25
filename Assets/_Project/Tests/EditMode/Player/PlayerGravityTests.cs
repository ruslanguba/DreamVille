using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using DreamVille.Configs;
using DreamVille.Gameplay.Player;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Player
{
    /// <summary><see cref="PlayerGravity"/> vertical velocity integration.</summary>
    public class PlayerGravityTests
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

        [Test]
        public void Tick_WhenAirborne_AddsGravityScaledByDeltaTime()
        {
            var config = Track(PlayerConfig.CreateForTests(gravity: -20f));
            var ground = new FakeGroundSensor { IsGrounded = false };
            var gravity = new PlayerGravity(config, ground);

            gravity.Tick(0.5f);

            Assert.That(gravity.VerticalVelocity.y, Is.EqualTo(-10f).Within(1e-4f));
        }

        [Test]
        public void Tick_WhenGroundedAndFalling_ResetsToGroundedStickSpeed()
        {
            var config = Track(PlayerConfig.CreateForTests(gravity: -20f, groundedStickSpeed: -2f));
            var ground = new FakeGroundSensor { IsGrounded = true };
            var gravity = new PlayerGravity(config, ground);
            gravity.Launch(-5f);

            gravity.Tick(0.1f);

            Assert.That(gravity.VerticalVelocity.y, Is.EqualTo(-2f + -20f * 0.1f).Within(1e-4f));
        }

        [Test]
        public void Tick_WhenGroundedAndRising_KeepsRisingSpeed()
        {
            var config = Track(PlayerConfig.CreateForTests(gravity: -20f));
            var ground = new FakeGroundSensor { IsGrounded = true };
            var gravity = new PlayerGravity(config, ground);
            gravity.Launch(5f);

            gravity.Tick(0.1f);

            Assert.That(gravity.VerticalVelocity.y, Is.EqualTo(5f + -20f * 0.1f).Within(1e-4f));
        }

        [Test]
        public void Launch_OverridesVerticalSpeed()
        {
            var config = Track(PlayerConfig.CreateForTests());
            var ground = new FakeGroundSensor();
            var gravity = new PlayerGravity(config, ground);

            gravity.Launch(14.142f);

            Assert.That(gravity.VerticalVelocity.y, Is.EqualTo(14.142f).Within(1e-3f));
        }
    }
}
