using UnityEngine;

namespace DreamVille.Gameplay.Enemies.Steering
{
    /// <summary>Pure math, unit-testable without a Rigidbody.</summary>
    public static class SteeringMath
    {
        /// <summary>Acceleration that would move <paramref name="velocity"/> towards <paramref name="targetSpeed"/> along <paramref name="direction"/>.</summary>
        public static Vector3 Acceleration(Vector3 velocity, Vector3 direction, float targetSpeed, float acceleration)
            => (direction.normalized * targetSpeed - velocity) * acceleration;
    }
}
