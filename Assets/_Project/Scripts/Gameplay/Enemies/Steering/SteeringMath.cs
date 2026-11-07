using UnityEngine;

namespace DreamVille.Gameplay.Enemies.Steering
{
    /// <summary>Pure math, unit-testable without a Rigidbody.</summary>
    public static class SteeringMath
    {
        /// <summary>
        /// Horizontal acceleration that would move <paramref name="velocity"/> towards <paramref name="targetSpeed"/>
        /// along <paramref name="direction"/>. Both are flattened and the result has no vertical component: enemies
        /// walk, so gravity alone owns their vertical motion (no floating up to a target above, no capped falls).
        /// </summary>
        public static Vector3 Acceleration(Vector3 velocity, Vector3 direction, float targetSpeed, float acceleration)
            => (Flatten(direction).normalized * targetSpeed - Flatten(velocity)) * acceleration;

        private static Vector3 Flatten(Vector3 vector)
        {
            vector.y = 0f;
            return vector;
        }
    }
}
