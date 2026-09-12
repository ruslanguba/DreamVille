using System;

namespace DreamVille.Core.Players
{
    /// <summary>
    /// A trigger volume that reports the player entering it.
    /// Implemented in Gameplay, consumed by Dialogue and UI.
    /// </summary>
    public interface IPlayerZone
    {
        event Action PlayerEntered;
    }
}
