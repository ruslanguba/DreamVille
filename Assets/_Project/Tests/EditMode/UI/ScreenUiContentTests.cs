using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using DreamVille.Tests.EditMode.Composition;

namespace DreamVille.Tests.EditMode.UI
{
    /// <summary>
    /// The UI must work on any screen and in a browser, not only in the editor at 1920×1080.
    /// <list type="bullet">
    /// <item>The UI is laid out at 1920×1080. A screen-space canvas with a constant pixel size only looks right at that
    /// resolution: in a smaller window (a browser canvas, a windowed build) its buttons run off the screen. Every root
    /// screen-space canvas therefore scales with the screen from a 1920×1080 reference. World-space canvases are sized in
    /// the world and are not checked.</item>
    /// <item>A legacy UI Text with Unity's built-in font borrows missing glyphs from the operating system's fonts.
    /// A WebGL build has no system fonts, so its Cyrillic text disappears. Every legacy Text uses a font from the project
    /// whose data ships with the build.</item>
    /// </list>
    /// The build scenes and the prefabs are checked; the scenes are scanned once in the fixture setup.
    /// </summary>
    public class ScreenUiContentTests
    {
        private const string PrefabsFolder = "Assets/_Project/Prefabs";
        private const string ProjectAssetsPrefix = "Assets/";
        private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

        private string _originalScenePath;
        private bool _scenesOpened;
        private List<string> _sceneCanvasOffenders;
        private List<string> _sceneFontOffenders;

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

            _sceneCanvasOffenders = new List<string>();
            _sceneFontOffenders = new List<string>();
            _scenesOpened = true;

            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes.Where(scene => scene.enabled))
            {
                Scene scene = EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    _sceneCanvasOffenders.AddRange(CanvasOffenders(buildScene.path, root));
                    _sceneFontOffenders.AddRange(FontOffenders(buildScene.path, root));
                }
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

        [Test]
        public void BuildScenes_ScreenSpaceCanvases_ScaleWithTheScreen()
        {
            Assert.That(_sceneCanvasOffenders, Is.Empty,
                "Screen-space canvases that do not scale from 1920x1080 (Canvas Scaler: Scale With Screen Size):\n" + string.Join("\n", _sceneCanvasOffenders));
        }

        [Test]
        public void Prefabs_ScreenSpaceCanvases_ScaleWithTheScreen()
        {
            List<string> offenders = Prefabs().SelectMany(prefab => CanvasOffenders(prefab.Key, prefab.Value)).ToList();

            Assert.That(offenders, Is.Empty,
                "Screen-space canvases that do not scale from 1920x1080 (Canvas Scaler: Scale With Screen Size):\n" + string.Join("\n", offenders));
        }

        [Test]
        public void BuildScenes_LegacyTexts_UseAProjectFontWithItsData()
        {
            Assert.That(_sceneFontOffenders, Is.Empty,
                "Legacy UI Texts whose font does not ship with the build (use a project font with Include Font Data):\n" + string.Join("\n", _sceneFontOffenders));
        }

        [Test]
        public void Prefabs_LegacyTexts_UseAProjectFontWithItsData()
        {
            List<string> offenders = Prefabs().SelectMany(prefab => FontOffenders(prefab.Key, prefab.Value)).ToList();

            Assert.That(offenders, Is.Empty,
                "Legacy UI Texts whose font does not ship with the build (use a project font with Include Font Data):\n" + string.Join("\n", offenders));
        }

        private static IEnumerable<KeyValuePair<string, GameObject>> Prefabs()
            => SceneContentScan.AssetPaths("t:Prefab", PrefabsFolder)
                .Select(path => new KeyValuePair<string, GameObject>(path, AssetDatabase.LoadAssetAtPath<GameObject>(path)))
                .Where(prefab => prefab.Value != null);

        private static IEnumerable<string> CanvasOffenders(string assetPath, GameObject root)
        {
            foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(true))
            {
                bool isNested = canvas.transform.parent != null && canvas.transform.parent.GetComponentInParent<Canvas>(true) != null;
                if (isNested || canvas.renderMode == RenderMode.WorldSpace)
                    continue;

                var scaler = canvas.GetComponent<CanvasScaler>();
                bool scales = scaler != null
                              && scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize
                              && scaler.referenceResolution == ReferenceResolution;

                if (!scales)
                    yield return $"{assetPath}: {SceneContentScan.HierarchyPathOf(canvas.transform)} " +
                                 $"({(scaler == null ? "no Canvas Scaler" : scaler.uiScaleMode + " " + scaler.referenceResolution)})";
            }
        }

        private static IEnumerable<string> FontOffenders(string assetPath, GameObject root)
        {
            foreach (Text text in root.GetComponentsInChildren<Text>(true))
            {
                string fontPath = text.font == null ? null : AssetDatabase.GetAssetPath(text.font);
                bool isProjectFont = fontPath != null && fontPath.StartsWith(ProjectAssetsPrefix, System.StringComparison.Ordinal);
                bool shipsItsData = isProjectFont && AssetImporter.GetAtPath(fontPath) is TrueTypeFontImporter importer && importer.includeFontData;

                if (!shipsItsData)
                    yield return $"{assetPath}: {SceneContentScan.HierarchyPathOf(text.transform)} " +
                                 $"(font: {(text.font == null ? "none" : text.font.name + " at " + fontPath)})";
            }
        }
    }
}
