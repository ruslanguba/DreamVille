using System;
using NUnit.Framework;
using UnityEngine;

using DreamVille.Gameplay.Weapons;
using DreamVille.Gameplay.Weapons.Aiming;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Weapons
{
    /// <summary><see cref="BouncePath"/> against a <see cref="FakeSphereCaster"/> world of analytic planes.</summary>
    public class BouncePathTests
    {
        private const float Radius = 0.5f;
        private const float Tolerance = 1e-4f;

        private FakeSphereCaster _caster;

        [SetUp]
        public void SetUp()
        {
            _caster = new FakeSphereCaster();
        }

        [Test]
        public void Compute_NoHit_EndsAtMaxLengthAlongRay()
        {
            var ray = new ShotRay(Vector3.zero, Vector3.forward);
            Vector3[] points = Buffer(maxContacts: 3);

            int count = BouncePath.Compute(ray, Radius, 3, 20f, _caster, points);

            Assert.That(count, Is.EqualTo(2));
            AssertNear(points[0], Vector3.zero);
            AssertNear(points[1], new Vector3(0f, 0f, 20f));
        }

        [Test]
        public void Compute_WallAtAnAngle_ContactIsSphereCentreAtCastDistance()
        {
            // A wall through (0, 0, 10) tilted 45°: the centre touches it where its distance to the plane equals the
            // radius, which is on the ray at z = 10 - r√2, not "hit point minus r along the ray".
            _caster.AddPlane(new Vector3(0f, 0f, 10f), new Vector3(-1f, 0f, -1f));
            var ray = new ShotRay(Vector3.zero, Vector3.forward);
            Vector3[] points = Buffer(maxContacts: 1);

            int count = BouncePath.Compute(ray, Radius, 1, 100f, _caster, points);

            Assert.That(count, Is.EqualTo(2));
            AssertNear(points[1], new Vector3(0f, 0f, 10f - Radius * Mathf.Sqrt(2f)));
        }

        [Test]
        public void Compute_StopsAfterMaxContacts()
        {
            // A corridor the diagonal ray bounces along forever; the bullet dies on its third contact.
            _caster.AddPlane(new Vector3(2f, 0f, 0f), Vector3.left);
            _caster.AddPlane(new Vector3(-2f, 0f, 0f), Vector3.right);
            var ray = new ShotRay(Vector3.zero, new Vector3(1f, 0f, 1f));
            Vector3[] points = Buffer(maxContacts: 3);

            int count = BouncePath.Compute(ray, Radius, 3, 1000f, _caster, points);

            Assert.That(count, Is.EqualTo(4));
            Assert.That(_caster.CastCount, Is.EqualTo(3));
            Assert.That(Mathf.Abs(points[3].x), Is.EqualTo(2f - Radius).Within(Tolerance));
        }

        [Test]
        public void Compute_LengthBudgetExhausted_TruncatesLastSegment()
        {
            // Contact at z = 3 after 3 m, reflected back, 2 m of budget left: the path ends at z = 1.
            _caster.AddPlane(new Vector3(0f, 0f, 3.5f), Vector3.back);
            var ray = new ShotRay(Vector3.zero, Vector3.forward);
            Vector3[] points = Buffer(maxContacts: 3);

            int count = BouncePath.Compute(ray, Radius, 3, 5f, _caster, points);

            Assert.That(count, Is.EqualTo(3));
            AssertNear(points[1], new Vector3(0f, 0f, 3f));
            AssertNear(points[2], new Vector3(0f, 0f, 1f));
        }

        [Test]
        public void Compute_PassesRadiusToCaster()
        {
            var ray = new ShotRay(Vector3.zero, Vector3.forward);

            BouncePath.Compute(ray, 0.25f, 3, 10f, _caster, Buffer(maxContacts: 3));

            Assert.That(_caster.LastRadius, Is.EqualTo(0.25f));
        }

        [Test]
        public void Compute_BufferSmallerThanRequiredCapacity_Throws()
        {
            var ray = new ShotRay(Vector3.zero, Vector3.forward);
            var points = new Vector3[BouncePath.RequiredCapacity(3) - 1];

            Assert.Throws<ArgumentException>(() => BouncePath.Compute(ray, Radius, 3, 10f, _caster, points));
        }

        private static Vector3[] Buffer(int maxContacts) => new Vector3[BouncePath.RequiredCapacity(maxContacts)];

        private static void AssertNear(Vector3 actual, Vector3 expected)
        {
            Assert.That(Vector3.Distance(actual, expected), Is.LessThan(Tolerance), $"Expected {expected}, was {actual}.");
        }
    }
}
