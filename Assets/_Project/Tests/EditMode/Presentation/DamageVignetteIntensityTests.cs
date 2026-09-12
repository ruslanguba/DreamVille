using NUnit.Framework;

using DreamVille.Presentation.Damage;

namespace DreamVille.Tests.EditMode.Presentation
{
    /// <summary><see cref="DamageVignetteIntensity"/> hit-count-to-intensity curve.</summary>
    public class DamageVignetteIntensityTests
    {
        [Test]
        public void Calculate_ZeroHits_ReturnsZero()
        {
            float intensity = DamageVignetteIntensity.Calculate(hits: 0, maxHits: 3, firstHitIntensity: 0.6f);

            Assert.That(intensity, Is.EqualTo(0f));
        }

        [Test]
        public void Calculate_FirstHit_ReturnsFirstHitIntensity()
        {
            float intensity = DamageVignetteIntensity.Calculate(hits: 1, maxHits: 3, firstHitIntensity: 0.6f);

            Assert.That(intensity, Is.EqualTo(0.6f));
        }

        [Test]
        public void Calculate_MaxHits_ReturnsOne()
        {
            float intensity = DamageVignetteIntensity.Calculate(hits: 3, maxHits: 3, firstHitIntensity: 0.6f);

            Assert.That(intensity, Is.EqualTo(1f));
        }

        [Test]
        public void Calculate_MiddleHit_LerpsBetweenFirstAndOne()
        {
            float intensity = DamageVignetteIntensity.Calculate(hits: 2, maxHits: 3, firstHitIntensity: 0.6f);

            Assert.That(intensity, Is.EqualTo(0.8f).Within(1e-5f));
        }

        [Test]
        public void Calculate_MaxHitsOne_ReturnsOne()
        {
            float intensity = DamageVignetteIntensity.Calculate(hits: 1, maxHits: 1, firstHitIntensity: 0.6f);

            Assert.That(intensity, Is.EqualTo(1f));
        }
    }
}
