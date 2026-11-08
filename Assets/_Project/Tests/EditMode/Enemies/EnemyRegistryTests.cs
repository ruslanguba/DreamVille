using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using DreamVille.Gameplay.Enemies;

namespace DreamVille.Tests.EditMode.Enemies
{
    /// <summary>
    /// <see cref="EnemyRegistry"/> over real <see cref="EnemyHealth"/> components (max health 2): it reports each kill
    /// with the count left and the killing hit count, and the last kill once.
    /// </summary>
    public class EnemyRegistryTests
    {
        private readonly List<GameObject> _hosts = new List<GameObject>();
        private readonly List<string> _events = new List<string>();
        private EnemyRegistry _registry;

        [TearDown]
        public void TearDown()
        {
            _registry?.Dispose();
            foreach (GameObject host in _hosts)
                Object.DestroyImmediate(host);

            _hosts.Clear();
            _events.Clear();
        }

        private EnemyHealth Enemy()
        {
            var host = new GameObject("Enemy");
            _hosts.Add(host);
            return host.AddComponent<EnemyHealth>();
        }

        private void Track(params EnemyHealth[] enemies)
        {
            _registry = new EnemyRegistry(new List<EnemyHealth>(enemies));
            _registry.EnemyKilled += (remaining, hitCount) => _events.Add($"Killed:{remaining}:{hitCount}");
            _registry.AllEnemiesKilled += () => _events.Add("AllKilled");
        }

        [Test]
        public void EnemyKilled_ReportsRemainingAndHitCount()
        {
            EnemyHealth first = Enemy();
            Track(first, Enemy());

            first.TakeDamage(2f, 3);

            Assert.That(_events, Is.EqualTo(new[] { "Killed:1:3" }));
            Assert.That(_registry.RemainingEnemies, Is.EqualTo(1));
        }

        [Test]
        public void LastKill_RaisesAllEnemiesKilledOnce()
        {
            EnemyHealth first = Enemy();
            EnemyHealth second = Enemy();
            Track(first, second);

            first.TakeDamage(2f, 1);
            second.Kill(1);

            Assert.That(_events, Is.EqualTo(new[] { "Killed:1:1", "Killed:0:1", "AllKilled" }));
        }

        [Test]
        public void SameEnemyKilledTwice_CountsOnce()
        {
            EnemyHealth enemy = Enemy();
            Track(enemy, enemy, Enemy());

            enemy.TakeDamage(2f, 1);
            enemy.Kill(1);

            Assert.That(_events, Is.EqualTo(new[] { "Killed:1:1" }));
            Assert.That(_registry.RemainingEnemies, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_ThenKill_RaisesNothing()
        {
            EnemyHealth enemy = Enemy();
            Track(enemy);

            _registry.Dispose();
            enemy.TakeDamage(2f, 1);

            Assert.That(_events, Is.Empty);
        }
    }
}
