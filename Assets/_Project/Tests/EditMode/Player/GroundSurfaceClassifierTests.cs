using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using DreamVille.Gameplay.Environment;
using DreamVille.Gameplay.Player.Ground;
using DreamVille.Gameplay.Weapons.Pillow;

namespace DreamVille.Tests.EditMode.Player
{
    /// <summary><see cref="GroundSurfaceClassifier"/> against real <see cref="Pillow"/>/<see cref="RotationObject"/> components.</summary>
    public class GroundSurfaceClassifierTests
    {
        private readonly List<GameObject> _createdObjects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject created in _createdObjects)
                Object.DestroyImmediate(created);

            _createdObjects.Clear();
        }

        private Collider CreateColliderWith<T>() where T : Component
        {
            var gameObject = new GameObject(typeof(T).Name);
            _createdObjects.Add(gameObject);
            gameObject.AddComponent<T>();
            return gameObject.AddComponent<BoxCollider>();
        }

        private Collider CreatePlainCollider()
        {
            var gameObject = new GameObject("Plain");
            _createdObjects.Add(gameObject);
            return gameObject.AddComponent<BoxCollider>();
        }

        [Test]
        public void Classify_NoHits_ReturnsNone()
        {
            var hits = new Collider[8];

            GroundSurface surface = GroundSurfaceClassifier.Classify(hits, 0);

            Assert.That(surface, Is.EqualTo(GroundSurface.None));
        }

        [Test]
        public void Classify_ColliderWithPillow_ReturnsPillow()
        {
            var hits = new Collider[8];
            hits[0] = CreateColliderWith<Pillow>();

            GroundSurface surface = GroundSurfaceClassifier.Classify(hits, 1);

            Assert.That(surface, Is.EqualTo(GroundSurface.Pillow));
        }

        [Test]
        public void Classify_ColliderWithRotationObject_ReturnsRotatingPlatform()
        {
            var hits = new Collider[8];
            hits[0] = CreateColliderWith<RotationObject>();

            GroundSurface surface = GroundSurfaceClassifier.Classify(hits, 1);

            Assert.That(surface, Is.EqualTo(GroundSurface.RotatingPlatform));
        }

        [Test]
        public void Classify_MixedHits_ReturnsBothFlags()
        {
            var hits = new Collider[8];
            hits[0] = CreateColliderWith<Pillow>();
            hits[1] = CreateColliderWith<RotationObject>();

            GroundSurface surface = GroundSurfaceClassifier.Classify(hits, 2);

            Assert.That(surface, Is.EqualTo(GroundSurface.Pillow | GroundSurface.RotatingPlatform));
        }

        [Test]
        public void Classify_IgnoresEntriesBeyondCount()
        {
            var hits = new Collider[8];
            hits[0] = CreateColliderWith<Pillow>();
            hits[1] = CreateColliderWith<RotationObject>();
            hits[2] = CreatePlainCollider();

            GroundSurface surface = GroundSurfaceClassifier.Classify(hits, 1);

            Assert.That(surface, Is.EqualTo(GroundSurface.Pillow));
        }
    }
}
