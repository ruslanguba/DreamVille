using UnityEngine;

namespace DreamVille.Gameplay.Enemies.Steering
{
    /// <summary>Steers a <see cref="Rigidbody"/> with mass-independent acceleration forces.</summary>
    public sealed class RigidbodySteering : ISteering
    {
        private readonly Rigidbody _body;

        public RigidbodySteering(Rigidbody body)
        {
            _body = body;
        }

        public Vector3 Velocity => _body.linearVelocity;

        public void Accelerate(Vector3 direction, float targetSpeed, float acceleration)
            => _body.AddForce(SteeringMath.Acceleration(_body.linearVelocity, direction, targetSpeed, acceleration), ForceMode.Acceleration);

        public void Stop() => _body.linearVelocity = Vector3.zero;
    }
}
