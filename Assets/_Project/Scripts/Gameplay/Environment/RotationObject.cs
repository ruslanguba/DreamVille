using UnityEngine;

namespace DreamVille.Gameplay.Environment
{
    /// <summary>
    /// Spins this object at constant per-axis speeds, in degrees per second of game time. Also the marker that makes
    /// <see cref="Player.Ground.GroundSurfaceClassifier"/> treat it as a rotating platform.
    /// </summary>
    public sealed class RotationObject : MonoBehaviour
    {
        [SerializeField] private float _speedRotationX;
        [SerializeField] private float _speedRotationY;
        [SerializeField] private float _speedRotationZ;

        private void Update()
        {
            transform.Rotate(_speedRotationX * Time.deltaTime, _speedRotationY * Time.deltaTime, _speedRotationZ * Time.deltaTime);
        }
    }
}
