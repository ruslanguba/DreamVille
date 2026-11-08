using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using DreamVille.Configs;
using DreamVille.Progression.Pillows;

namespace DreamVille.Tests.EditMode.Progression
{
    /// <summary>
    /// Content guards: pins the shipped per-level pillow caps and the single
    /// Lobby_02 dialogue grant, so a designer edit cannot silently reopen an early level to
    /// pillows or duplicate the unlock source into another scene.
    /// </summary>
    public class PillowUnlockContentTests
    {
        private const string CatalogAssetPath = "Assets/_Project/Settings/Levels/SceneCatalog.asset";

        private static readonly SceneCatalog Catalog = AssetDatabase.LoadAssetAtPath<SceneCatalog>(CatalogAssetPath);

        private static readonly (string SceneName, int MaxPillows)[] ExpectedCaps =
        {
            ("Lobby_01", 0),
            ("Level_01", 0),
            ("Level_02", 0),
            ("Lobby_02", 2),
            ("Level_03", 2),
            ("Level_04", 2)
        };

        private string _originalScenePath;
        private bool _scenesOpened;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Assume.That(Catalog, Is.Not.Null, $"{CatalogAssetPath} is missing.");

            Scene activeScene = EditorSceneManager.GetActiveScene();
            _originalScenePath = activeScene.path;

            const string dirtySceneMessage = "A dirty scene is open; save or discard changes before running this fixture.";
            if (Application.isBatchMode)
                Assert.That(activeScene.isDirty, Is.False, dirtySceneMessage);
            else
                Assume.That(!activeScene.isDirty, dirtySceneMessage);

            _scenesOpened = true;
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

        private static IEnumerable<string> EnabledScenePaths()
            => EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path);

        private static IEnumerable<string> GameplayScenePathsExceptLobby02()
            => EnabledScenePaths()
                .Where(path => Path.GetFileNameWithoutExtension(path) != Catalog.BootstrapScene.SceneName
                    && Path.GetFileNameWithoutExtension(path) != Catalog.MainMenuScene.SceneName
                    && Path.GetFileNameWithoutExtension(path) != "Lobby_02");

        private static void OpenScene(string scenePath) => EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        [Test]
        public void LevelDefinitions_MaxPillowsMatchDesign()
        {
            foreach ((string sceneName, int maxPillows) in ExpectedCaps)
            {
                LevelDefinition level = Catalog.Levels.FirstOrDefault(l => l != null && l.SceneName == sceneName);

                Assert.That(level, Is.Not.Null, $"SceneCatalog has no level for scene '{sceneName}'.");
                Assert.That(level.MaxPillows, Is.EqualTo(maxPillows), $"{sceneName}.MaxPillows must be {maxPillows}.");
            }
        }

        [Test]
        public void Lobby02_HasOnePillowRewardOnDialogue_ForPillowTwo_WithTrigger()
        {
            string lobby02Path = EnabledScenePaths().Single(path => Path.GetFileNameWithoutExtension(path) == "Lobby_02");
            OpenScene(lobby02Path);

            PillowRewardOnDialogue[] rewards = UnityEngine.Object.FindObjectsByType<PillowRewardOnDialogue>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            Assert.That(rewards, Has.Length.EqualTo(1), "Lobby_02 must have exactly one PillowRewardOnDialogue.");

            var serializedReward = new SerializedObject(rewards[0]);
            SerializedProperty dialogue = serializedReward.FindProperty("_dialogue");
            SerializedProperty pillowNumber = serializedReward.FindProperty("_pillowNumber");

            Assert.That(dialogue.objectReferenceValue, Is.Not.Null, "PillowRewardOnDialogue._dialogue is not assigned.");
            Assert.That(pillowNumber.intValue, Is.EqualTo(2), "PillowRewardOnDialogue._pillowNumber must grant both pillows (2).");
        }

        [TestCaseSource(nameof(GameplayScenePathsExceptLobby02))]
        public void NoGameplayScene_OtherThanLobby02_GrantsPillows(string scenePath)
        {
            OpenScene(scenePath);

            PillowRewardOnDialogue[] rewards = UnityEngine.Object.FindObjectsByType<PillowRewardOnDialogue>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            Assert.That(rewards, Is.Empty, $"{scenePath} must not have a PillowRewardOnDialogue; only Lobby_02 grants pillows.");
        }
    }
}
