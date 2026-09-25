using NUnit.Framework;

using DreamVille.Gameplay.Interactables;

namespace DreamVille.Tests.EditMode.Interactables
{
    /// <summary>Progress clamping and the grow/fly scale curves of <see cref="PickupTween"/>.</summary>
    public class PickupTweenTests
    {
        private const float Tolerance = 1e-5f;

        [Test]
        public void Progress_IsClampedToZeroAndOne()
        {
            float before = PickupTween.Progress(-1f, 0.5f);
            float half = PickupTween.Progress(0.25f, 0.5f);
            float after = PickupTween.Progress(1f, 0.5f);

            Assert.That(before, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(half, Is.EqualTo(0.5f).Within(Tolerance));
            Assert.That(after, Is.EqualTo(1f).Within(Tolerance));
        }

        [Test]
        public void Progress_WithNonPositiveDuration_IsComplete()
        {
            float zero = PickupTween.Progress(0f, 0f);
            float negative = PickupTween.Progress(0f, -1f);

            Assert.That(zero, Is.EqualTo(1f));
            Assert.That(negative, Is.EqualTo(1f));
        }

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
