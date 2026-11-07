using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Zenject;

using DreamVille.Gameplay.Interactables;
using DreamVille.Scoring;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Interactables
{
    /// <summary>
    /// The scoring path of <see cref="Pickup"/>: only the player's touch collects it, and only once.
    /// Awake does not run in EditMode, so this also pins that the pickup works before it.
    /// </summary>
    public class PickupTests
    {
        private const float ScoreToAdd = 5f;

        private GameObject _host;
        private GameObject _collector;
        private Collider _collider;
        private FakeScoreService _score;
        private Pickup _pickup;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("Pickup");
            _collider = _host.AddComponent<BoxCollider>();
            _pickup = _host.AddComponent<Pickup>();
            _collector = new GameObject("Collector");

            var serializedPickup = new SerializedObject(_pickup);
            serializedPickup.FindProperty("_scoreToAdd").floatValue = ScoreToAdd;
            serializedPickup.ApplyModifiedPropertiesWithoutUndo();

            _score = new FakeScoreService();
            var container = new DiContainer();
            container.Bind<IScoreService>().FromInstance(_score);
            container.Inject(_pickup);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
            Object.DestroyImmediate(_collector);
        }

        [Test]
        public void OnInteract_TouchedTwice_AddsScoreOnce()
        {
            _pickup.OnInteract(InteractionContext.Touch(_collector.transform));
            _pickup.OnInteract(InteractionContext.Touch(_collector.transform));

            Assert.That(_score.TotalScore, Is.EqualTo(ScoreToAdd));
            Assert.That(_collider.enabled, Is.False);
        }

        [Test]
        public void OnInteract_FromShot_DoesNothing()
        {
            _pickup.OnInteract(InteractionContext.Shot(_collector.transform));

            Assert.That(_score.TotalScore, Is.EqualTo(0f));
            Assert.That(_collider.enabled, Is.True);
        }
    }
}
