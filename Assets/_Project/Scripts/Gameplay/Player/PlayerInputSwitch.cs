using Unity.Cinemachine;
using UnityEngine;

using DreamVille.Core.Players;
using DreamVille.Input;

namespace DreamVille.Gameplay.Player
{
    /// <summary>Turns player input reading and camera rotation on and off.</summary>
    public class PlayerInputSwitch : MonoBehaviour, IPlayerControlSwitch
    {
        private InputReader _input;
        private CinemachineInputAxisController _axisController;

        private void Awake()
        {
            _input = GetComponent<InputReader>();
            _axisController = GetComponentInChildren<CinemachineInputAxisController>(true);
        }

        public void EnableControl() => SetEnabled(true);

        public void DisableControl() => SetEnabled(false);

        private void SetEnabled(bool value)
        {
            if (_input != null)
                _input.enabled = value;

            if (_axisController != null)
                _axisController.enabled = value;
        }
    }
}
