using System;
using System.Collections.Generic;

using DreamVille.Core.Events;

namespace DreamVille.Gameplay.Enemies
{
    /// <summary>
    /// Tracks the enemies alive in the current level. The roster is every enemy in the scene, inactive ones included
    /// (an enemy may be waiting for its wave); the container hands it over when it builds the registry, i.e. before
    /// any Initialize runs, so subscribers can read up-to-date state right away regardless of composition order.
    /// </summary>
    public sealed class EnemyRegistry : IEnemyRegistry, IDisposable
    {
        public event Action<int, int> EnemyKilled;

        public event Action AllEnemiesKilled;

        private readonly List<EnemyHealth> _alive = new();

        public EnemyRegistry(List<EnemyHealth> enemies)
        {
            if (enemies == null)
                throw new ArgumentNullException(nameof(enemies));

            foreach (EnemyHealth enemy in enemies)
            {
                if (enemy == null || _alive.Contains(enemy))
                    continue;

                _alive.Add(enemy);
                enemy.Killed += OnEnemyKilled;
            }
        }

        public int RemainingEnemies => _alive.Count;

        /// <summary>Unsubscribes from the enemies still alive; deaths after this are not reported.</summary>
        public void Dispose()
        {
            foreach (EnemyHealth enemy in _alive)
            {
                if (enemy != null)
                    enemy.Killed -= OnEnemyKilled;
            }

            _alive.Clear();
        }

        private void OnEnemyKilled(EnemyHealth enemy, int hitCount)
        {
            enemy.Killed -= OnEnemyKilled;

            if (!_alive.Remove(enemy))
                return;

            // Isolated: the HUD, scoring and the level director listen independently; one failure must not stop the rest.
            IsolatedEvent.Raise(EnemyKilled, _alive.Count, hitCount);

            if (_alive.Count == 0)
                IsolatedEvent.Raise(AllEnemiesKilled);
        }
    }
}
