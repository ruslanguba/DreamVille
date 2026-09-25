using System;
using UnityEngine;

using DreamVille.Configs;
using DreamVille.Gameplay.Weapons.Pillow;
using DreamVille.Persistence;

namespace DreamVille.Progression.Pillows
{
    /// <summary>
    /// Persisted pillow unlocks, capped by the current level's <see cref="LevelDefinition.MaxPillows"/>.
    /// Saves written before unlocks existed are seeded once from the levels they completed.
    /// </summary>
    public sealed class PillowInventory : IPillowInventory
    {
        private readonly IProgressRepository _progress;
        private readonly LevelDefinition _level;

        private int _unlocked;

        public PillowInventory(IProgressRepository progress, LevelDefinition level, SceneCatalog catalog)
        {
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _level = level != null ? level : throw new ArgumentNullException(nameof(level));

            if (catalog == null)
                throw new ArgumentNullException(nameof(catalog));

            _unlocked = _progress.GetUnlockedPillows();

            // 0 means the key was never written: the repository only stores counts that beat the stored one.
            if (_unlocked == 0)
                SeedFromCompletedLevels(catalog);
        }

        public event Action<int> AvailableChanged;

        public int Available => Mathf.Min(_unlocked, _level.MaxPillows);

        public bool IsUnlocked(int pillowNumber)
        {
            ThrowIfNotAPillowNumber(pillowNumber);
            return pillowNumber <= _unlocked;
        }

        public bool UnlockUpTo(int pillowNumber)
        {
            ThrowIfNotAPillowNumber(pillowNumber);

            if (pillowNumber <= _unlocked)
                return false;

            int availableBefore = Available;
            _progress.SetUnlockedPillowsIfHigher(pillowNumber);
            _unlocked = pillowNumber;

            if (Available > availableBefore)
                AvailableChanged?.Invoke(Available);

            return true;
        }

        /// <summary>
        /// A completed level that allows N pillows proves the player already passed the point where N are granted,
        /// so the save gets the highest such cap. Writing the seed makes the key present, so this writes at most once.
        /// </summary>
        private void SeedFromCompletedLevels(SceneCatalog catalog)
        {
            if (catalog.Levels == null)
                return;

            int seed = 0;
            foreach (LevelDefinition level in catalog.Levels)
            {
                if (level != null && level.MaxPillows > seed && _progress.IsCompleted(level.LevelId))
                    seed = level.MaxPillows;
            }

            if (seed > 0 && _progress.SetUnlockedPillowsIfHigher(seed))
                _unlocked = seed;
        }

        private static void ThrowIfNotAPillowNumber(int pillowNumber)
        {
            if (pillowNumber < 1)
                throw new ArgumentOutOfRangeException(nameof(pillowNumber), pillowNumber, "Pillow numbers start at 1.");
        }
    }
}
