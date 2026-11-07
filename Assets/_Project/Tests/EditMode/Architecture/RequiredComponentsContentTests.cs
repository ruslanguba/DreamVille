using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using DreamVille.Tests.EditMode.Composition;

namespace DreamVille.Tests.EditMode.Architecture
{
    /// <summary>
    /// [RequireComponent] acts only when a script is added, so a prefab or scene saved before the attribute existed can
    /// still lack what a script takes from its own GameObject. Every DreamVille script in every prefab and scene has the
    /// components its attributes require (spec 16 §5 items 3-4). The scene scan runs once in the fixture setup and keeps
    /// plain strings, because opening the next scene unloads the previous one.
    /// </summary>
    public class RequiredComponentsContentTests
    {
        private const string ProjectFolder = "Assets/_Project";

        private string _originalScenePath;
        private bool _scenesOpened;
        private List<string> _sceneOffenders;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Scene activeScene = EditorSceneManager.GetActiveScene();
            _originalScenePath = activeScene.path;

            // Interactively a dirty scene just skips the fixture; on a build machine it is a real failure.
            const string dirtySceneMessage = "A dirty scene is open; save or discard changes before running this fixture.";
            if (Application.isBatchMode)
                Assert.That(activeScene.isDirty, Is.False, dirtySceneMessage);
            else
                Assume.That(!activeScene.isDirty, dirtySceneMessage);

            _sceneOffenders = new List<string>();
            _scenesOpened = true;

            SceneContentScan.ForEach<MonoBehaviour>((scene, path, behaviour) =>
                _sceneOffenders.AddRange(MissingRequirements(behaviour).Select(required => Describe(scene, path, behaviour, required))));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            // Never reopen a scene when the setup was skipped: that would discard the user's unsaved changes.
            if (!_scenesOpened)
                return;

            if (string.IsNullOrEmpty(_originalScenePath))
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            else
                EditorSceneManager.OpenScene(_originalScenePath, OpenSceneMode.Single);
        }

        [Test]
        public void EveryPrefab_HasTheComponentsItsScriptsRequire()
        {
            var offenders = new List<string>();

            foreach (string prefabPath in SceneContentScan.AssetPaths("t:Prefab", ProjectFolder))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                    continue;

                foreach (MonoBehaviour behaviour in prefab.GetComponentsInChildren<MonoBehaviour>(true).Where(behaviour => behaviour != null))
                {
                    string path = SceneContentScan.HierarchyPathOf(behaviour.transform);
                    offenders.AddRange(MissingRequirements(behaviour).Select(required => Describe(prefabPath, path, behaviour, required)));
                }
            }

            Assert.That(offenders, Is.Empty,
                "Scripts in prefabs miss components their [RequireComponent] asks for (add them to the prefab):\n" + string.Join("\n", offenders));
        }

        [Test]
        public void EveryScene_HasTheComponentsItsScriptsRequire()
        {
            Assert.That(_sceneOffenders, Is.Empty,
                "Scripts in scenes miss components their [RequireComponent] asks for (add them to the object or its prefab):\n" +
                string.Join("\n", _sceneOffenders));
        }

        private static IEnumerable<Type> MissingRequirements(MonoBehaviour behaviour)
        {
            Type type = behaviour.GetType();
            if (type.Namespace == null || !type.Namespace.StartsWith("DreamVille", StringComparison.Ordinal))
                return Enumerable.Empty<Type>();

            return type.GetCustomAttributes(typeof(RequireComponent), true)
                .Cast<RequireComponent>()
                .SelectMany(attribute => new[] { attribute.m_Type0, attribute.m_Type1, attribute.m_Type2 })
                .Where(required => required != null && behaviour.GetComponent(required) == null)
                .ToArray();
        }

        private static string Describe(string asset, string path, MonoBehaviour behaviour, Type required)
            => $"{asset}: {path} -> {behaviour.GetType().Name} needs {required.Name}";
    }
}
