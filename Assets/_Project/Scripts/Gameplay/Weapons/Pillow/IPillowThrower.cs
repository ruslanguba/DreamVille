using System;

namespace DreamVille.Gameplay.Weapons.Pillow
{
    /// <summary>Throws and recalls the player's pillow platforms.</summary>
    public interface IPillowThrower
    {
        /// <summary>Raised after a pillow is thrown.</summary>
        event Action Thrown;

        /// <summary>Raised after a recalled pillow arrives back at the spawn point.</summary>
        event Action Recalled;

        /// <summary>Raised once during initialization with the number of available pillow slots.</summary>
        event Action<int> PillowCountChanged;

        /// <summary>Called by the presenter until phase 4.4 moves pillow setup into the player prefab.</summary>
        void Initialize();
    }
}
