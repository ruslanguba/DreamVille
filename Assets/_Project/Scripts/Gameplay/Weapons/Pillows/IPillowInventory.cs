using System;

namespace DreamVille.Gameplay.Weapons.Pillows
{
    /// <summary>Pillow unlocks as the current level sees them. Implemented by Progression, consumed by the player.</summary>
    public interface IPillowInventory
    {
        /// <summary>Pillows usable in this level: min(unlocked, level cap).</summary>
        int Available { get; }

        /// <summary>Raised with the new value when <see cref="Available"/> grows.</summary>
        event Action<int> AvailableChanged;

        /// <summary>True when pillow number <paramref name="pillowNumber"/> (1-based) is unlocked, regardless of the level cap.</summary>
        bool IsUnlocked(int pillowNumber);

        /// <summary>Unlocks every pillow up to <paramref name="pillowNumber"/>; idempotent, never lowers, persists. True when the count grew.</summary>
        bool UnlockUpTo(int pillowNumber);
    }
}
