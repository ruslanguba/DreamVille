using UnityEngine;

namespace DreamVille.Gameplay.Enemies
{
    /// <summary>
    /// Turns an enemy towards its own velocity while enabled, and towards a world point on demand.
    /// Disabling the component suspends the automatic facing without blocking the explicit calls.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class EnemyRotationHandler : MonoBehaviour
    {
        private const float MinSqrSpeed = 0.01f;
        private const float MinSqrDirection = 0.0001f;

        private Rigidbody _body;

        // Set by Enemy.Awake from EnemyConfig.RotationSpeed; stays 0 (no turning) on an enemy that refused to start.
        private float _turnSpeed;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            // Horizontal motion only: a body falling straight down would otherwise turn after the noise in its drift.
            Vector3 horizontalVelocity = Flatten(_body.linearVelocity);
            if (horizontalVelocity.sqrMagnitude < MinSqrSpeed)
                return;

            RotateTowards(horizontalVelocity);
        }

        /// <summary>Sets the facing slerp rate. Called once by <see cref="Enemy"/> in Awake.</summary>
        public void Configure(float turnSpeed) => _turnSpeed = turnSpeed;

        /// <summary>Slerps facing towards a world point at the configured rotation speed.</summary>
        public void FaceTowards(Vector3 worldPoint) => RotateTowards(Flatten(worldPoint - transform.position));

        /// <summary>Snaps facing towards a world point in a single frame.</summary>
        public void SnapTo(Vector3 worldPoint)
        {
            Vector3 direction = Flatten(worldPoint - transform.position);
            if (direction.sqrMagnitude < MinSqrDirection)
                return;

            transform.rotation = Quaternion.LookRotation(direction);
        }

        private void RotateTowards(Vector3 direction)
        {
            if (direction.sqrMagnitude < MinSqrDirection)
                return;

            Quaternion target = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, _turnSpeed * Time.deltaTime);
        }

        private static Vector3 Flatten(Vector3 direction)
        {
            direction.y = 0f;
            return direction;
        }
    }
}
