using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using DreamVille.Configs;
using DreamVille.Gameplay.Damage;

namespace DreamVille.Tests.EditMode.Player
{
    /// <summary><see cref="PlayerKnockback"/> impulse application and decay.</summary>
    public class PlayerKnockbackTests
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
        public void Apply_FlattensVerticalComponent()
        {
            var config = Track(PlayerConfig.CreateForTests());
            var knockback = new PlayerKnockback(config);

            knockback.Apply(new Vector3(1f, 5f, 1f), 3f);

            Assert.That(knockback.Velocity.y, Is.EqualTo(0f));
        }

        [Test]
        public void Apply_SetsSpeedToForce()
        {
            var config = Track(PlayerConfig.CreateForTests());
            var knockback = new PlayerKnockback(config);

            knockback.Apply(Vector3.forward, 7f);

            Assert.That(knockback.Velocity.magnitude, Is.EqualTo(7f).Within(1e-4f));
        }

        [Test]
        public void Tick_DecaysTowardsZero()
        {
            var config = Track(PlayerConfig.CreateForTests(knockbackDecay: 1f, knockbackStopSpeed: 0.01f));
            var knockback = new PlayerKnockback(config);
            knockback.Apply(Vector3.right, 5f);

            knockback.Tick(0.1f);
            float afterFirstTick = knockback.Velocity.magnitude;
            knockback.Tick(0.1f);
            float afterSecondTick = knockback.Velocity.magnitude;

            Assert.That(afterFirstTick, Is.LessThan(5f));
            Assert.That(afterSecondTick, Is.LessThan(afterFirstTick));
        }

        [Test]
        public void Tick_BelowStopSpeed_StopsCompletely()
        {
            var config = Track(PlayerConfig.CreateForTests(knockbackStopSpeed: 0.1f));
            var knockback = new PlayerKnockback(config);
            knockback.Apply(Vector3.right, 0.05f);

            knockback.Tick(0.1f);

            Assert.That(knockback.Velocity, Is.EqualTo(Vector3.zero));
        }
    }
}
