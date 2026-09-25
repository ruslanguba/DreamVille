using UnityEngine;

using DreamVille.Audio;
using DreamVille.Configs;
using DreamVille.Gameplay.Player.Animation;
using DreamVille.Gameplay.Player.Ground;
using DreamVille.Input;

namespace DreamVille.Gameplay.Player
{
    /// <summary>
    /// Camera-relative walking: accelerates toward the walk speed, turns the body away from the camera while moving,
    /// and feeds locomotion to the animator and footsteps to the audio.
    /// </summary>
    public sealed class PlayerMovement
    {
        private const float MoveInputThreshold = 0.0001f;
        private const float MinFacingSqrMagnitude = 0.01f;
        private const float MinTurningSpeed = 0.1f;

        private readonly PlayerConfig _config;
        private readonly IPlayerInput _input;
        private readonly Transform _body;
        private readonly Transform _camera;
        private readonly IGroundSensor _ground;
        private readonly IPlayerAnimator _animator;
        private readonly IActorAudio _audio;

        private Vector3 _horizontalVelocity;
        private float _currentSpeed;

        public PlayerMovement(
            PlayerConfig config,
            IPlayerInput input,
            Transform body,
            Camera camera,
            IGroundSensor ground,
            IPlayerAnimator animator,
            IActorAudio audio)
        {
            _config = config;
            _input = input;
            _body = body;
            _camera = camera.transform;
            _ground = ground;
            _animator = animator;
            _audio = audio;
        }

        public Vector3 HorizontalVelocity => _horizontalVelocity;

        public void Tick(float deltaTime)
        {
            Vector2 moveInput = _input.Move;

            float targetSpeed = moveInput.sqrMagnitude > MoveInputThreshold ? _config.WalkSpeed : 0f;
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, _config.Acceleration * deltaTime);

            // Forward follows the camera, right follows the body: strafing stays relative to where the character faces.
            Vector3 forward = Flatten(_camera.forward);
            Vector3 right = Flatten(_body.right);

            Vector3 facing = Flatten(_body.position - _camera.position);
            if (facing.sqrMagnitude > MinFacingSqrMagnitude && _currentSpeed > MinTurningSpeed)
                TurnToward(facing, deltaTime);

            _animator.SetLocomotion(_currentSpeed, LocomotionMapper.Map(moveInput));
            _audio.TickFootsteps(_currentSpeed > 0f && _ground.IsGrounded);

            _horizontalVelocity = (forward * moveInput.y + right * moveInput.x).normalized * _currentSpeed;
        }

        private void TurnToward(Vector3 direction, float deltaTime)
        {
            Quaternion target = Quaternion.LookRotation(direction);
            _body.rotation = Quaternion.Slerp(_body.rotation, target, _config.RotationSpeed * deltaTime);
        }

        private static Vector3 Flatten(Vector3 vector)
        {
            vector.y = 0f;
            return vector.normalized;
        }
    }
}
