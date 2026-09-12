using NUnit.Framework;
using UnityEngine;

using DreamVille.Gameplay.Enemies.Steering;

namespace DreamVille.Tests.EditMode.Enemies
{
    /// <summary>Pure velocity-matching math behind every steering state, exercised without a Rigidbody.</summary>
    public class SteeringMathTests
    {
        [Test]
        public void Acceleration_AtTargetVelocity_IsZero()
        {
            var direction = new Vector3(1f, 0f, 0f);
            var velocity = direction * 5f;

            Vector3 acceleration = SteeringMath.Acceleration(velocity, direction, targetSpeed: 5f, acceleration: 10f);

            Assert.That(acceleration, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void Acceleration_FromRest_PointsAlongDirection()
        {
            var direction = new Vector3(0f, 0f, 1f);

            Vector3 acceleration = SteeringMath.Acceleration(Vector3.zero, direction, targetSpeed: 4f, acceleration: 2f);

            Assert.That(acceleration, Is.EqualTo(new Vector3(0f, 0f, 8f)));
        }
    }
}
