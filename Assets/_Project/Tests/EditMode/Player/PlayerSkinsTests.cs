using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Zenject;

using DreamVille.Gameplay.Player.Animation;
using DreamVille.Gameplay.Player.Skins;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Player
{
    /// <summary>The skin swap activates exactly one skin and re-targets the player animator at it.</summary>
    public class PlayerSkinsTests
    {
        private const int SkinCount = 3;

        private GameObject _root;
        private Animator[] _skins;
        private PlayerSkins _playerSkins;
        private FakePlayerAnimator _animator;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Player");
            _skins = new Animator[SkinCount];
            for (int i = 0; i < SkinCount; i++)
            {
                var skin = new GameObject($"Skin {i}");
                skin.transform.SetParent(_root.transform);
                _skins[i] = skin.AddComponent<Animator>();
            }

            _playerSkins = _root.AddComponent<PlayerSkins>();
            var serializedSkins = new SerializedObject(_playerSkins);
            SerializedProperty skinsProperty = serializedSkins.FindProperty("_skins");
            skinsProperty.arraySize = SkinCount;
            for (int i = 0; i < SkinCount; i++)
                skinsProperty.GetArrayElementAtIndex(i).objectReferenceValue = _skins[i];
            serializedSkins.ApplyModifiedPropertiesWithoutUndo();

            _animator = new FakePlayerAnimator();
            var container = new DiContainer();
            container.Bind<IPlayerAnimator>().FromInstance(_animator);
            container.Inject(_playerSkins);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void SetSkin_ValidIndex_ActivatesOnlyThatSkinAndRetargetsTheAnimator()
        {
            _playerSkins.SetSkin(2);

            Assert.That(_skins[0].gameObject.activeSelf, Is.False);
            Assert.That(_skins[1].gameObject.activeSelf, Is.False);
            Assert.That(_skins[2].gameObject.activeSelf, Is.True);
            Assert.That(_animator.LastAnimator, Is.SameAs(_skins[2]));
        }

        [Test]
        public void SetSkin_OutOfRange_KeepsTheCurrentSkinAndLogsError()
        {
            LogAssert.Expect(LogType.Error, new Regex("out of range"));

            _playerSkins.SetSkin(7);

            foreach (Animator skin in _skins)
                Assert.That(skin.gameObject.activeSelf, Is.True);
            Assert.That(_animator.LastAnimator, Is.Null);
        }
    }
}
