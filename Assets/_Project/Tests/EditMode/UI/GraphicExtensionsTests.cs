using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

using DreamVille.UI;

namespace DreamVille.Tests.EditMode.UI
{
    public class GraphicExtensionsTests
    {
        private GameObject _gameObject;
        private Image _image;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("Graphic", typeof(RectTransform));
            _image = _gameObject.AddComponent<Image>();
            _image.color = new Color(0.2f, 0.4f, 0.6f, 1f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void SetAlpha_ChangesAlphaAndKeepsRgb()
        {
            _image.SetAlpha(0.25f);

            Assert.That(_image.color.r, Is.EqualTo(0.2f).Within(1e-6f));
            Assert.That(_image.color.g, Is.EqualTo(0.4f).Within(1e-6f));
            Assert.That(_image.color.b, Is.EqualTo(0.6f).Within(1e-6f));
            Assert.That(_image.color.a, Is.EqualTo(0.25f).Within(1e-6f));
        }

        [Test]
        public void SetAlpha_BelowZero_ClampsToZero()
        {
            _image.SetAlpha(-0.5f);

            Assert.That(_image.color.a, Is.EqualTo(0f));
        }

        [Test]
        public void SetAlpha_AboveOne_ClampsToOne()
        {
            _image.SetAlpha(1.5f);

            Assert.That(_image.color.a, Is.EqualTo(1f));
        }
    }
}
