using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using DreamVille.Gameplay.Npc;

namespace DreamVille.Tests.EditMode.Npc
{
    /// <summary>
    /// Pins the authored NPC start poses, the hatter dialogue timing,
    /// the two Lobby_01 queues and the Lobby_01 shooters, so a renamed serialized field that lost its value
    /// or a "cleanup" that removes a queue turns into a red test.
    /// </summary>
    public class NpcContentTests
    {
        private const string ScenesRoot = "Assets/_Project/Scenes/";
        private const string Lobby01ScenePath = ScenesRoot + "Lobby_01.unity";
        private const string NpcDialoguePrefabPath = "Assets/_Project/Prefabs/NPCDialogue.prefab";
        private const string NpcBulletPrefabPath = "Assets/_Project/Prefabs/NPCBullet.prefab";

        private const int QueueCount = 2;
        private const int NpcsPerQueue = 6;

        private string _originalScenePath;
        private bool _scenesOpened;

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

        // Per asset, the multiset of "GameObject name: start state[+swim]" of every NpcAnimationHandler.
        private static IEnumerable<TestCaseData> AuthoredPoses()
        {
            yield return Poses("Assets/_Project/Prefabs/NPC/PluchNpc.prefab", "PluchNpc: Greeting");
            yield return Poses("Assets/_Project/Prefabs/Balloon.prefab", "Townsman2 (3): Greeting");
            // Level_01 has three Balloon prefab instances, each carrying its own Townsman2 (3).
            yield return Poses(ScenesRoot + "Level_01.unity",
                "Townsman2 (3): Greeting", "Townsman2 (3): Greeting", "Townsman2 (3): Greeting");
            yield return Poses(ScenesRoot + "Level_02.unity",
                "Townsman2 (1): Greeting", "Townsman2: Idle+swim", "PluchNpc: Idle");
            yield return Poses(ScenesRoot + "Level_03.unity", "Townsman2: Idle+swim", "PluchNpc: Greeting");
            yield return Poses(ScenesRoot + "Level_04.unity", "PluchNpc: Greeting");
            yield return Poses(Lobby01ScenePath,
                "PluhTalking: Talk", "PluhTalking: Talk", "Ploh_idle (4): Greeting",
                "PluchNpc: Greeting", "PluchNpcFinal: Greeting");
            yield return Poses(ScenesRoot + "Lobby_02.unity",
                "PluhTalking: Talk", "PluhTalking (1): Talk", "PluhTalking (2): Talk", "PluchNpc: Greeting");
            yield return Poses(ScenesRoot + "Max/Final Level 1_Max.unity",
                "Townsman2 (1): Greeting", "Townsman2: Idle+swim", "PluchNpc: Idle");
            yield return Poses(ScenesRoot + "Max/Level_2_Max.unity", "Townsman2: Idle+swim", "PluchNpc: Greeting");
            yield return Poses(ScenesRoot + "Max/Lobby_2_Max.unity", "PluchNpc: Greeting");
        }

        private static TestCaseData Poses(string assetPath, params string[] expected)
            => new TestCaseData(assetPath, expected)
                .SetName($"NpcAnimationHandlers_StartStates_MatchAuthoredPoses({Path.GetFileNameWithoutExtension(assetPath)})");

        [TestCaseSource(nameof(AuthoredPoses))]
        public void NpcAnimationHandlers_StartStates_MatchAuthoredPoses(string assetPath, string[] expected)
        {
            IEnumerable<NpcAnimationHandler> handlers = assetPath.EndsWith(".prefab")
                ? LoadPrefab(assetPath).GetComponentsInChildren<NpcAnimationHandler>(true)
                : OpenSceneAndFindAll<NpcAnimationHandler>(assetPath);

            string[] actual = handlers.Select(DescribePose).ToArray();

            Assert.That(actual, Is.EquivalentTo(expected), $"{assetPath}: authored NPC start poses changed.");
        }

        [Test]
        public void NpcDialoguePrefab_KeepsTwoSpeakersAndFiveSecondInterval()
        {
            AmbientConversation dialogue = LoadPrefab(NpcDialoguePrefabPath).GetComponent<AmbientConversation>();
            Assert.That(dialogue, Is.Not.Null, $"{NpcDialoguePrefabPath} has no {nameof(AmbientConversation)} on its root.");

            var serialized = new SerializedObject(dialogue);
            SerializedProperty speakers = serialized.FindProperty("_speakers");
            Object[] assigned = Enumerable.Range(0, speakers.arraySize)
                .Select(i => speakers.GetArrayElementAtIndex(i).objectReferenceValue)
                .ToArray();

            Assert.That(serialized.FindProperty("_switchInterval").floatValue, Is.EqualTo(5f));
            Assert.That(assigned, Has.Length.EqualTo(2), "speaker count");
            Assert.That(assigned, Has.All.Not.Null, "every speaker assigned");
            Assert.That(assigned, Is.Unique, "two different speakers");
        }

        [Test]
        public void Lobby01Queues_EachDriveTheirOwnSixNpcsWithAuthoredTiming()
        {
            NpcQueue[] queues = OpenSceneAndFindAll<NpcQueue>(Lobby01ScenePath);
            Assert.That(queues, Has.Length.EqualTo(QueueCount), "Lobby_01 has two separate NPC queues (Column, Column (1)).");

            var drivenNpcs = new HashSet<GameObject>();
            foreach (NpcQueue queue in queues)
            {
                var serialized = new SerializedObject(queue);
                SerializedProperty jumpers = serialized.FindProperty("_jumpers");

                Assert.That(serialized.FindProperty("_moveDuration").floatValue, Is.EqualTo(1f), $"'{queue.name}' move duration.");
                Assert.That(serialized.FindProperty("_delayBetweenJumps").floatValue, Is.EqualTo(2f), $"'{queue.name}' jump delay.");
                Assert.That(jumpers.arraySize, Is.EqualTo(NpcsPerQueue), $"'{queue.name}' jumper count.");

                for (int i = 0; i < jumpers.arraySize; i++)
                {
                    var mover = jumpers.GetArrayElementAtIndex(i).objectReferenceValue as NpcMover;
                    Assert.That(mover, Is.Not.Null, $"'{queue.name}' jumper {i} is not assigned.");
                    Assert.That(mover.transform.IsChildOf(queue.transform), Is.True,
                        $"'{queue.name}' jumper {i} ('{mover.name}') is not one of the queue's own NPCs.");
                    Assert.That(drivenNpcs.Add(mover.gameObject), Is.True,
                        $"'{mover.name}' is driven by more than one queue slot.");
                }
            }

            NpcMover[] movers = FindAll<NpcMover>();
            Assert.That(movers, Has.Length.EqualTo(QueueCount * NpcsPerQueue), "one mover per queued NPC, none elsewhere.");
            foreach (NpcMover mover in movers)
            {
                var serialized = new SerializedObject(mover);

                Assert.That(mover.GetComponents<NpcMover>(), Has.Length.EqualTo(1), $"'{mover.name}' has more than one mover.");
                Assert.That(serialized.FindProperty("_jumpForce").floatValue, Is.EqualTo(8f), $"'{mover.name}' jump force.");
                Assert.That(serialized.FindProperty("_forwardForce").floatValue, Is.EqualTo(3f), $"'{mover.name}' forward force.");
            }
        }

        [Test]
        public void Lobby01Shooters_KeepAuthoredRhythm()
        {
            NpcFire[] shooters = OpenSceneAndFindAll<NpcFire>(Lobby01ScenePath);
            GameObject bulletPrefab = LoadPrefab(NpcBulletPrefabPath);

            var rhythms = new List<(float Rate, float LifeTime)>();
            foreach (NpcFire shooter in shooters)
            {
                var serialized = new SerializedObject(shooter);

                Assert.That(serialized.FindProperty("_bulletPrefab").objectReferenceValue, Is.EqualTo(bulletPrefab),
                    $"'{shooter.name}' does not shoot {NpcBulletPrefabPath}.");
                Assert.That(serialized.FindProperty("_spawn").objectReferenceValue, Is.Not.Null, $"'{shooter.name}' spawn.");
                Assert.That(serialized.FindProperty("_target").objectReferenceValue, Is.Not.Null, $"'{shooter.name}' target.");

                rhythms.Add((serialized.FindProperty("_shotInterval").floatValue, serialized.FindProperty("_bulletLifetime").floatValue));
            }

            Assert.That(rhythms, Is.EquivalentTo(new[] { (1f, 3f), (2f, 2f) }));
        }

        private static string DescribePose(NpcAnimationHandler handler)
        {
            bool swims = new SerializedObject(handler).FindProperty("_isSwim").boolValue;
            return $"{handler.name}: {handler.StartState}{(swims ? "+swim" : string.Empty)}";
        }

        private static GameObject LoadPrefab(string assetPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            Assert.That(prefab, Is.Not.Null, $"{assetPath} did not load.");
            return prefab;
        }

        private static T[] OpenSceneAndFindAll<T>(string scenePath) where T : Object
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            return FindAll<T>();
        }

        private static T[] FindAll<T>() where T : Object
            => Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }
}
