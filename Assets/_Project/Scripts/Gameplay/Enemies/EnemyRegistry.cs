using System.Collections.Generic;
using System;
using UnityEngine;

using DreamVille.Core.Events;

namespace DreamVille.Gameplay.Enemies
{
    /// <summary>
    /// Tracks the enemies alive in the current level.
    /// The roster is collected in Awake, i.e. strictly before any container Initialize runs,
    /// so subscribers can read up-to-date state right away regardless of composition order.
    /// </summary>
    public sealed class EnemyRegistry : MonoBehaviour, IEnemyRegistry
    {
        public event Action<int, int> EnemyKilled;

        public event Action AllEnemiesKilled;

        private readonly List<EnemyHealth> _alive = new();

        public int RemainingEnemies => _alive.Count;

        private void Awake()
        {
            // Inactive objects count too: an enemy may be waiting for its wave.
            foreach (var enemy in FindObjectsByType<EnemyHealth>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                _alive.Add(enemy);
                enemy.Killed += OnEnemyKilled;
            }
        }

        private void OnEnemyKilled(EnemyHealth enemy, int hitCount)
        {
            enemy.Killed -= OnEnemyKilled;

            if (!_alive.Remove(enemy))
                return;

            // Isolated: the HUD, scoring and the level director listen independently; one failure must not stop the rest.
            IsolatedEvent.Raise(EnemyKilled, _alive.Count, hitCount, this);

            if (_alive.Count == 0)
                IsolatedEvent.Raise(AllEnemiesKilled, this);
        }

        private void OnDestroy()
        {
            foreach (var enemy in _alive)
                if (enemy != null)
                    enemy.Killed -= OnEnemyKilled;

            _alive.Clear();
        }
    }
}
