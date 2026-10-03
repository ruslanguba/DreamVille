using UnityEditor;
using UnityEngine;

using DreamVille.Dialogue;
using DreamVille.Tests.EditMode.Composition;

namespace DreamVille.Tests.EditMode.Dialogue
{
    /// <summary>
    /// Visits every <see cref="DialogueTrigger"/> that can ship: each scene under <c>Assets/_Project/Scenes</c>
    /// (through <see cref="SceneContentScan"/>, which also explains why a visitor must copy plain data)
    /// and each prefab asset under <c>Assets/_Project</c>.
    /// </summary>
    internal static class DialogueTriggerScan
    {
        /// <summary>The source name used for triggers that live in prefab assets rather than in a scene.</summary>
        public const string PrefabSource = "Prefabs";

        private const string ProjectFolder = "Assets/_Project";

        public delegate void TriggerVisitor(string source, string triggerPath, DialogueTrigger trigger);

        public static void ForEachTrigger(TriggerVisitor visit)
        {
            SceneContentScan.ForEach<DialogueTrigger>((scene, triggerPath, trigger) => visit(scene, triggerPath, trigger));

            foreach (string prefabPath in SceneContentScan.AssetPaths("t:Prefab", ProjectFolder))
            {
                var root = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                foreach (DialogueTrigger trigger in root.GetComponentsInChildren<DialogueTrigger>(true))
                    visit(PrefabSource, SceneContentScan.HierarchyPathOf(trigger.transform), trigger);
            }
        }
    }
}
