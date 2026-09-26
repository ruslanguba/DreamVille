using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

using DreamVille.Gameplay.Interactables;
using DreamVille.Gameplay.Interactables.Effects;
using DreamVille.Tests.Support;

namespace DreamVille.Tests.EditMode.Interactables
{
    /// <summary>The skin pickup effect reaches the player's skins through the interaction's instigator.</summary>
    public class ChangeSkinEffectTests
    {
        private const int SkinIndex = 3;

        private GameObject _player;
        private GameObject _collector;
        private GameObject _pickup;
        private ChangeSkinEffect _effect;

        [SetUp]
        public void SetUp()
        {
            _player = new GameObject("Player");
            _collector = new GameObject("Collector");
            _collector.transform.SetParent(_player.transform);
            _pickup = new GameObject("SkinPickup");
            _effect = _pickup.AddComponent<ChangeSkinEffect>();

            var serializedEffect = new SerializedObject(_effect);
            serializedEffect.FindProperty("_skinIndex").intValue = SkinIndex;
            serializedEffect.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_player);
            Object.DestroyImmediate(_pickup);
        }

        [Test]
        public void Apply_WithPlayerInstigator_SetsTheConfiguredSkin()
        {
            var skins = _player.AddComponent<FakePlayerSkinsBehaviour>();
            Assert.That(skins, Is.Not.Null);

            _effect.Apply(InteractionContext.Touch(_collector.transform));

            Assert.That(skins.CallCount, Is.EqualTo(1));
            Assert.That(skins.LastIndex, Is.EqualTo(SkinIndex));
        }

        [Test]
        public void Apply_WithoutSkins_LogsErrorAndDoesNothing()
        {
            LogAssert.Expect(LogType.Error, new Regex(nameof(ChangeSkinEffect)));

            _effect.Apply(InteractionContext.Touch(_collector.transform));

            LogAssert.NoUnexpectedReceived();
        }
    }
}
