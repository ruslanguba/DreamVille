using Unity.Cinemachine;
using UnityEngine;

using DreamVille.Configs;
using DreamVille.Input;

namespace DreamVille.Gameplay.Player
{
    /// <summary>While aiming, turns the body's yaw toward the camera's so the character faces where the player aims.</summary>
    public sealed class PlayerLook
    {
        private readonly PlayerConfig _config;
        private readonly IPlayerInput _input;
        private readonly Transform _body;
        private readonly CinemachineCamera _camera;

        public PlayerLook(PlayerConfig config, IPlayerInput input, Transform body, CinemachineCamera camera)
        {
            _config = config;
            _input = input;
            _body = body;
            _camera = camera;
        }

        public void Tick(float deltaTime)
        {
            if (!_input.IsAimHeld)
                return;

            float cameraYaw = _camera.State.RawOrientation.eulerAngles.y;
            float bodyYaw = _body.eulerAngles.y;
            float newYaw = Mathf.LerpAngle(bodyYaw, cameraYaw, _config.RotationSpeed * deltaTime);

            _body.rotation = Quaternion.Euler(0f, newYaw, 0f);
        }
    }
}
