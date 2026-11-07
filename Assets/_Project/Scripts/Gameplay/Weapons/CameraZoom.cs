using Unity.Cinemachine;
using UnityEngine;
using Zenject;

using DreamVille.Input;

namespace DreamVille.Gameplay.Weapons
{
    /// <summary>Zooms the third-person camera with the mouse wheel by changing the orbital follow radius within [min, max].</summary>
    public sealed class CameraZoom : MonoBehaviour
    {
        [SerializeField] private float _minZoom;
        [SerializeField] private float _maxZoom;
        [SerializeField] private float _zoomSensitivity;
        [SerializeField] private CinemachineOrbitalFollow _orbitalFollow;

        private IPlayerInput _input;

        [Inject]
        private void Construct(IPlayerInput input)
        {
            _input = input;
        }

        private void OnEnable()
        {
            _input.Scroll += Zoom;
        }

        private void OnDisable()
        {
            _input.Scroll -= Zoom;
        }

        /// <summary>Positive scroll moves the camera closer; each step continues from the current orbit radius, clamped to [min, max].</summary>
        public void Zoom(float zoom)
            => _orbitalFollow.Radius = Mathf.Clamp(_orbitalFollow.Radius - zoom * _zoomSensitivity, _minZoom, _maxZoom);
    }
}
