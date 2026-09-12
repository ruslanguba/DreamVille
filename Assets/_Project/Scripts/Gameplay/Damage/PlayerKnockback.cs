using UnityEngine;

using DreamVille.Configs;

namespace DreamVille.Gameplay.Damage
{
    /// <summary>Impulse-driven horizontal displacement applied on top of normal player movement.</summary>
    public sealed class PlayerKnockback
    {
        private readonly PlayerConfig _config;
        private Vector3 _velocity;

        public PlayerKnockback(PlayerConfig config)
        {
            _config = config;
        }

        public Vector3 Velocity => _velocity;

        /// <summary>Flattens the vertical component and sets the velocity to <paramref name="direction"/> scaled by <paramref name="force"/>.</summary>
        public void Apply(Vector3 direction, float force)
        {
            direction.y = 0f;
            _velocity = direction.normalized * force;
        }

        /// <summary>Decays the velocity toward zero; snaps to zero below <see cref="PlayerConfig.KnockbackStopSpeed"/>.</summary>
        public void Tick(float deltaTime)
        {
            if (_velocity.magnitude <= _config.KnockbackStopSpeed)
            {
                _velocity = Vector3.zero;
                return;
            }

            _velocity = Vector3.Lerp(_velocity, Vector3.zero, deltaTime * _config.KnockbackDecay);
        }
    }
}
