using UnityEngine;

namespace DreamVille.Gameplay.Environment
{
    /// <summary>Spins this object at constant per-axis speeds and exposes the rotation applied since the last frame, so whatever stands on it can turn with it.</summary>
    public class RotationObject : MonoBehaviour
    {
        [SerializeField] private float _speedRotationX;
        [SerializeField] private float _speedRotationY;
        [SerializeField] private float _speedRotationZ;

        public Quaternion DeltaRot { get; private set; }

        private Quaternion _lastRotation;

        void Start()
        {
            _lastRotation = transform.rotation;
        }

        void LateUpdate()
        {
            // Rotation applied since the last frame.
            DeltaRot = transform.rotation * Quaternion.Inverse(_lastRotation);

            _lastRotation = transform.rotation;
        }

        void Update()
        {

            transform.Rotate(_speedRotationX * Time.deltaTime, _speedRotationY * Time.deltaTime, _speedRotationZ * Time.deltaTime);
        }
    }
}
