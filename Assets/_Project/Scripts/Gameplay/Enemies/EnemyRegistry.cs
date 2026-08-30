using System.Collections.Generic;
using System;
using UnityEngine;

namespace DreamVille.Gameplay.Enemies
{
    /// <summary>
    /// Учёт живых врагов уровня.
    /// Состав собирается в Awake — то есть заведомо раньше любого Initialize контейнера,
    /// поэтому подписчики могут сразу читать актуальное состояние и не зависят от порядка.
    /// </summary>
    public class EnemyRegistry : MonoBehaviour
    {
        /// <summary>Убит враг. Аргументы: сколько осталось, сколько было столкновений.</summary>
        public event Action<int, int> OnEnemyKilled;

        /// <summary>Врагов на уровне не осталось.</summary>
        public event Action OnAllEnemiesKilled;

        private readonly List<EnemyHealth> _alive = new();

        /// <summary>Сколько врагов было на старте уровня.</summary>
        public int TotalEnemies { get; private set; }

        /// <summary>Сколько врагов осталось сейчас.</summary>
        public int RemainingEnemies => _alive.Count;

        private void Awake()
        {
            // Выключенные объекты тоже считаются: враг может ждать своей волны.
            foreach (var enemy in FindObjectsByType<EnemyHealth>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                _alive.Add(enemy);
                enemy.OnDeath += OnEnemyDeath;
            }

            TotalEnemies = _alive.Count;
        }

        private void OnEnemyDeath(EnemyHealth enemy, int collisionsCount)
        {
            enemy.OnDeath -= OnEnemyDeath;

            if (!_alive.Remove(enemy))
                return;

            OnEnemyKilled?.Invoke(_alive.Count, collisionsCount);

            if (_alive.Count == 0)
                OnAllEnemiesKilled?.Invoke();
        }

        private void OnDestroy()
        {
            foreach (var enemy in _alive)
                if (enemy != null)
                    enemy.OnDeath -= OnEnemyDeath;

            _alive.Clear();
        }
    }
}
