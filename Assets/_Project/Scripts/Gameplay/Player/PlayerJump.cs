using System;

using UnityEngine;
using Zenject;

using DreamVille.Audio;
using DreamVille.Configs;
using DreamVille.Gameplay.Player.Animation;
using DreamVille.Gameplay.Player.Ground;
using DreamVille.Input;

namespace DreamVille.Gameplay.Player
{
    /// <summary>Turns the jump input into a vertical launch when the player is grounded.</summary>
    public sealed class PlayerJump : IInitializable, IDisposable
    {
        private readonly PlayerConfig _config;
        private readonly IPlayerInput _input;
        private readonly IGroundSensor _ground;
        private readonly PlayerGravity _gravity;
        private readonly IPlayerAnimator _animator;
        private readonly IActorAudio _audio;

        public PlayerJump(
            PlayerConfig config,
            IPlayerInput input,
            IGroundSensor ground,
            PlayerGravity gravity,
            IPlayerAnimator animator,
            IActorAudio audio)
        {
            _config = config;
            _input = input;
            _ground = ground;
            _gravity = gravity;
            _animator = animator;
            _audio = audio;
        }

        public void Initialize()
        {
            _input.Jump += OnJump;
        }

        public void Dispose()
        {
            _input.Jump -= OnJump;
        }

        /// <summary>Launches the player if grounded; returns false and changes nothing while airborne.</summary>
        public bool TryJump()
        {
            if (!_ground.IsGrounded)
                return false;

            _gravity.Launch(JumpSpeed(_config.JumpHeight, _config.Gravity));
            _animator.TriggerJump();
            _audio.PlayJump();
            return true;
        }

        /// <summary>Initial upward speed that reaches <paramref name="jumpHeight"/> under <paramref name="gravity"/> (negative).</summary>
        public static float JumpSpeed(float jumpHeight, float gravity) => Mathf.Sqrt(jumpHeight * -2f * gravity);

        private void OnJump()
        {
            TryJump();
        }
    }
}
