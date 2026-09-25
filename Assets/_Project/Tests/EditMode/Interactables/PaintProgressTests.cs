using System;
using NUnit.Framework;

using DreamVille.Gameplay.Interactables.Effects;

namespace DreamVille.Tests.EditMode.Interactables
{
    /// <summary>Stroke counting and the shader ramp of <see cref="PaintProgress"/> (five strokes, 0..2 like the shipped paintable).</summary>
    public class PaintProgressTests
    {
        private const float Tolerance = 1e-5f;

        [Test]
        public void TryAddStroke_UpToStrokeCount_ReturnsTrue()
        {
            var progress = new PaintProgress(5);

            for (int stroke = 1; stroke <= 5; stroke++)
            {
                Assert.That(progress.IsComplete, Is.False, $"complete before stroke {stroke}");
                Assert.That(progress.TryAddStroke(), Is.True, $"stroke {stroke}");
            }

            Assert.That(progress.IsComplete, Is.True);
        }

        [Test]
        public void TryAddStroke_WhenComplete_ReturnsFalseAndKeepsTransient()
        {
            var progress = new PaintProgress(5);
            for (int stroke = 0; stroke < 5; stroke++)
                progress.TryAddStroke();

            bool added = progress.TryAddStroke();

            Assert.That(added, Is.False);
            Assert.That(progress.Strokes, Is.EqualTo(5));
            Assert.That(progress.Transient, Is.EqualTo(PaintProgress.TransientAtComplete).Within(Tolerance));
        }

        [TestCase(1, 0.4f)]
        [TestCase(5, 2f)]
        public void Transient_IsStrokeShareTimesTwo(int strokes, float expected)
        {
            var progress = new PaintProgress(5);

            for (int stroke = 0; stroke < strokes; stroke++)
                progress.TryAddStroke();

            Assert.That(progress.Transient, Is.EqualTo(expected).Within(Tolerance));
        }

        [Test]
        public void Constructor_WithNonPositiveStrokeCount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PaintProgress(0));
        }
    }
}
