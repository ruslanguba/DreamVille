using Unity.Cinemachine;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] private float _minZoom;
    [SerializeField] private float _maxZoom;
    [SerializeField] private float _zoomSensitivity;
    [SerializeField] private float _currentZoom;
    [SerializeField] private CinemachineOrbitalFollow _orbitalFollow;
    [SerializeField] private IPlayerInput _inputReader;


    private void OnEnable()
    {
        _orbitalFollow.GetComponentInChildren<CinemachineOrbitalFollow>();
        _inputReader = GetComponent<IPlayerInput>();
        _inputReader.Scroll += Zoom;
        _currentZoom = _orbitalFollow.Radius;
    }
    private void OnDisable()
    {
        _inputReader.Scroll -= Zoom;
    }

    public void Zoom(float zoom)
    {
        float newZoom = _currentZoom - zoom * _zoomSensitivity;
        newZoom = Mathf.Clamp(newZoom, _minZoom, _maxZoom);

        _currentZoom = newZoom;
        _orbitalFollow.Radius = _currentZoom;
    }
}
