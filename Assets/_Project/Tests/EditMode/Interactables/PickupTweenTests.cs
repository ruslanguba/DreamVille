using NUnit.Framework;

using DreamVille.Gameplay.Interactables;

namespace DreamVille.Tests.EditMode.Interactables
{
    /// <summary>The grow/fly scale curves of <see cref="PickupTween"/>.</summary>
    public class PickupTweenTests
    {
        private const float Tolerance = 1e-5f;

        [Test]
        public void GrowScale_GoesFromOneToGrowFactor()
        {
            float start = PickupTween.GrowScale(0f);
            float end = PickupTween.GrowScale(1f);

            Assert.That(start, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(end, Is.EqualTo(PickupTween.GrowFactor).Within(Tolerance));
            Assert.That(PickupTween.GrowFactor, Is.EqualTo(1.3f));
        }

        [Test]
        public void FlyScale_GoesFromGrowFactorToZero()
        {
            float start = PickupTween.FlyScale(0f);
            float end = PickupTween.FlyScale(1f);

            Assert.That(start, Is.EqualTo(PickupTween.GrowFactor).Within(Tolerance));
            Assert.That(end, Is.EqualTo(0f).Within(Tolerance));
        }
    }
}
