using System;

using DreamVille.Gameplay.Enemies;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>An enemy registry with a settable roster; raises its events only when a test asks.</summary>
    public sealed class FakeEnemyRegistry : IEnemyRegistry
    {
        public event Action<int, int> EnemyKilled;
        public event Action AllEnemiesKilled;

        public int RemainingEnemies { get; set; }

        /// <summary>Raises one kill with <paramref name="remaining"/> enemies left and the killing hit's hit count.</summary>
        public void RaiseEnemyKilled(int remaining, int hitCount)
        {
            RemainingEnemies = remaining;
            EnemyKilled?.Invoke(remaining, hitCount);
        }

        public void RaiseAllEnemiesKilled()
        {
            RemainingEnemies = 0;
            EnemyKilled?.Invoke(0, 0);
            AllEnemiesKilled?.Invoke();
        }
    }
}
