using NUnit.Framework;
using UnityEngine;

using DreamVille.Gameplay.Interactables;
using DreamVille.Tests.Support;

namespace DreamVille.Tests.EditMode.Interactables
{
    /// <summary>
    /// The shot host: applies sibling effects on every shot and ignores touches. Awake does not run in EditMode,
    /// so these tests also pin the host's lazy effect lookup.
    /// </summary>
    public class InteractableTests
    {
        private GameObject _host;
        private GameObject _bullet;
        private Interactable _interactable;
        private RecordingEffectBehaviour _first;
        private RecordingEffectBehaviour _second;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("Interactable");
            _bullet = new GameObject("Bullet");
            _interactable = _host.AddComponent<Interactable>();
            _first = _host.AddComponent<RecordingEffectBehaviour>();
            _second = _host.AddComponent<RecordingEffectBehaviour>();

            Assert.That(_first, Is.Not.Null);
            Assert.That(_second, Is.Not.Null);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
            Object.DestroyImmediate(_bullet);
        }

        [Test]
        public void OnInteract_FromShot_AppliesEverySiblingEffect()
        {
            _interactable.OnInteract(InteractionContext.Shot(_bullet.transform));

            Assert.That(_first.CallCount, Is.EqualTo(1));
            Assert.That(_second.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void OnInteract_FromTouch_AppliesNothing()
        {
            _interactable.OnInteract(InteractionContext.Touch(_bullet.transform));

            Assert.That(_first.CallCount, Is.EqualTo(0));
            Assert.That(_second.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void OnInteract_Twice_AppliesEffectsTwice()
        {
            _interactable.OnInteract(InteractionContext.Shot(_bullet.transform));
            _interactable.OnInteract(InteractionContext.Shot(_bullet.transform));

            Assert.That(_first.CallCount, Is.EqualTo(2));
            Assert.That(_second.CallCount, Is.EqualTo(2));
        }
    }
}
