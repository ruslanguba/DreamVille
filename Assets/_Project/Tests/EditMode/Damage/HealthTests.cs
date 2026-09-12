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
        public void TakeDamage_ReducesCurrentAndRaisesChanged()
        {
            var health = new Health(100f);
            float? changedTo = null;
            health.Changed += value => changedTo = value;

            health.TakeDamage(30f);

            Assert.That(health.Current, Is.EqualTo(70f));
            Assert.That(changedTo, Is.EqualTo(70f));
        }

        [Test]
        public void TakeDamage_HitThatReachesZero_ReturnsTrueAndRaisesDiedOnce()
        {
            var health = new Health(50f);
            int diedCount = 0;
            health.Died += () => diedCount++;

            bool killingHit = health.TakeDamage(50f);

            Assert.That(killingHit, Is.True);
            Assert.That(diedCount, Is.EqualTo(1));
        }

        [Test]
        public void TakeDamage_AfterDeath_ReturnsFalseAndRaisesNothing()
        {
            var health = new Health(10f);
            health.TakeDamage(10f);
            bool anyEventRaised = false;
            health.Changed += _ => anyEventRaised = true;
            health.Died += () => anyEventRaised = true;

            bool result = health.TakeDamage(5f);

            Assert.That(result, Is.False);
            Assert.That(anyEventRaised, Is.False);
        }

        [Test]
        public void TakeDamage_NonPositiveAmount_IsIgnored()
        {
            var health = new Health(10f);
            bool anyEventRaised = false;
            health.Changed += _ => anyEventRaised = true;

            bool zeroResult = health.TakeDamage(0f);
            bool negativeResult = health.TakeDamage(-1f);

            Assert.That(zeroResult, Is.False);
            Assert.That(negativeResult, Is.False);
            Assert.That(health.Current, Is.EqualTo(10f));
            Assert.That(anyEventRaised, Is.False);
        }
    }
}
