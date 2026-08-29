using System.Collections.Generic;
using System;
using UnityEngine;

namespace DreamVille.Gameplay.Enemies
{
    public class EnemyRegistry : MonoBehaviour
    {
        public event Action<int, int> OnEnemyKilled;
        public event Action OnAllEnemiesKilled;

        private readonly List<EnemyHealth> _enemies = new();

        public int TotalEnemies => _enemies.Count;
        public int RemainingEnemies => _enemies.Count;


        public void Initialize()
        {
            RegisterAllEnemiesInScene();
        }    

        private void RegisterAllEnemiesInScene()
        {
            var enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

            foreach (var enemy in enemies)
            {
                Register(enemy);
            }
        }

        private void Register(EnemyHealth enemy)
        {
            _enemies.Add(enemy);
            enemy.OnDeath += OnEnemyDeath;
        }

        private void OnEnemyDeath(EnemyHealth enemy, int collisionsCount)
        {
            enemy.OnDeath -= OnEnemyDeath;
            _enemies.Remove(enemy);

            OnEnemyKilled?.Invoke(_enemies.Count, collisionsCount);

            if (_enemies.Count == 0)
            {
                OnAllEnemiesKilled?.Invoke();
            }
        }

        private void OnDestroy()
        {
            foreach (var enemy in _enemies)
            {
                if (enemy != null)
                    enemy.OnDeath -= OnEnemyDeath;
            }

            _enemies.Clear();
        }
    }
}
