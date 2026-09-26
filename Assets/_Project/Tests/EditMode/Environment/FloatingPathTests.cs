using NUnit.Framework;
using UnityEngine;

using DreamVille.Gameplay.Environment;

namespace DreamVille.Tests.EditMode.Environment
{
    /// <summary>Target picking and height clamp of <see cref="FloatingPath"/>.</summary>
    public class FloatingPathTests
    {
        private const float MinY = -50f;
        private const float MaxY = 50f;
        private const float Tolerance = 1e-4f;

        [Test]
        public void PickTarget_WithinLimits_IsOriginPlusOffset()
        {
            Vector3 target = FloatingPath.PickTarget(new Vector3(1f, 2f, 3f), new Vector3(4f, 5f, 6f), MinY, MaxY);

            AssertVector(target, new Vector3(5f, 7f, 9f));
        }

        [Test]
        public void PickTarget_BelowMin_ClampsHeight()
        {
            Vector3 target = FloatingPath.PickTarget(new Vector3(1f, -45f, 3f), new Vector3(4f, -20f, 6f), MinY, MaxY);

            AssertVector(target, new Vector3(5f, MinY, 9f));
        }

        [Test]
        public void PickTarget_AboveMax_ClampsHeight()
        {
            Vector3 target = FloatingPath.PickTarget(new Vector3(1f, 45f, 3f), new Vector3(4f, 20f, 6f), MinY, MaxY);

            AssertVector(target, new Vector3(5f, MaxY, 9f));
        }

        private static void AssertVector(Vector3 actual, Vector3 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(Tolerance));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(Tolerance));
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(Tolerance));
        }
    }
}
