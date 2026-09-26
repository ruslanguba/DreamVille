using NUnit.Framework;
using UnityEngine;

using DreamVille.Gameplay.Enemies;

namespace DreamVille.Tests.EditMode.Enemies
{
    /// <summary>
    /// <see cref="EnemyHealth"/> as an objective member. Awake does not run in EditMode, which is exactly
    /// the state of an inactive enemy still waiting for its wave.
    /// </summary>
    public class EnemyHealthTests
    {
        private GameObject _host;

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
        }

        [Test]
        public void IsDead_BeforeAwake_IsFalse()
        {
            _host = new GameObject("Enemy");
            var health = _host.AddComponent<EnemyHealth>();
            Assert.That(health, Is.Not.Null);

            bool isDead = health.IsDead;

            Assert.That(isDead, Is.False);
        }
    }
}
