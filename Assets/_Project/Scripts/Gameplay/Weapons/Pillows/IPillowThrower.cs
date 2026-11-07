using System;

namespace DreamVille.Gameplay.Weapons.Pillows
{
    /// <summary>What the HUD and the level see of the player's pillows: state to pull, not deltas to count.</summary>
    public interface IPillowThrower
    {
        /// <summary>Pillows the player may use in this level.</summary>
        int Available { get; }

        /// <summary>Available pillows currently in hand, ready to be thrown.</summary>
        int InHand { get; }

        /// <summary>Raised whenever <see cref="Available"/> or <see cref="InHand"/> changed (throw, arrival, unlock).</summary>
        event Action Changed;
    }
}
