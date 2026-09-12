using UnityEngine;

namespace DreamVille.Core.Players
{
    /// <summary>Single answer to "does this collider belong to the player".</summary>
    public static class PlayerDetection
    {
        /// <summary>
        /// Searches parents: the player's colliders live on child objects, while
        /// <see cref="IPlayerMarker"/> is implemented on the character root.
        /// </summary>
        public static bool IsPlayer(Collider collider) => TryGetPlayer(collider, out _);

        /// <inheritdoc cref="IsPlayer"/>
        public static bool TryGetPlayer(Collider collider, out IPlayerMarker player)
        {
            player = collider == null ? null : collider.GetComponentInParent<IPlayerMarker>();

            // A destroyed component still compares non-null through an interface reference.
            return player is Component component ? component != null : player != null;
        }
    }
}
