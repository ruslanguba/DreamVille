using System;

namespace DreamVille.Gameplay.Enemies
{
    /// <summary>Tracks the enemies alive in the current level.</summary>
    public interface IEnemyRegistry
    {
        /// <summary>Arguments: enemies remaining, ricochet count of the killing hit.</summary>
        event Action<int, int> EnemyKilled;

        /// <summary>Raised once no enemy is left alive.</summary>
        event Action AllEnemiesKilled;

        /// <summary>How many enemies were present when the level started.</summary>
        int TotalEnemies { get; }

        /// <summary>How many enemies are alive right now.</summary>
        int RemainingEnemies { get; }
    }
}
