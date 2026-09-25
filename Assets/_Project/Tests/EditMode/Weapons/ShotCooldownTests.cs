using System;
using NUnit.Framework;

using DreamVille.Gameplay.Weapons;

namespace DreamVille.Tests.EditMode.Weapons
{
    public class ShotCooldownTests
    {
        private const float Interval = 0.5f;

        [Test]
        public void IsReady_BeforeFirstShot_ReturnsTrue()
        {
            var cooldown = new ShotCooldown(Interval);

            bool ready = cooldown.IsReady(0f);

            Assert.That(ready, Is.True);
        }

        [Test]
        public void Consume_ThenBeforeInterval_IsNotReady()
        {
            var cooldown = new ShotCooldown(Interval);

            cooldown.Consume(10f);

            Assert.That(cooldown.IsReady(10f + Interval * 0.9f), Is.False);
        }

        [Test]
        public void Consume_ThenAtExactlyInterval_IsReady()
        {
            var cooldown = new ShotCooldown(Interval);

            cooldown.Consume(10f);

            Assert.That(cooldown.IsReady(10f + Interval), Is.True);
        }

        [Test]
        public void Constructor_NegativeInterval_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ShotCooldown(-0.1f));
        }

        [Test]
        public void Constructor_NaNInterval_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ShotCooldown(float.NaN));
        }
    }
}
