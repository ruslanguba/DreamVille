using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using DreamVille.Configs;
using DreamVille.Gameplay.Damage;

namespace DreamVille.Tests.EditMode.Player
{
    /// <summary><see cref="PlayerHealth"/> hit counting and recovery timer.</summary>
    public class PlayerHealthTests
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object created in _createdObjects)
                Object.DestroyImmediate(created);

            _createdObjects.Clear();
        }

        private T Track<T>(T asset) where T : Object
        {
            _createdObjects.Add(asset);
            return asset;
        }

        [Test]
        public void RegisterHit_IncrementsHitsAndRaisesHitsChanged()
        {
            var config = Track(PlayerConfig.CreateForTests(maxHits: 3, hitRecoveryDelay: 5f));
            var health = new PlayerHealth(config);
            int? raisedHits = null;
            health.HitsChanged += hits => raisedHits = hits;

            health.RegisterHit();

            Assert.That(health.Hits, Is.EqualTo(1));
            Assert.That(raisedHits, Is.EqualTo(1));
        }

        [Test]
        public void RegisterHit_AtMaxHits_StaysAtMaxAndStillRaises()
        {
            var config = Track(PlayerConfig.CreateForTests(maxHits: 2, hitRecoveryDelay: 5f));
            var health = new PlayerHealth(config);
            int raiseCount = 0;
            health.HitsChanged += _ => raiseCount++;

            health.RegisterHit();
            health.RegisterHit();
            health.RegisterHit();

            Assert.That(health.Hits, Is.EqualTo(2));
            Assert.That(raiseCount, Is.EqualTo(3));
        }

        [Test]
        public void Tick_AfterRecoveryDelay_ResetsHitsAndRaisesRecovered()
        {
            var config = Track(PlayerConfig.CreateForTests(maxHits: 3, hitRecoveryDelay: 5f));
            var health = new PlayerHealth(config);
            int recoveredCount = 0;
            health.Recovered += () => recoveredCount++;
            health.RegisterHit();

            health.Tick(5f);

            Assert.That(health.Hits, Is.EqualTo(0));
            Assert.That(recoveredCount, Is.EqualTo(1));
        }

        [Test]
        public void Tick_BeforeRecoveryDelay_KeepsHits()
        {
            var config = Track(PlayerConfig.CreateForTests(maxHits: 3, hitRecoveryDelay: 5f));
            var health = new PlayerHealth(config);
            bool recovered = false;
            health.Recovered += () => recovered = true;
            health.RegisterHit();

            health.Tick(2f);

            Assert.That(health.Hits, Is.EqualTo(1));
            Assert.That(recovered, Is.False);
        }

        [Test]
        public void RegisterHit_RestartsRecoveryTimer()
        {
            var config = Track(PlayerConfig.CreateForTests(maxHits: 3, hitRecoveryDelay: 5f));
            var health = new PlayerHealth(config);
            bool recovered = false;
            health.Recovered += () => recovered = true;
            health.RegisterHit();
            health.Tick(3f);

            health.RegisterHit();
            health.Tick(3f);

            Assert.That(health.Hits, Is.EqualTo(2));
            Assert.That(recovered, Is.False);
        }

        [Test]
        public void Tick_WhenUnhurt_DoesNotRaiseRecovered()
        {
            var config = Track(PlayerConfig.CreateForTests(maxHits: 3, hitRecoveryDelay: 5f));
            var health = new PlayerHealth(config);
            bool recovered = false;
            health.Recovered += () => recovered = true;

            health.Tick(100f);

            Assert.That(recovered, Is.False);
        }
    }
}
