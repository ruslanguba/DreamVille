using Unity.Cinemachine;
using UnityEngine;

using DreamVille.Configs;
using DreamVille.Input;

namespace DreamVille.Gameplay.Player
{
    public class PlayerLook
    {
        private PlayerContext _context;
        private PlayerConfig _settings;
        private Transform _bodyTransform;
        private CinemachineOrbitalFollow _orbitalFollow;
        private CinemachineCamera _cineCam;
        private IPlayerInput _inputReader;

        public PlayerLook(PlayerContext context, CinemachineCamera cineCam, CinemachineOrbitalFollow orbitalFollow)
        {
            _context = context;
            _settings = _context.Settings;
            _bodyTransform = _context.BodyTransform;
            _inputReader = _context.Input;

            _cineCam = cineCam;
            _orbitalFollow = orbitalFollow;
        }

        public void UpdateLook()
        {
            if (_inputReader.IsAimHeld)
                AimMode();
        }

        private void AimMode()
        {
            float cameraYaw = _cineCam.State.RawOrientation.eulerAngles.y;
            float currentYaw = _bodyTransform.eulerAngles.y;

            float newYaw = Mathf.LerpAngle(
                currentYaw,
                cameraYaw,
                _settings.RotationSpeed * Time.deltaTime
            );

            _bodyTransform.rotation = Quaternion.Euler(0f, newYaw, 0f);
        }
    }
}
