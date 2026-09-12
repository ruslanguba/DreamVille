using System;

namespace DreamVille.Core.Players
{
    /// <summary>Accumulated hits on the player as presentation sees them; implemented in Gameplay.</summary>
    public interface IPlayerDamageState
    {
        int Hits { get; }
        int MaxHits { get; }

        /// <summary>Raised after every registered hit with the new hit count (also at the cap).</summary>
        event Action<int> HitsChanged;

        /// <summary>Raised when the hit count falls back to zero after the recovery delay.</summary>
        event Action Recovered;
    }
}
