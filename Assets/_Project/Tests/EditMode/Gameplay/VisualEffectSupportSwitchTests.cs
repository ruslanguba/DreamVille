using NUnit.Framework;
using UnityEditor;
using UnityEngine;

using DreamVille.Gameplay.Rendering;

namespace DreamVille.Tests.EditMode.Gameplay
{
    /// <summary>
    /// <see cref="VisualEffectSupportSwitch"/> shows the effects or their fallbacks, never both, and it may list its own
    /// GameObject among the effects (the effect prefabs switch themselves off).
    /// </summary>
    public class VisualEffectSupportSwitchTests
    {
        private GameObject _root;
        private GameObject _effect;
        private GameObject _fallback;
        private VisualEffectSupportSwitch _switch;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Projectile");
            _effect = new GameObject("Effect");
            _fallback = new GameObject("Fallback");
            _effect.transform.SetParent(_root.transform);
            _fallback.transform.SetParent(_root.transform);
            _fallback.SetActive(false);

            _switch = _root.AddComponent<VisualEffectSupportSwitch>();
            Assign(_switch, new[] { _effect }, new[] { _fallback });
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void Apply_WhenSupported_ShowsTheEffectsAndHidesTheFallbacks()
        {
            _switch.Apply(visualEffectGraphSupported: true);

            Assert.That(_effect.activeSelf, Is.True);
            Assert.That(_fallback.activeSelf, Is.False);
        }

        [Test]
        public void Apply_WhenUnsupported_HidesTheEffectsAndShowsTheFallbacks()
        {
            _switch.Apply(visualEffectGraphSupported: false);

            Assert.That(_effect.activeSelf, Is.False);
            Assert.That(_fallback.activeSelf, Is.True);
        }

        [Test]
        public void Apply_WhenUnsupported_CanSwitchOffItsOwnObject()
        {
            Assign(_switch, new[] { _root }, new GameObject[0]);

            _switch.Apply(visualEffectGraphSupported: false);

            Assert.That(_root.activeSelf, Is.False);
        }

        private static void Assign(VisualEffectSupportSwitch target, GameObject[] effects, GameObject[] fallbacks)
        {
            var serialized = new SerializedObject(target);
            Fill(serialized.FindProperty("_visualEffects"), effects);
            Fill(serialized.FindProperty("_fallbacks"), fallbacks);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Fill(SerializedProperty array, GameObject[] items)
        {
            array.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }
    }
}
