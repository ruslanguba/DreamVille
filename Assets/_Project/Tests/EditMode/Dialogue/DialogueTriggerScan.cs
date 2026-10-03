using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

using DreamVille.Dialogue;

namespace DreamVille.Tests.EditMode.Dialogue
{
    /// <summary>
    /// Visits every <see cref="DialogueTrigger"/> that can ship: each scene under <c>Assets/_Project/Scenes</c>
    /// (build scenes and the WIP scenes alike) and each prefab asset under <c>Assets/_Project</c>.
    /// Scenes are opened one at a time (Single), so a visitor must copy what it needs into plain data
    /// before returning: Unity objects loaded for one scene are not safe to keep once the next one opens.
    /// </summary>
    internal static class DialogueTriggerScan
    {
        /// <summary>The source name used for triggers that live in prefab assets rather than in a scene.</summary>
        public const string PrefabSource = "Prefabs";

        private const string ScenesFolder = "Assets/_Project/Scenes";
        private const string ProjectFolder = "Assets/_Project";

        public delegate void TriggerVisitor(string source, string triggerPath, DialogueTrigger trigger);

        public static void ForEachTrigger(TriggerVisitor visit)
        {
            foreach (string scenePath in AssetPaths("t:Scene", ScenesFolder))
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                string source = Path.GetFileNameWithoutExtension(scenePath);

                foreach (DialogueTrigger trigger in UnityEngine.Object.FindObjectsByType<DialogueTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    visit(source, HierarchyPathOf(trigger.transform), trigger);
            }

            foreach (string prefabPath in AssetPaths("t:Prefab", ProjectFolder))
            {
                var root = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                foreach (DialogueTrigger trigger in root.GetComponentsInChildren<DialogueTrigger>(true))
                    visit(PrefabSource, HierarchyPathOf(trigger.transform), trigger);
            }
        }

        private static string[] AssetPaths(string filter, string folder)
            => AssetDatabase.FindAssets(filter, new[] { folder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();

        private static string HierarchyPathOf(Transform transform)
        {
            string path = transform.name;

            for (Transform parent = transform.parent; parent != null; parent = parent.parent)
                path = parent.name + "/" + path;

            return path;
        }
    }
}
