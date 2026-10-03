using Unity.Cinemachine;
using UnityEngine;
using Zenject;

using DreamVille.Core.Players;
using DreamVille.Input;

namespace DreamVille.Gameplay.Player
{
    /// <summary>Turns player input reading and camera rotation on and off.</summary>
    public sealed class PlayerInputSwitch : MonoBehaviour, IPlayerControlSwitch
    {
        private InputReader _input;
        private CinemachineInputAxisController _axisController;

        [Inject]
        private void Construct(InputReader input, CinemachineInputAxisController axisController)
        {
            _input = input;
            _axisController = axisController;
        }

        public void EnableControl() => SetEnabled(true);

        public void DisableControl() => SetEnabled(false);

        private void SetEnabled(bool value)
        {
            // Pause and dialogue rules may still call in while the scene is being unloaded.
            if (_input == null || _axisController == null)
                return;

            _input.enabled = value;
            _axisController.enabled = value;
        }
    }
}
