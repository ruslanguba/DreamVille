using UnityEngine;

using DreamVille.Configs;
using DreamVille.Gameplay.Player.Ground;

namespace DreamVille.Gameplay.Player
{
    /// <summary>Vertical velocity of the player: gravity while airborne, a small downward stick while grounded.</summary>
    public sealed class PlayerGravity
    {
        private readonly PlayerConfig _config;
        private readonly IGroundSensor _ground;
        private Vector3 _velocity;

        public PlayerGravity(PlayerConfig config, IGroundSensor ground)
        {
            _config = config;
            _ground = ground;
        }

        public Vector3 VerticalVelocity => _velocity;

        /// <summary>
        /// Snaps a falling grounded body to <see cref="PlayerConfig.GroundedStickSpeed"/> so the controller stays grounded,
        /// then integrates gravity.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (_ground.IsGrounded && _velocity.y < 0f)
                _velocity.y = _config.GroundedStickSpeed;

            _velocity.y += _config.Gravity * deltaTime;
        }

        /// <summary>Overrides the vertical speed, e.g. at the start of a jump.</summary>
        public void Launch(float upwardSpeed)
        {
            _velocity.y = upwardSpeed;
        }
    }
}
