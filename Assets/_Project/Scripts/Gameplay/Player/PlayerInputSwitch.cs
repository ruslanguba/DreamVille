using Unity.Cinemachine;
using UnityEngine;

using DreamVille.Core.Input;

namespace DreamVille.Gameplay.Player
{
    /// <summary>
    /// Включает и выключает управление игроком: чтение ввода и вращение камеры.
    /// Единственная обязанность — раньше делил класс с провайдером трансформа.
    /// </summary>
    public class PlayerInputSwitch : MonoBehaviour
    {
        private InputReader _input;
        private CinemachineInputAxisController _axisController;

        private void Awake()
        {
            _input = GetComponent<InputReader>();
            _axisController = GetComponentInChildren<CinemachineInputAxisController>(true);
        }

        public void EnableInput() => SetEnabled(true);

        public void DisableInput() => SetEnabled(false);

        private void SetEnabled(bool value)
        {
            if (_input != null)
                _input.enabled = value;

            if (_axisController != null)
                _axisController.enabled = value;
        }
    }
}
