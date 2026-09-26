using NUnit.Framework;

using DreamVille.Core.Async;

namespace DreamVille.Tests.EditMode.Core
{
    /// <summary>Progress clamping of <see cref="Tween"/>; the frame loop itself needs the player loop and is smoke-tested.</summary>
    public class TweenTests
    {
        private const float Tolerance = 1e-5f;

        [TestCase(0f)]
        [TestCase(-1f)]
        public void Progress_NonPositiveDuration_IsComplete(float duration)
        {
            float progress = Tween.Progress(0f, duration);

            Assert.That(progress, Is.EqualTo(1f));
        }

        [Test]
        public void Progress_HalfElapsed_IsHalf()
        {
            float progress = Tween.Progress(0.25f, 0.5f);

            Assert.That(progress, Is.EqualTo(0.5f).Within(Tolerance));
        }

        [Test]
        public void Progress_PastDuration_ClampsToOne()
        {
            float progress = Tween.Progress(2f, 0.5f);

            Assert.That(progress, Is.EqualTo(1f));
        }

        [Test]
        public void Progress_NegativeElapsed_ClampsToZero()
        {
            float progress = Tween.Progress(-1f, 0.5f);

            Assert.That(progress, Is.EqualTo(0f));
        }
    }
}
