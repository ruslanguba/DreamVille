using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Zenject;

using DreamVille.Bootstrap;
using DreamVille.Configs;
using DreamVille.Dialogue;
using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.NPC;
using DreamVille.Gameplay.Player;
using DreamVille.Gameplay.Player.Ground;
using DreamVille.Gameplay.Weapons.Aiming;
using DreamVille.Gameplay.Weapons.Pillow;
using DreamVille.Input;
using DreamVille.Progression;
using DreamVille.SceneManagement;
using DreamVille.UI.Hints;
using DreamVille.UI.Menus;

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

        /// <summary>
        /// A UGUI graphic is drawn only under a Canvas. Level_02's hints once sat under a plain level object and never
        /// appeared (spec 12 F1); every hint target must live under a screen-space canvas.
        /// </summary>
        [TestCaseSource(nameof(GameplayScenePaths))]
        public void GameplayScene_EveryHintTargetIsUnderAScreenSpaceCanvas(string scenePath)
        {
            OpenScene(scenePath);

            string[] offenders = FindAll<ProximityHint>()
                .Select(hint => new SerializedObject(hint).FindProperty("_object").objectReferenceValue as GameObject)
                .Where(target => target == null || !IsUnderScreenSpaceCanvas(target))
                .Select(target => target == null ? "<no target>" : target.name)
                .ToArray();

            Assert.That(offenders, Is.Empty, $"{scenePath}: hint targets not under a screen-space canvas: {string.Join(", ", offenders)}");
        }

        private static bool IsUnderScreenSpaceCanvas(GameObject target)
        {
            Canvas canvas = target.GetComponentInParent<Canvas>(true);
            return canvas != null && canvas.rootCanvas.renderMode != RenderMode.WorldSpace;
        }

        /// <summary>
        /// A component whose script was deleted stays in the scene as a silent "missing script". Checks every
        /// GameObject, inactive ones and children included, not only the roots.
        /// </summary>
        [TestCaseSource(nameof(EnabledScenePaths))]
        public void EveryEnabledScene_HasNoMissingScripts(string scenePath)
        {
            OpenScene(scenePath);

            string[] broken = FindAll<Transform>()
                .Where(transform => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) > 0)
                .Select(transform => transform.name)
                .ToArray();

            Assert.That(broken, Is.Empty, $"{scenePath} has missing scripts on: {string.Join(", ", broken)}");
        }

        [TestCaseSource(nameof(GameplayScenePaths))]
        public void GameplayScene_InstallsTheFourRequiredInstallers(string scenePath)
        {
            OpenScene(scenePath);
            SceneContext sceneContext = FindSingle<SceneContext>();

            Type[] installerTypes = InstalledInstallerTypes(sceneContext);

            Assert.That(installerTypes, Is.EquivalentTo(new[]
            {
                typeof(SceneServicesInstaller),
                typeof(CombatInstaller),
                typeof(ProgressionInstaller),
                typeof(UiInstaller)
            }));
        }

        [TestCaseSource(nameof(GameplayScenePaths))]
        public void GameplayScene_HasEveryFromComponentInHierarchyTarget(string scenePath)
        {
            OpenScene(scenePath);

            Assert.That(FindAll<PlayerCharacter>(), Is.Not.Empty, "PlayerCharacter is missing.");
            Assert.That(FindAll<PlayerInputSwitch>(), Is.Not.Empty, "PlayerInputSwitch is missing.");
            Assert.That(FindAll<PillowLauncher>(), Is.Not.Empty, "PillowLauncher is missing.");
            Assert.That(FindAll<EnemyRegistry>(), Is.Not.Empty, "EnemyRegistry is missing.");
            Assert.That(FindAll<GameInput>(), Is.Not.Empty, "GameInput is missing.");
            Assert.That(FindAll<PauseMenuView>(), Has.Length.EqualTo(1), "Expected exactly one PauseMenuView.");
            Assert.That(FindAll<AudioSettingsView>(), Has.Length.EqualTo(1), "Expected exactly one AudioSettingsView (the pause menu's).");

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

        [TestCaseSource(nameof(GameplayScenePaths))]
        public void GameplayScene_UiInstallerHasDialoguePrefab(string scenePath)
        {
            OpenScene(scenePath);

            AssertPrefabFieldAssigned(FindSingle<UiInstaller>(), "_dialoguePrefab");
        }

        /// <summary>
        /// The dialogue canvas no longer creates an EventSystem at runtime (spec 11 B3), so every gameplay scene
        /// must bring exactly one for the Next button and the pause menu to receive clicks.
        /// </summary>
        [TestCaseSource(nameof(GameplayScenePaths))]
        public void GameplayScene_HasExactlyOneEventSystem(string scenePath)
        {
            OpenScene(scenePath);

            Assert.That(FindAll<EventSystem>(), Has.Length.EqualTo(1), $"{scenePath} must have exactly one EventSystem.");
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
        public void GameplayScene_PlayerHasExactlyOneGroundSensor(string scenePath)
        {
            OpenScene(scenePath);

            Assert.That(FindAll<GroundSurfaceSensor>(), Has.Length.EqualTo(1),
                $"{scenePath} must have exactly one GroundSurfaceSensor on the player (spec 04 §2.2 stray-component drift).");
        }

        [TestCaseSource(nameof(GameplayScenePaths))]
        public void GameplayScene_PlayerHasExactlyOneAimPreview(string scenePath)
        {
            OpenScene(scenePath);

            Assert.That(FindAll<AimPreview>(), Has.Length.EqualTo(1),
                $"{scenePath} must have exactly one AimPreview (spec 05 §2.2 duplicate BounceRayTrajectory drift).");
        }

        [TestCaseSource(nameof(GameplayScenePaths))]
        public void GameplayScene_NoInstanceOverridesPillowList(string scenePath)
        {
            OpenScene(scenePath);
            PillowLauncher launcher = FindSingle<PillowLauncher>();

            GameObject instanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(launcher.gameObject);
            Assert.That(instanceRoot, Is.Not.Null, $"{scenePath}'s PillowLauncher is not part of a prefab instance.");

            PropertyModification[] modifications = PrefabUtility.GetPropertyModifications(instanceRoot) ?? Array.Empty<PropertyModification>();

            Assert.That(modifications.Any(m => m.propertyPath.StartsWith("_pillows")), Is.False,
                $"{scenePath}'s player instance overrides PillowLauncher._pillows; pillow availability must come from IPillowInventory data, not per-scene overrides.");
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

        [TestCaseSource(nameof(GameplayScenePaths))]
        public void GameplayScene_CanBeCompleted(string scenePath)
        {
            OpenScene(scenePath);

            bool hasFinale = FindAll<FinalZone>().Any(IsArmable);
            bool hasCompletingPortal = FindAll<SceneTransitionTrigger>()
                .Any(trigger => new SerializedObject(trigger).FindProperty("_completesLevel").boolValue);

            Assert.That(hasFinale || hasCompletingPortal, Is.True,
                $"{scenePath} can never be completed: no armable FinalZone and no SceneTransitionTrigger with _completesLevel, so its level-select button stays locked.");
        }

        /// <summary>A FinalZone the player can actually trigger: its dialogue is wired, directly or as a child.</summary>
        private static bool IsArmable(FinalZone zone)
        {
            var serializedZone = new SerializedObject(zone);
            SerializedProperty dialogue = serializedZone.FindProperty("_dialogue");

            if (dialogue.objectReferenceValue != null)
                return true;

            return zone.GetComponentInChildren<DialogueTrigger>(true) != null;
        }

        [TestCaseSource(nameof(GameplayScenePaths))]
        public void GameplayScene_WithAFinalZone_HasLevelActorsPointingAtIt(string scenePath)
        {
            OpenScene(scenePath);

            FinalZone[] armableZones = FindAll<FinalZone>().Where(IsArmable).ToArray();

            if (armableZones.Length == 0)
            {
                TestContext.WriteLine($"{scenePath}: no armed-able FinalZone (legitimately absent, e.g. Lobby_02).");
                return;
            }

            Assert.That(armableZones, Has.Length.EqualTo(1),
                $"{scenePath} has {armableZones.Length} armed-able FinalZone components; expected exactly one (regression guard for 920740b).");

            LevelActors levelActors = FindSingle<LevelActors>();

            Assert.That(levelActors.FinalZone, Is.SameAs(armableZones[0]),
                $"{scenePath}'s LevelActors must point at the armed-able FinalZone, not a decoy; the final zone never arms otherwise.");
        }

        private const float NpcNearFinalZoneMaxDistance = 20f;

        [TestCaseSource(nameof(GameplayScenePaths))]
        public void GameplayScene_LevelActorsFinalNpc_IsInsideOrNearFinalZone(string scenePath)
        {
            OpenScene(scenePath);

            LevelActors[] found = FindAll<LevelActors>();
            if (found.Length != 1)
                return;

            var serializedActors = new SerializedObject(found[0]);
            var finalNpc = serializedActors.FindProperty("_finalNpc").objectReferenceValue as NPCReactionController;

            if (finalNpc == null)
            {
                Assert.Ignore($"{scenePath}: LevelActors has no FinalNpc (legitimately absent, e.g. Level_01).");
                return;
            }

            FinalZone zone = found[0].FinalZone;
            Assert.That(zone, Is.Not.Null, $"{scenePath}'s LevelActors has a FinalNpc but no FinalZone.");

            float distance = Vector3.Distance(finalNpc.transform.position, zone.transform.position);
            Assert.That(distance, Is.LessThanOrEqualTo(NpcNearFinalZoneMaxDistance),
                $"{scenePath}'s final NPC is {distance:F1}m from its FinalZone (max {NpcNearFinalZoneMaxDistance}m); it should stand inside or near it.");
        }

        [Test]
        public void MenuScene_HasEveryFromComponentInHierarchyTarget()
        {
            string menuScenePath = EnabledScenePaths().Single(path => Classify(path) == SceneRole.Menu);
            OpenScene(menuScenePath);

            Assert.That(FindAll<MainMenuView>(), Has.Length.EqualTo(1), "Expected exactly one MainMenuView.");
            Assert.That(FindAll<LevelSelectView>(), Has.Length.EqualTo(1), "Expected exactly one LevelSelectView.");
            Assert.That(FindAll<AudioSettingsView>(), Has.Length.EqualTo(1), "Expected exactly one AudioSettingsView.");
        }

        [Test]
        public void MenuScene_LevelButtonsReferenceCatalogLevels()
        {
            string menuScenePath = EnabledScenePaths().Single(path => Classify(path) == SceneRole.Menu);
            OpenScene(menuScenePath);

            LevelButtonView[] buttons = FindAll<LevelButtonView>();
            Assert.That(buttons, Is.Not.Empty, "The menu scene has no LevelButtonView components.");

            foreach (LevelButtonView button in buttons)
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
