using UnityEngine;

namespace DreamVille.Core.Players
{
    /// <summary>
    /// Marks the root object of the player character. Lets non-gameplay code answer
    /// "is this collider the player" without referencing DreamVille.Gameplay.
    /// </summary>
    public interface IPlayerMarker
    {
        /// <summary>Transform of the player root, not of the collider that was hit.</summary>
        Transform Transform { get; }
    }
}
