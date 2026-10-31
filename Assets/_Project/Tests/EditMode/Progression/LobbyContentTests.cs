using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

using DreamVille.Configs;
using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Environment;
using DreamVille.Gameplay.Npc;
using DreamVille.Progression;
using DreamVille.Progression.Lobby;

namespace DreamVille.Tests.EditMode.Progression
{
    /// <summary>
    /// Content guards for spec 07 batch 4 (§5 group D): pins how `Lobby_01`'s scenario shipped
    /// (2 + 2 kill groups per user decision D10, the doors, the guide) and the cross-scene
    /// hardening from batch 3 (a required, scene-native `LevelActors`; the renamed `LevelSystems`
    /// prefab), so a future edit that silently drops a reference or reintroduces an added
    /// component on a prefab instance turns into a red test instead of the `ebe83a3` regression.
    /// </summary>
    public class LobbyContentTests
    {
        private const string CatalogAssetPath = "Assets/_Project/Settings/Levels/SceneCatalog.asset";
        private const string LobbyScenePath = "Assets/_Project/Scenes/Lobby_01.unity";
        private const string LevelSystemsPrefabPath = "Assets/_Project/Prefabs/Utilities/LevelSystems.prefab";
        private const string LevelSystemsPrefabGuid = "2cb29353ae990934096c7e001e02409b";

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

        private static SceneRole Classify(string scenePath)
        {
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);

            if (sceneName == Catalog.BootstrapScene.SceneName)
                return SceneRole.Bootstrap;
            if (sceneName == Catalog.MainMenuScene.SceneName)
                return SceneRole.Menu;

            return SceneRole.Gameplay;
        }

        private static IEnumerable<string> GameplayScenePaths()
            => EnabledScenePaths().Where(path => Classify(path) == SceneRole.Gameplay);

        private static void OpenScene(string scenePath) => EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        private static void OpenLobby() => OpenScene(LobbyScenePath);

        private static T[] FindAll<T>() where T : Object
            => Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        private static string HierarchyPath(Transform transform)
        {
            var segments = new List<string>();
            for (Transform current = transform; current != null; current = current.parent)
                segments.Insert(0, current.name);

            return string.Join("/", segments);
        }

        [Test]
        public void Lobby01_HasOneLobbyContext_WithCompleteCast()
        {
            OpenLobby();

            LobbyActors[] casts = FindAll<LobbyActors>();
            Assert.That(casts, Has.Length.EqualTo(1), "Lobby_01 must have exactly one LobbyActors.");

            LobbyActors actors = casts[0];
            Assert.That(actors.FindMissingReferences(), Is.Empty, "Lobby_01's LobbyActors is missing a cast reference.");

            GameObjectContext context = actors.GetComponent<GameObjectContext>();
            Assert.That(context, Is.Not.Null, "The LobbyActors object must also carry the lobby's GameObjectContext.");

            LobbyInstaller installer = actors.GetComponent<LobbyInstaller>();
            Assert.That(installer, Is.Not.Null, "The LobbyActors object must also carry the LobbyInstaller.");

            var serializedContext = new SerializedObject(context);
            SerializedProperty monoInstallers = serializedContext.FindProperty("_monoInstallers");
            var installedTypes = new List<Object>();
            for (int i = 0; i < monoInstallers.arraySize; i++)
                installedTypes.Add(monoInstallers.GetArrayElementAtIndex(i).objectReferenceValue);

            Assert.That(installedTypes, Does.Contain(installer),
                "The lobby's GameObjectContext must install its LobbyInstaller.");

            bool hasLegacyProgress = FindAll<Component>()
                .Any(component => component != null && component.GetType().FullName == "DreamVille.Progression.Lobby1Progress");
            Assert.That(hasLegacyProgress, Is.False, "Lobby1Progress must not exist anywhere in Lobby_01 (deleted, spec 07 batch 2).");

            int missingScripts = EditorSceneManager.GetActiveScene().GetRootGameObjects()
                .Sum(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount);
            Assert.That(missingScripts, Is.Zero, "Lobby_01 has a component with a missing script.");
        }

        [Test]
        public void Lobby01_KillGroups_AreDisjointAndCoverTheQuestDesign()
        {
            OpenLobby();

            LobbyActors actors = FindAll<LobbyActors>().Single();
            var firstGroup = actors.FirstGroup.Cast<EnemyHealth>().ToArray();
            var secondGroup = actors.SecondGroup.Cast<EnemyHealth>().ToArray();

            Assert.That(firstGroup, Has.Length.EqualTo(2), "First kill group must have exactly 2 members.");
            Assert.That(secondGroup, Has.Length.EqualTo(2), "Second kill group must have exactly 2 members (user decision D10).");

            Assert.That(firstGroup.Select(e => HierarchyPath(e.transform)), Is.EquivalentTo(new[]
            {
                "exampl_1/EnemyExample (5)",
                "exampl_1/EnemyExample (3)"
            }));

            Assert.That(secondGroup.Select(e => HierarchyPath(e.transform)), Is.EquivalentTo(new[]
            {
                "S1/EnemyExample (1)",
                "exampl_1/EnemyExample (6)"
            }));

            EnemyHealth[] allEnemies = FindAll<EnemyHealth>();
            EnemyHealth[] grouped = firstGroup.Concat(secondGroup).ToArray();

            Assert.That(grouped, Is.Unique, "A member appears in both groups; the second gate would never open on its own.");
            Assert.That(grouped, Is.EquivalentTo(allEnemies),
                "Every EnemyHealth in Lobby_01 must belong to exactly one kill group (D10: 2 + 2 covers the whole cast).");
        }

        [Test]
        public void Lobby01_Doors_OpenThroughATrigger_WithAudibleSound()
        {
            OpenLobby();

            LobbyActors actors = FindAll<LobbyActors>().Single();

            AssertDoor((Door)actors.FirstDoor, "first door");
            AssertDoor((Door)actors.SecondDoor, "second door");
        }

        private static void AssertDoor(Door door, string context)
        {
            Assert.That(door, Is.Not.Null, $"Lobby_01's {context} is not assigned.");

            var serializedDoor = new SerializedObject(door);
            Assert.That(serializedDoor.FindProperty("_openSound").objectReferenceValue, Is.Not.Null,
                $"{context}'s Door has no _openSound assigned.");

            var controller = door.GetComponent<Animator>().runtimeAnimatorController as AnimatorController;
            Assert.That(controller, Is.Not.Null, $"{context}'s Animator has no AnimatorController.");
            AnimatorControllerParameter openParameter = controller.parameters.FirstOrDefault(p => p.name == "open");
            Assert.That(openParameter, Is.Not.Null, $"{context}'s controller has no 'open' parameter.");
            Assert.That(openParameter.type, Is.EqualTo(AnimatorControllerParameterType.Trigger),
                $"{context}'s 'open' parameter must be a Trigger.");

            var audio = door.GetComponent<AudioSource>();
            Assert.That(audio, Is.Not.Null, $"{context} has no AudioSource.");
            Assert.That(audio.volume, Is.EqualTo(1f), $"{context}'s AudioSource volume must be 1 (same loudness as before, D6).");
            Assert.That(audio.spatialBlend, Is.EqualTo(0f), $"{context}'s AudioSource must stay 2D (open question 5: keep).");
        }

        [Test]
        public void Lobby01_Guide_IsPluchNpc_WithMumbleAtFullVolume()
        {
            OpenLobby();

            LobbyActors actors = FindAll<LobbyActors>().Single();
            var guide = (NpcReactionController)actors.Guide;
            Assert.That(guide, Is.Not.Null, "Lobby_01's guide is not assigned.");
            Assert.That(HierarchyPath(guide.transform), Is.EqualTo("PluchAllNpces/PluchNpc"),
                "The guide must live on PluchAllNpces/PluchNpc.");

            var serializedGuide = new SerializedObject(guide);
            SerializedProperty mumbleClips = serializedGuide.FindProperty("_mumbleClips");
            Assert.That(mumbleClips.arraySize, Is.EqualTo(3), "The guide must have three mumble clips (D3).");

            var audio = guide.GetComponent<AudioSource>();
            Assert.That(audio, Is.Not.Null, "The guide has no AudioSource.");
            Assert.That(audio.volume, Is.EqualTo(1f), "The guide's AudioSource must be overridden to volume 1 (D3; prefab default is 0.5).");

            LevelActors levelActors = FindAll<LevelActors>().SingleOrDefault();
            if (levelActors != null)
                Assert.That(guide, Is.Not.SameAs(levelActors.FinalNpc), "The lobby guide and the final NPC must be different NPCs.");
        }

        [TestCaseSource(nameof(GameplayScenePaths))]
        public void GameplayScene_HasExactlyOneLevelActorsOnASceneObject(string scenePath)
        {
            OpenScene(scenePath);

            LevelActors[] found = FindAll<LevelActors>();
            Assert.That(found, Has.Length.EqualTo(1), $"{scenePath} must have exactly one LevelActors (D9: required, no silent skip).");

            LevelActors actors = found[0];
            Assert.That(PrefabUtility.IsPartOfPrefabInstance(actors.gameObject), Is.False,
                $"{scenePath}'s LevelActors must be a scene-native object, not part of a prefab instance (the ebe83a3 failure mode).");

            SceneContext sceneContext = FindAll<SceneContext>().Single();
            GameObject instanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(sceneContext.gameObject);
            Assert.That(instanceRoot, Is.Not.Null, $"{scenePath}'s SceneContext is not part of a prefab instance.");

            Component[] addedComponents = PrefabUtility.GetAddedComponents(instanceRoot).Select(a => a.instanceComponent).ToArray();
            Assert.That(addedComponents, Is.Empty,
                $"{scenePath}'s SceneContext prefab instance must have no added components; LevelActors must live on its own scene object.");
        }

        [TestCaseSource(nameof(GameplayScenePaths))]
        public void GameplayScene_HasOneLevelSystemsPrefabInstance(string scenePath)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(LevelSystemsPrefabPath);
            Assert.That(prefabAsset, Is.Not.Null, $"{LevelSystemsPrefabPath} is missing.");
            Assert.That(AssetDatabase.AssetPathToGUID(LevelSystemsPrefabPath), Is.EqualTo(LevelSystemsPrefabGuid),
                "LevelSystems.prefab must keep its original GUID (renamed from LevelProgressSystem, not recreated).");

            OpenScene(scenePath);

            GameObject[] instanceRoots = FindAll<Transform>()
                .Select(t => t.gameObject)
                .Where(go => PrefabUtility.GetNearestPrefabInstanceRoot(go) == go
                    && PrefabUtility.GetCorrespondingObjectFromSource(go) == prefabAsset)
                .ToArray();

            Assert.That(instanceRoots, Has.Length.EqualTo(1), $"{scenePath} must have exactly one LevelSystems.prefab instance.");
        }
    }
}
