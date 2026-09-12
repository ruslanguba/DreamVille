using UnityEngine;

using DreamVille.Configs;
using DreamVille.Input;

namespace DreamVille.Gameplay.Player
{
    public class PlayerJump
    {
        private PlayerContext _context;
        private PlayerConfig _settings;
        private PlayerGravity _gravity;
        private CharacterController _controller;
        private IPlayerInput _inputReader;
        private AnimatorHandler _animator;

        public PlayerJump(PlayerContext context, PlayerGravity gravity)
        {
            _context = context;
            _settings = _context.Settings;
            _controller = _context.Controller;
            _gravity = gravity;
            _inputReader = _context.Input;
            _animator = _context.AnimatorHandler;
        }

        public void Jump()
        {
            if (_controller.isGrounded)
            {
                float jumpVelocity = Mathf.Sqrt(_settings.JumpHeight * -2f * _settings.Gravity);
                _gravity.SetYVelocity(jumpVelocity);
                _animator.SetJump();
                _context.Audio.PlayJump();
            }
        }
    }
}
