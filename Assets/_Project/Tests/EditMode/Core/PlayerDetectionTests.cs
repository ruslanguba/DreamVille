using NUnit.Framework;
using UnityEngine;

using DreamVille.Core.Players;
using DreamVille.Tests.Support;

namespace DreamVille.Tests.EditMode.Core
{
    /// <summary><see cref="PlayerDetection"/>: a collider belongs to the player when a live marker sits on it or a parent.</summary>
    public class PlayerDetectionTests
    {
        private GameObject _root;
        private BoxCollider _collider;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("PlayerRoot");
            _collider = new GameObject("Hitbox").AddComponent<BoxCollider>();
            _collider.transform.SetParent(_root.transform);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void Detects_MarkerOnAParent_AsThePlayer()
        {
            _root.AddComponent<PlayerMarkerBehaviour>();

            bool isPlayer = PlayerDetection.TryGetPlayer(_collider, out IPlayerMarker marker);

            Assert.That(isPlayer, Is.True);
            Assert.That(marker.Transform, Is.SameAs(_root.transform));
        }

        [Test]
        public void Detects_DestroyedComponent_AsAbsent()
        {
            var marker = _root.AddComponent<PlayerMarkerBehaviour>();
            Object.DestroyImmediate(marker);

            bool isPlayer = PlayerDetection.IsPlayer(_collider);

            Assert.That(isPlayer, Is.False);
        }

        [Test]
        public void IsPlayer_NullCollider_IsFalse()
        {
            bool isPlayer = PlayerDetection.IsPlayer(null);

            Assert.That(isPlayer, Is.False);
        }
    }
}
