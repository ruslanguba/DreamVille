using UnityEngine;

namespace DreamVille.Gameplay.Weapons.Aiming
{
    /// <summary>Physics.SphereCast against everything but the player and player bullets; triggers ignored.</summary>
    public sealed class PhysicsSphereCaster : ISphereCaster
    {
        private readonly int _mask;

        public PhysicsSphereCaster()
        {
            // The preview must never stop at the character casting it or at a projectile in flight.
            _mask = Physics.DefaultRaycastLayers & ~LayerMask.GetMask(Layers.Player, Layers.PlayerBullet);
        }

        public bool Cast(Vector3 origin, Vector3 direction, float radius, float maxDistance, out float distance, out Vector3 normal)
        {
            if (Physics.SphereCast(origin, radius, direction, out RaycastHit hit, maxDistance, _mask, QueryTriggerInteraction.Ignore))
            {
                distance = hit.distance;
                normal = hit.normal;
                return true;
            }

            distance = 0f;
            normal = Vector3.zero;
            return false;
        }
    }
}
