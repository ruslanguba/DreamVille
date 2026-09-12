using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

using DreamVille.Bootstrap;
using DreamVille.Configs;
using DreamVille.Dialogue;
using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Player;
using DreamVille.Gameplay.Weapons.Pillow;
using DreamVille.Input;
using DreamVille.SceneManagement;
using DreamVille.UI;

namespace DreamVille.Tests.EditMode.Composition
{
    /// <summary>
    /// Guards spec 01 §3.7/§3.8 and spec 02 §3.2: opens every enabled build scene and checks its
    /// composition root instead of trusting a GUID scan of the scene file, because
    /// installers reach into nested prefabs (conventions §9, roadmap step 3.7). Scene roles and
    /// level identity come from the shipped SceneCatalog asset, never from a scene name literal.
    /// </summary>
    public class SceneCompositionTests
    {
        private const string CatalogAssetPath = "Assets/_Project/Settings/Levels/SceneCatalog.asset";

        private static readonly SceneCatalog Catalog = AssetDatabase.LoadAssetAtPath<SceneCatalog>(CatalogAssetPath);

        private enum SceneRole
        {
            Bootstrap,
            Menu,
            Gameplay
        }

        private string _originalScenePath;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Assume.That(Catalog, Is.Not.Null, $"{CatalogAssetPath} is missing.");

            Scene activeScene = EditorSceneManager.GetActiveScene();
            _originalScenePath = activeScene.path;

            // Interactively a dirty scene just skips the fixture; on a build machine it is a real failure.
            const string dirtySceneMessage = "A dirty scene is open; save or discard changes before running this fixture.";
            if (Application.isBatchMode)
                Assert.That(activeScene.isDirty, Is.False, dirtySceneMessage);
            else
                Assume.That(!activeScene.isDirty, dirtySceneMessage);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (string.IsNullOrEmpty(_originalScenePath))
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            else
                EditorSceneManager.OpenScene(_originalScenePath, OpenSceneMode.Single);
        }

        private static IEnumerable<string> EnabledScenePaths()
            => EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path);

        private static IEnumerable<string> GameplayScenePaths()
            => EnabledScenePaths().Where(path => Classify(path) == SceneRole.Gameplay);

        private static SceneRole Classify(string scenePath)
        {
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);

            if (sceneName == Catalog.BootstrapScene.SceneName)
                return SceneRole.Bootstrap;
            if (sceneName == Catalog.MainMenuScene.SceneName)
                return SceneRole.Menu;

            return SceneRole.Gameplay;
        }

        private static void OpenScene(string scenePath) => EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        private static T[] FindAll<T>() where T : UnityEngine.Object
            => UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        private static T FindSingle<T>() where T : UnityEngine.Object
        {
            T[] found = FindAll<T>();

            Assert.That(found, Has.Length.EqualTo(1), $"Expected exactly one {typeof(T).Name} in the loaded scene, found {found.Length}.");

            return found[0];
        }

        private static Type[] InstalledInstallerTypes(SceneContext sceneContext)
        {
            var serializedContext = new SerializedObject(sceneContext);
            SerializedProperty installers = serializedContext.FindProperty("_monoInstallers");
            var types = new Type[installers.arraySize];

            for (int i = 0; i < installers.arraySize; i++)
            {
                UnityEngine.Object installer = installers.GetArrayElementAtIndex(i).objectReferenceValue;
                types[i] = installer != null ? installer.GetType() : null;
            }

            return types;
        }

        private static void AssertPrefabFieldAssigned(UnityEngine.Object installer, string fieldName)
        {
            var serializedInstaller = new SerializedObject(installer);
            SerializedProperty field = serializedInstaller.FindProperty(fieldName);

            Assert.That(field, Is.Not.Null, $"{installer.GetType().Name} has no serialized field named {fieldName}.");
            Assert.That(field.objectReferenceValue, Is.Not.Null, $"{installer.GetType().Name}.{fieldName} is not assigned.");
        }

        /// <summary>Avoids referencing DreamVille.Progression from the test assembly for one
        /// optional component; see spec 01 §3.2 (Tests.EditMode reference list).</summary>
        private static bool HasLevelActors()
            => FindAll<Component>().Any(component => component.GetType().FullName == "DreamVille.Progression.LevelActors");

        [TestCaseSource(nameof(EnabledScenePaths))]
        public void EveryEnabledScene_HasExactlyOneSceneContext(string scenePath)
        {
            OpenScene(scenePath);

            SceneContext[] contexts = FindAll<SceneContext>();

            Assert.That(contexts, Has.Length.EqualTo(1), $"{scenePath} must install exactly one SceneContext.");
        }

        [TestCaseSource(nameof(GameplayScenePaths))]
        public void GameplayScene_InstallsTheFiveRequiredInstallers(string scenePath)
        {
            OpenScene(scenePath);
            SceneContext sceneContext = FindSingle<SceneContext>();

            Type[] installerTypes = InstalledInstallerTypes(sceneContext);

            Assert.That(installerTypes, Is.EquivalentTo(new[]
            {
                typeof(SceneServicesInstaller),
                typeof(CombatInstaller),
                typeof(GameplayInstaller),
                typeof(ProgressionInstaller),
                typeof(UiInstaller)
            }));
        }

        [TestCaseSource(nameof(GameplayScenePaths))]
        public void GameplayScene_HasEveryFromComponentInHierarchyTarget(string scenePath)
        {
            OpenScene(scenePath);

            Assert.That(FindAll<PlayerContext>(), Is.Not.Empty, "PlayerContext is missing.");
            Assert.That(FindAll<PlayerInputSwitch>(), Is.Not.Empty, "PlayerInputSwitch is missing.");
            Assert.That(FindAll<PillowThrowHandler>(), Is.Not.Empty, "PillowThrowHandler is missing.");
            Assert.That(FindAll<EnemyRegistry>(), Is.Not.Empty, "EnemyRegistry is missing.");
            Assert.That(FindAll<DialogueManager>(), Is.Not.Empty, "DialogueManager is missing.");
            Assert.That(FindAll<GameInput>(), Is.Not.Empty, "GameInput is missing.");

            if (!HasLevelActors())
                TestContext.WriteLine($"{scenePath}: no LevelActors (optional; legitimately absent from Lobby_02).");
        }

        [TestCaseSource(nameof(GameplayScenePaths))]
        public void GameplayScene_InstallerPrefabFieldsAreAssigned(string scenePath)
        {
            OpenScene(scenePath);

            AssertPrefabFieldAssigned(FindSingle<UiInstaller>(), "_uiPrefab");
            AssertPrefabFieldAssigned(FindSingle<CombatInstaller>(), "_gameplaySystemsPrefab");
            AssertPrefabFieldAssigned(FindSingle<CombatInstaller>(), "_enemyProjectilePrefab");
            AssertPrefabFieldAssigned(FindSingle<ProgressionInstaller>(), "_catalog");
            AssertPrefabFieldAssigned(FindSingle<ProgressionInstaller>(), "_scoreRules");
        }

        [Test]
        public void BootstrapScene_InstallsBootstrapInstaller()
        {
            string bootstrapScenePath = EnabledScenePaths().Single(path => Classify(path) == SceneRole.Bootstrap);
            OpenScene(bootstrapScenePath);
            SceneContext sceneContext = FindSingle<SceneContext>();

            Type[] installerTypes = InstalledInstallerTypes(sceneContext);

            Assert.That(installerTypes, Is.EqualTo(new[] { typeof(BootstrapInstaller) }));
        }

        [Test]
        public void MenuScene_InstallsMainMenuInstallerOnly()
        {
            string menuScenePath = EnabledScenePaths().Single(path => Classify(path) == SceneRole.Menu);
            OpenScene(menuScenePath);
            SceneContext sceneContext = FindSingle<SceneContext>();

            Type[] installerTypes = InstalledInstallerTypes(sceneContext);

            Assert.That(installerTypes, Is.EqualTo(new[] { typeof(MainMenuInstaller) }));
        }

        [TestCaseSource(nameof(GameplayScenePaths))]
        public void GameplayScene_ProgressionInstallerResolvesItsLevel(string scenePath)
        {
            OpenScene(scenePath);
            string sceneName = EditorSceneManager.GetActiveScene().name;

            Assert.That(Catalog.FindBySceneName(sceneName), Is.InstanceOf<LevelDefinition>(),
                $"Scene '{sceneName}' has no LevelDefinition in the catalog; ProgressionInstaller throws when the scene starts.");
        }

        [TestCaseSource(nameof(EnabledScenePaths))]
        public void EverySceneTransitionTrigger_HasATarget(string scenePath)
        {
            OpenScene(scenePath);

            foreach (SceneTransitionTrigger trigger in FindAll<SceneTransitionTrigger>())
                AssertPrefabFieldAssigned(trigger, "_target");
        }

        [Test]
        public void MenuScene_LevelButtonsReferenceCatalogLevels()
        {
            string menuScenePath = EnabledScenePaths().Single(path => Classify(path) == SceneRole.Menu);
            OpenScene(menuScenePath);

            LevelSelectButton[] buttons = FindAll<LevelSelectButton>();
            Assert.That(buttons, Is.Not.Empty, "The menu scene has no LevelSelectButton components.");

            foreach (LevelSelectButton button in buttons)
            {
                var serializedButton = new SerializedObject(button);
                SerializedProperty levelField = serializedButton.FindProperty("_level");
                SerializedProperty buttonField = serializedButton.FindProperty("_button");

                Assert.That(buttonField.objectReferenceValue, Is.Not.Null, $"{button.name} has no _button assigned.");

                var level = levelField.objectReferenceValue as LevelDefinition;
                Assert.That(level, Is.Not.Null, $"{button.name} has no _level assigned.");
                Assert.That(Catalog.Levels, Does.Contain(level), $"{button.name}'s level '{level.SceneName}' is not in the SceneCatalog.");
            }
        }
    }
}
