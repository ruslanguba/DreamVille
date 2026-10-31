using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using DreamVille.Tests.EditMode.Composition;

namespace DreamVille.Tests.EditMode.Architecture
{
    /// <summary>
    /// Guards spec 14 §5 items 14-15: once a serialized field has been renamed and its data migrated, no asset keeps
    /// data under the old name, neither in a component's own YAML nor as a prefab-instance override, and no field takes
    /// a retired name again (Unity would quietly load the stale data into it). Overrides on a ledger owner that point
    /// at a field which no longer exists count as well: they are dead data the editor can no longer show (the few pairs in
    /// <see cref="RetiredSerializedFields.KnownDeadOverrides"/> are tolerated until they are dropped, and must still exist).
    /// The scenes and prefabs are scanned once in the fixture setup.
    /// </summary>
    public class SerializedDataHygieneTests
    {
        private const string ProjectFolder = "Assets/_Project";
        private const string ScenesFolder = "Assets/_Project/Scenes";
        private const string DocumentSeparator = "\n--- !u!";

        private static readonly string[] YamlAssetExtensions = { ".unity", ".prefab", ".asset" };

        private static List<KeyValuePair<string, string>> _assetTexts;

        private readonly Dictionary<string, bool> _fieldExists = new Dictionary<string, bool>(StringComparer.Ordinal);

        private string _originalScenePath;
        private bool _scenesOpened;
        private List<string> _overrideOffenders;
        private HashSet<string> _knownDeadOverridesSeen;

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

            Dictionary<Type, HashSet<string>> retiredByOwner = RetiredSerializedFields.All
                .ToDictionary(entry => entry.Owner, entry => new HashSet<string>(entry.OldNames, StringComparer.Ordinal));

            _overrideOffenders = new List<string>();
            _knownDeadOverridesSeen = new HashSet<string>(StringComparer.Ordinal);

            // Prefabs before scenes: opening a scene Single unloads assets that nothing references any more.
            foreach (string prefabPath in SceneContentScan.AssetPaths("t:Prefab", ProjectFolder))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab != null)
                    CollectOverrideOffenders(prefabPath, new[] { prefab }, isScene: false, retiredByOwner);
            }

            _scenesOpened = true;

            foreach (string scenePath in SceneContentScan.AssetPaths("t:Scene", ScenesFolder))
            {
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                CollectOverrideOffenders(scenePath, scene.GetRootGameObjects(), isScene: true, retiredByOwner);
            }
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

        private static IEnumerable<Type> LedgerOwners() => RetiredSerializedFields.All.Select(entry => entry.Owner);

        [TestCaseSource(nameof(LedgerOwners))]
        public void RetiredSerializedFields_AreAbsentFromAssetData(Type owner)
        {
            string[] retired = RetiredSerializedFields.All.Single(entry => entry.Owner == owner).OldNames;
            var offenders = new List<string>();

            foreach (string name in retired)
            {
                if (DeclaresInstanceField(owner, name))
                    offenders.Add($"{owner.Name}.{name} is a field again; a retired name must never be reused");
            }

            string guid = ScriptGuidOf(owner);
            Assert.That(guid, Is.Not.Null, $"No MonoScript asset resolves to {owner.FullName}.");

            string scriptReference = "m_Script: {fileID: 11500000, guid: " + guid + ", type: 3}";

            foreach (KeyValuePair<string, string> asset in AssetTexts())
            {
                if (!asset.Value.Contains(guid))
                    continue;

                foreach (string document in asset.Value.Split(new[] { DocumentSeparator }, StringSplitOptions.None))
                {
                    if (!document.Contains(scriptReference))
                        continue;

                    // Fields of a serialized object are its keys at two spaces of indentation.
                    foreach (string name in retired)
                    {
                        if (document.Contains("\n  " + name + ":"))
                            offenders.Add($"{asset.Key}: {owner.Name} data is still stored under '{name}'");
                    }
                }
            }

            Assert.That(offenders, Is.Empty,
                $"{owner.Name} keeps data under retired serialized names (re-serialize the asset after the rename):\n" + string.Join("\n", offenders));
        }

        [Test]
        public void RetiredSerializedFields_HaveNoInstanceOverrides()
        {
            string[] staleExceptions = RetiredSerializedFields.KnownDeadOverrides
                .Select(entry => KnownDeadOverrideKey(entry.Owner, entry.Field))
                .Where(key => !_knownDeadOverridesSeen.Contains(key))
                .ToArray();

            Assert.That(_overrideOffenders, Is.Empty,
                "Prefab-instance overrides that address a retired or missing field of a renamed type (rewrite or drop them):\n" +
                string.Join("\n", _overrideOffenders));
            Assert.That(staleExceptions, Is.Empty,
                "Known dead overrides that no asset holds any more (drop the entry from RetiredSerializedFields.KnownDeadOverrides):\n" +
                string.Join("\n", staleExceptions));
        }

        private void CollectOverrideOffenders(string assetPath, IEnumerable<GameObject> roots, bool isScene, Dictionary<Type, HashSet<string>> retiredByOwner)
        {
            foreach (GameObject root in roots)
            {
                foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
                {
                    GameObject instanceRoot = transform.gameObject;
                    if (!IsInstanceRoot(instanceRoot, isScene))
                        continue;

                    PropertyModification[] modifications = PrefabUtility.GetPropertyModifications(instanceRoot);
                    if (modifications == null)
                        continue;

                    foreach (PropertyModification modification in modifications)
                    {
                        if (modification.target == null || !retiredByOwner.TryGetValue(modification.target.GetType(), out HashSet<string> retired))
                            continue;

                        string head = HeadOf(modification.propertyPath);
                        if (head.StartsWith("m_", StringComparison.Ordinal))
                            continue;

                        string where = $"{assetPath}: {SceneContentScan.HierarchyPathOf(instanceRoot.transform)} -> {modification.target.GetType().Name}.{head}";

                        if (retired.Contains(head))
                        {
                            _overrideOffenders.Add(where + " (retired name)");
                        }
                        else if (!FieldExists(modification.target, head))
                        {
                            string key = KnownDeadOverrideKey(modification.target.GetType(), head);
                            if (IsKnownDeadOverride(key))
                                _knownDeadOverridesSeen.Add(key);
                            else
                                _overrideOffenders.Add(where + " (no such field)");
                        }
                    }
                }
            }
        }

        // A prefab variant's root holds the overrides of its base prefab, like an instance root in a scene does.
        private static bool IsInstanceRoot(GameObject gameObject, bool isScene)
            => PrefabUtility.IsOutermostPrefabInstanceRoot(gameObject)
               || (!isScene && gameObject.transform.parent == null && PrefabUtility.IsPartOfVariantPrefab(gameObject));

        private bool FieldExists(UnityEngine.Object target, string head)
        {
            string key = target.GetType().FullName + "." + head;
            if (_fieldExists.TryGetValue(key, out bool exists))
                return exists;

            using (var serializedTarget = new SerializedObject(target))
                exists = serializedTarget.FindProperty(head) != null;

            _fieldExists[key] = exists;
            return exists;
        }

        private static string KnownDeadOverrideKey(Type owner, string field) => owner.FullName + "." + field;

        private static bool IsKnownDeadOverride(string key)
            => RetiredSerializedFields.KnownDeadOverrides.Any(entry => KnownDeadOverrideKey(entry.Owner, entry.Field) == key);

        private static string HeadOf(string propertyPath)
        {
            int dot = propertyPath.IndexOf('.');
            return dot < 0 ? propertyPath : propertyPath.Substring(0, dot);
        }

        private static bool DeclaresInstanceField(Type owner, string name)
        {
            const BindingFlags InstanceFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            for (Type type = owner; type != null && type != typeof(MonoBehaviour) && type != typeof(ScriptableObject); type = type.BaseType)
            {
                if (type.GetField(name, InstanceFields) != null)
                    return true;
            }

            return false;
        }

        private static string ScriptGuidOf(Type owner)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:MonoScript " + owner.Name))
            {
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));
                if (script != null && script.GetClass() == owner)
                    return guid;
            }

            return null;
        }

        /// <summary>Every scene, prefab and asset file under <c>Assets</c>, read once per test run.</summary>
        private static List<KeyValuePair<string, string>> AssetTexts()
        {
            if (_assetTexts != null)
                return _assetTexts;

            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            _assetTexts = Directory.EnumerateFiles(Application.dataPath, "*", SearchOption.AllDirectories)
                .Where(path => YamlAssetExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
                .Select(path => new KeyValuePair<string, string>(
                    path.Substring(projectRoot.Length + 1).Replace('\\', '/'),
                    File.ReadAllText(path)))
                .ToList();

            return _assetTexts;
        }
    }
}
