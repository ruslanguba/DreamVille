using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DreamVille.Tests.EditMode.Composition
{
    /// <summary>
    /// Visits every component of one type in every scene that can ship: all scenes under
    /// <c>Assets/_Project/Scenes</c>, build scenes and the WIP scenes alike.
    /// Scenes are opened one at a time (Single), so a visitor must copy what it needs into plain data
    /// before returning: Unity objects loaded for one scene are not safe to keep once the next one opens.
    /// </summary>
    internal static class SceneContentScan
    {
        private const string ScenesFolder = "Assets/_Project/Scenes";

        /// <param name="scene">Scene name without folder and extension.</param>
        /// <param name="path">Hierarchy path of the component's GameObject from the scene root.</param>
        /// <param name="component">Valid only until the visitor returns.</param>
        public delegate void Visitor<T>(string scene, string path, T component) where T : Component;

        /// <summary>Calls <paramref name="visit"/> for each <typeparamref name="T"/>, inactive objects included.</summary>
        public static void ForEach<T>(Visitor<T> visit) where T : Component
        {
            foreach (string scenePath in AssetPaths("t:Scene", ScenesFolder))
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                string scene = Path.GetFileNameWithoutExtension(scenePath);

                foreach (T component in UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    visit(scene, HierarchyPathOf(component.transform), component);
            }
        }

        /// <summary>Asset paths under <paramref name="folder"/> matching an AssetDatabase <paramref name="filter"/>, in a stable order.</summary>
        public static string[] AssetPaths(string filter, string folder)
            => AssetDatabase.FindAssets(filter, new[] { folder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();

        public static string HierarchyPathOf(Transform transform)
        {
            string path = transform.name;

            for (Transform parent = transform.parent; parent != null; parent = parent.parent)
                path = parent.name + "/" + path;

            return path;
        }
    }
}
