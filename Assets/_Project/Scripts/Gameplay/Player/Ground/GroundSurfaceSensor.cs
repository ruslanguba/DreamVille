using UnityEngine;
using Zenject;

namespace DreamVille.Gameplay.Player.Ground
{
    /// <summary>
    /// Zero-allocation ground query under the player's feet: one overlap sphere per <see cref="Sample"/>,
    /// classified into a <see cref="GroundSurface"/> that movement and respawn logic share.
    /// </summary>
    public sealed class GroundSurfaceSensor : MonoBehaviour, IGroundSensor
    {
        private const int HitBufferSize = 8;

        [SerializeField] private Transform _pivot;

        [SerializeField, Min(0f)] private float _radius = 0.5f;

        [Tooltip("Layers that count as ground. Exclude the Player layer so the character's own colliders do not fill the buffer.")]
        [SerializeField] private LayerMask _surfaceMask = ~0;

        private readonly Collider[] _hits = new Collider[HitBufferSize];
        private CharacterController _controller;
        private GroundSurface _surface;
        private bool _bufferFullReported;

        [Inject]
        private void Construct(CharacterController controller)
        {
            _controller = controller;
        }

        private void Awake()
        {
            if (_pivot != null)
                return;

            Debug.LogError($"[{nameof(GroundSurfaceSensor)}] Pivot is not assigned; ground sensing is disabled.", this);
            enabled = false;
        }

        public bool IsGrounded => _controller.isGrounded;
        public GroundSurface Surface => _surface;
        public bool IsSafeGround => IsGrounded && _surface == GroundSurface.None;

        public void Sample()
        {
            if (!enabled)
                return;

            int count = Physics.OverlapSphereNonAlloc(_pivot.position, _radius, _hits, _surfaceMask);
            if (count == _hits.Length && !_bufferFullReported)
            {
                _bufferFullReported = true;
                Debug.LogWarning($"[{nameof(GroundSurfaceSensor)}] {count} overlaps filled the buffer; surfaces beyond it are ignored. Check the surface mask.", this);
            }

            _surface = GroundSurfaceClassifier.Classify(_hits, count);
        }

        private void OnDrawGizmosSelected()
        {
            if (_pivot == null)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_pivot.position, _radius);
        }
    }
}
