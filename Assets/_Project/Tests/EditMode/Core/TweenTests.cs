using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

using DreamVille.Core.Async;

namespace DreamVille.Tests.EditMode.Core
{
    /// <summary>Progress clamping and the synchronous edges of <see cref="Tween"/>; the frame loop itself needs the player loop and is smoke-tested.</summary>
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
        public void RunAsync_NonPositiveDuration_AppliesOneSynchronously()
        {
            var applied = new List<float>();

            UniTask task = Tween.RunAsync(0f, applied.Add, CancellationToken.None);

            Assert.That(applied, Is.EqualTo(new[] { 1f }));
            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
        }

        [Test]
        public void RunAsync_CancelledToken_IsCanceled()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            var applied = new List<float>();

            UniTask task = Tween.RunAsync(1f, applied.Add, cts.Token);

            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));
            Assert.That(applied, Is.Empty);
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
