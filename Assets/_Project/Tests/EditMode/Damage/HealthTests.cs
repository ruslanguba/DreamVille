using System;
using NUnit.Framework;

using DreamVille.Gameplay.Damage;

namespace DreamVille.Tests.EditMode.Damage
{
    public class HealthTests
    {
        [Test]
        public void Constructor_MaxIsZeroOrNegative_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Health(0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Health(-5f));
        }

        [Test]
        public void TakeDamage_ReducesCurrent()
        {
            var health = new Health(100f);

            health.TakeDamage(30f);

            Assert.That(health.Current, Is.EqualTo(70f));
        }

        [Test]
        public void TakeDamage_HitThatReachesZero_ReturnsTrueAndKills()
        {
            var health = new Health(50f);

            bool killingHit = health.TakeDamage(50f);

            Assert.That(killingHit, Is.True);
            Assert.That(health.IsDead, Is.True);
        }

        [Test]
        public void TakeDamage_AfterDeath_ReturnsFalse()
        {
            var health = new Health(10f);
            health.TakeDamage(10f);

            bool result = health.TakeDamage(5f);

            Assert.That(result, Is.False);
            Assert.That(health.Current, Is.EqualTo(0f));
        }

        [Test]
        public void TakeDamage_NonPositiveAmount_IsIgnored()
        {
            var health = new Health(10f);

            bool zeroResult = health.TakeDamage(0f);
            bool negativeResult = health.TakeDamage(-1f);

            Assert.That(zeroResult, Is.False);
            Assert.That(negativeResult, Is.False);
            Assert.That(health.Current, Is.EqualTo(10f));
        }
    }
}
