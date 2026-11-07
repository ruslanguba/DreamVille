using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using DreamVille.Gameplay.Enemies;

using Object = UnityEngine.Object;

namespace DreamVille.Tests.EditMode.Enemies
{
    /// <summary>
    /// <see cref="EnemyHealth"/> as an objective member. Awake does not run in EditMode, which is exactly
    /// the state of an inactive enemy still waiting for its wave. The default max health is 2.
    /// </summary>
    public class EnemyHealthTests
    {
        private GameObject _host;
        private EnemyHealth _health;
        private List<string> _events;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("Enemy");
            _health = _host.AddComponent<EnemyHealth>();
            _events = new List<string>();
            _health.Killed += (enemy, hitCount) => _events.Add($"Killed:{hitCount}");
            _health.Died += _ => _events.Add("Died");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
        }

        [Test]
        public void IsDead_BeforeAwake_IsFalse()
        {
            bool isDead = _health.IsDead;

            Assert.That(isDead, Is.False);
        }

        [Test]
        public void TakeDamage_Lethal_RaisesKilledBeforeDied()
        {
            _health.TakeDamage(2f, 1);

            Assert.That(_events, Is.EqualTo(new[] { "Killed:1", "Died" }));
            Assert.That(_health.IsDead, Is.True);
        }

        [Test]
        public void TakeDamage_Lethal_PassesTheHitCount()
        {
            _health.TakeDamage(2f, 4);

            Assert.That(_events[0], Is.EqualTo("Killed:4"));
        }

        [Test]
        public void TakeDamage_WhileInvulnerable_DoesNotDie()
        {
            _health.SetVulnerable(false);

            _health.TakeDamage(2f, 1);

            Assert.That(_health.IsDead, Is.False);
            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void TakeDamage_AfterDeath_RaisesNothing()
        {
            _health.TakeDamage(2f, 1);
            _events.Clear();

            _health.TakeDamage(2f, 1);

            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void TakeDamage_WhenAKilledListenerThrows_StillRaisesDied()
        {
            _health.Killed += (enemy, hitCount) => throw new InvalidOperationException("listener failure");
            LogAssert.Expect(LogType.Exception, new Regex("listener failure"));

            _health.TakeDamage(2f, 1);

            Assert.That(_events, Is.EqualTo(new[] { "Killed:1", "Died" }));
        }
    }
}
