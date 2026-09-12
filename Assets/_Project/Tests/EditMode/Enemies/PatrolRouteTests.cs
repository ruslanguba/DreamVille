using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using DreamVille.Gameplay.Enemies;

namespace DreamVille.Tests.EditMode.Enemies
{
    /// <summary>
    /// <see cref="PatrolRoute.WithoutEmptySlots"/> is what makes a designer-authored route with
    /// unfilled slots behave like the old Enemy did: a route of nothing but empty slots is no route,
    /// so the Patrol state faces the player instead of walking on the spot.
    /// </summary>
    public class PatrolRouteTests
    {
        private readonly List<GameObject> _created = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject created in _created)
                Object.DestroyImmediate(created);

            _created.Clear();
        }

        [Test]
        public void WithoutEmptySlots_NullArray_ReturnsEmptyRoute()
        {
            Transform[] route = PatrolRoute.WithoutEmptySlots(null);

            Assert.That(route, Is.Empty);
        }

        [Test]
        public void WithoutEmptySlots_AllSlotsEmpty_ReturnsEmptyRoute()
        {
            Transform[] route = PatrolRoute.WithoutEmptySlots(new Transform[3]);

            Assert.That(route, Is.Empty);
        }

        [Test]
        public void WithoutEmptySlots_MixedSlots_KeepsAssignedPointsInOrder()
        {
            Transform first = Point("first");
            Transform second = Point("second");

            Transform[] route = PatrolRoute.WithoutEmptySlots(new[] { null, first, null, second });

            Assert.That(route, Is.EqualTo(new[] { first, second }));
        }

        [Test]
        public void WithoutEmptySlots_FullyAssigned_ReturnsTheSameArray()
        {
            Transform[] points = { Point("a"), Point("b") };

            Transform[] route = PatrolRoute.WithoutEmptySlots(points);

            Assert.That(route, Is.SameAs(points));
        }

        private Transform Point(string name)
        {
            var point = new GameObject(name);
            _created.Add(point);
            return point.transform;
        }
    }
}
