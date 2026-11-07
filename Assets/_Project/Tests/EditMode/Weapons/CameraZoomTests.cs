using NUnit.Framework;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

using DreamVille.Gameplay.Weapons;

namespace DreamVille.Tests.EditMode.Weapons
{
    /// <summary>
    /// <see cref="CameraZoom"/> keeps no zoom copy: each scroll step reads and writes the orbital follow radius, clamped to
    /// the range. OnEnable does not run here, so a cached value would still be unset.
    /// </summary>
    public class CameraZoomTests
    {
        private GameObject _gameObject;
        private CinemachineOrbitalFollow _orbitalFollow;
        private CameraZoom _zoom;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("Camera");
            _orbitalFollow = _gameObject.AddComponent<CinemachineOrbitalFollow>();
            _orbitalFollow.Radius = 5f;
            _zoom = _gameObject.AddComponent<CameraZoom>();

            var serialized = new SerializedObject(_zoom);
            serialized.FindProperty("_minZoom").floatValue = 1f;
            serialized.FindProperty("_maxZoom").floatValue = 10f;
            serialized.FindProperty("_zoomSensitivity").floatValue = 1f;
            serialized.FindProperty("_orbitalFollow").objectReferenceValue = _orbitalFollow;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void Zoom_WhenScrolledIn_ShrinksTheRadiusBySensitivity()
        {
            _zoom.Zoom(1f);

            Assert.That(_orbitalFollow.Radius, Is.EqualTo(4f));
        }

        [TestCase(-6f, 10f)]
        [TestCase(100f, 1f)]
        public void Zoom_BeyondTheLimits_ClampsTheRadius(float scroll, float expected)
        {
            _zoom.Zoom(scroll);

            Assert.That(_orbitalFollow.Radius, Is.EqualTo(expected));
        }
    }
}
