using UnityEngine;

namespace DreamVille.Gameplay.Weapons.Aiming
{
    /// <summary>Physics seam for the aim preview: sweep a sphere, report the travelled distance and the surface normal.</summary>
    public interface ISphereCaster
    {
        /// <summary>
        /// Sweeps a sphere of <paramref name="radius"/> from <paramref name="origin"/> along <paramref name="direction"/>.
        /// On a hit, <paramref name="distance"/> is how far the sphere centre travelled before the contact.
        /// </summary>
        bool Cast(Vector3 origin, Vector3 direction, float radius, float maxDistance, out float distance, out Vector3 normal);
    }
}
