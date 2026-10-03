using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DreamVille.Tests.EditMode.Composition
{
    /// <summary>
    /// Deleting a script leaves "missing script" components behind in every prefab that used it. A GUID scan before
    /// a deletion is the first guard; this test is the net under it.
    /// </summary>
    public class PrefabIntegrityTests
    {
        private static IEnumerable<string> ProjectPrefabPaths()
            => AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path);

        [Test]
        public void EveryProjectPrefab_HasNoMissingScripts()
        {
            var broken = new List<string>();

            foreach (string path in ProjectPrefabPaths())
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                foreach (Transform transform in prefab.GetComponentsInChildren<Transform>(true))
                {
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) > 0)
                        broken.Add($"{path}: {transform.name}");
                }
            }

            Assert.That(broken, Is.Empty, "Missing scripts:\n" + string.Join("\n", broken));
        }
    }
}
