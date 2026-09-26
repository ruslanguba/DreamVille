using System;

using DreamVille.Gameplay.Enemies;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>An enemy registry with a settable roster; raises its events only when a test asks.</summary>
    public sealed class FakeEnemyRegistry : IEnemyRegistry
    {
        public event Action<int, int> EnemyKilled;
        public event Action AllEnemiesKilled;

        public int TotalEnemies { get; set; }
        public int RemainingEnemies { get; set; }

        public void RaiseAllEnemiesKilled()
        {
            RemainingEnemies = 0;
            EnemyKilled?.Invoke(0, 0);
            AllEnemiesKilled?.Invoke();
        }
    }
}
