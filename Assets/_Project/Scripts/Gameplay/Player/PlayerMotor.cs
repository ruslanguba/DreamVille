using UnityEngine;
using Zenject;

using DreamVille.Gameplay.Damage;
using DreamVille.Gameplay.Player.Ground;

namespace DreamVille.Gameplay.Player
{
    /// <summary>
    /// The one place that moves the player's body. Runs the player's systems in a fixed order every frame and sums
    /// their velocities into a single <see cref="CharacterController.Move"/>, then samples the ground for the next frame.
    /// </summary>
    public sealed class PlayerMotor : ITickable
    {
        private readonly CharacterController _controller;
        private readonly Transform _body;
        private readonly IGroundSensor _ground;
        private readonly PlayerLook _look;
        private readonly PlayerMovement _movement;
        private readonly PlayerGravity _gravity;
        private readonly PlayerKnockback _knockback;
        private readonly SafePositionTracker _safePosition;

        public PlayerMotor(
            CharacterController controller,
            Transform body,
            IGroundSensor ground,
            PlayerLook look,
            PlayerMovement movement,
            PlayerGravity gravity,
            PlayerKnockback knockback,
            SafePositionTracker safePosition)
        {
            _controller = controller;
            _body = body;
            _ground = ground;
            _look = look;
            _movement = movement;
            _gravity = gravity;
            _knockback = knockback;
            _safePosition = safePosition;
        }

        void ITickable.Tick()
        {
            float deltaTime = Time.deltaTime;

            _look.Tick(deltaTime);
            _movement.Tick(deltaTime);
            _gravity.Tick(deltaTime);

            Vector3 velocity = _movement.HorizontalVelocity + _gravity.VerticalVelocity + _knockback.Velocity;
            _controller.Move(velocity * deltaTime);

            // Knockback decays after it has moved the body, so a hit's first frame travels at full force.
            _knockback.Tick(deltaTime);

            _ground.Sample();
            _safePosition.Tick(deltaTime, _ground.IsSafeGround, _body.position);
        }
    }
}
