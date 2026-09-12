using UnityEngine;
using UnityEngine.Serialization;

namespace DreamVille.Gameplay.Weapons.Aiming
{
    /// <summary>Draws a bouncing aim preview by sphere-casting and reflecting off surfaces.</summary>
    public class BounceRayTrajectory : MonoBehaviour
    {
        [FormerlySerializedAs("line")]
        [SerializeField] private LineRenderer _line;

        [FormerlySerializedAs("origin")]
        [SerializeField] private Transform _origin;

        [FormerlySerializedAs("maxBounces")]
        [SerializeField] private int _maxBounces = 5;

        [FormerlySerializedAs("maxDistance")]
        [SerializeField] private float _maxDistance = 100f;

        [Header("Sphere settings")]
        [FormerlySerializedAs("sphereRadius")]
        [SerializeField] private float _sphereRadius = 0.25f;

        [Header("Hit mask")]
        [Tooltip("When enabled, the preview ignores the player and its bullets. Disable to use the custom mask below.")]
        [SerializeField] private bool _useDefaultHitMask = true;
        [SerializeField] private LayerMask _hitMask = ~0;

        private Vector3[] _points;

        private void Awake()
        {
            _points = new Vector3[_maxBounces + 2];

            if (_useDefaultHitMask)
            {
                // The preview must never hit a projectile in flight or the character casting the ray.
                _hitMask = Physics.DefaultRaycastLayers & ~LayerMask.GetMask(Layers.Player, Layers.PlayerBullet);
            }
        }

        /// <summary>Recomputes the bounced ray preview from the origin along the given direction.</summary>
        public void DrawTrajectory(Vector3 direction)
        {
            if (_origin == null || _line == null) return;

            Vector3 currentPos = _origin.position;
            Vector3 currentDir = direction.normalized;

            int count = 0;
            _points[count++] = currentPos;

            for (int i = 0; i < _maxBounces; i++)
            {
                if (Physics.SphereCast(currentPos, _sphereRadius, currentDir, out RaycastHit hit, _maxDistance, _hitMask, QueryTriggerInteraction.Ignore))
                {
                    // The sphere center at the moment of contact sits behind the hit point.
                    Vector3 sphereCenterHit = hit.point - currentDir * _sphereRadius;
                    _points[count++] = sphereCenterHit;

                    currentDir = Vector3.Reflect(currentDir, hit.normal);
                    currentPos = sphereCenterHit;
                }
                else
                {
                    _points[count++] = currentPos + currentDir * _maxDistance;
                    break;
                }
            }

            _line.positionCount = count;
            for (int i = 0; i < count; i++)
            {
                _line.SetPosition(i, _points[i]);
            }
        }

        /// <summary>Hides the preview line.</summary>
        public void Clear()
        {
            if (_line != null)
                _line.positionCount = 0;
        }
    }
}
