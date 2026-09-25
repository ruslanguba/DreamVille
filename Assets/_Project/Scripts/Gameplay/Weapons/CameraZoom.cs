using Unity.Cinemachine;
using UnityEngine;
using Zenject;

using DreamVille.Input;

namespace DreamVille.Gameplay.Weapons
{
    public class CameraZoom : MonoBehaviour
    {
        [SerializeField] private float _minZoom;
        [SerializeField] private float _maxZoom;
        [SerializeField] private float _zoomSensitivity;
        [SerializeField] private float _currentZoom;
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
            _currentZoom = _orbitalFollow.Radius;
        }

        private void OnDisable()
        {
            _input.Scroll -= Zoom;
        }

        public void Zoom(float zoom)
        {
            float newZoom = _currentZoom - zoom * _zoomSensitivity;
            newZoom = Mathf.Clamp(newZoom, _minZoom, _maxZoom);

            _currentZoom = newZoom;
            _orbitalFollow.Radius = _currentZoom;
        }
    }
}
