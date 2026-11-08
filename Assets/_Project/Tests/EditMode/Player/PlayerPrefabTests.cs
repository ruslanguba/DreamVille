using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Unity.Cinemachine;
using Zenject;

using DreamVille.Audio;
using DreamVille.Configs;
using DreamVille.Gameplay;
using DreamVille.Gameplay.Damage;
using DreamVille.Gameplay.Player;
using DreamVille.Gameplay.Player.Ground;
using DreamVille.Input;

namespace DreamVille.Tests.EditMode.Player
{
    /// <summary>
    /// Content guards: pins the shipped <c>Character.prefab</c>
    /// composition root so a future edit that removes the GameObjectContext, drops the config or
    /// reintroduces a stray ground sensor turns into a red test instead of a ZenjectException at Play.
    /// </summary>
    public class PlayerPrefabTests
    {
        private const string CharacterPrefabPath = "Assets/_Project/Prefabs/Character.prefab";

        private static GameObject LoadPrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterPrefabPath);

            Assert.That(prefab, Is.Not.Null, $"{CharacterPrefabPath} is missing.");

            return prefab;
        }

        [Test]
        public void CharacterPrefab_RootHasGameObjectContextWithPlayerInstaller()
        {
            GameObject prefab = LoadPrefab();
            var context = prefab.GetComponent<GameObjectContext>();
            var installer = prefab.GetComponent<PlayerInstaller>();

            Assert.That(context, Is.Not.Null, "Character root has no GameObjectContext.");
            Assert.That(installer, Is.Not.Null, "Character root has no PlayerInstaller.");

            var serializedContext = new SerializedObject(context);
            SerializedProperty installers = serializedContext.FindProperty("_monoInstallers");

            var installedInstallers = Enumerable.Range(0, installers.arraySize)
                .Select(i => installers.GetArrayElementAtIndex(i).objectReferenceValue)
                .ToArray();

            Assert.That(installedInstallers, Does.Contain(installer),
                "GameObjectContext._monoInstallers does not contain the root's PlayerInstaller.");
        }

        [Test]
        public void CharacterPrefab_PlayerInstallerHasConfig()
        {
            GameObject prefab = LoadPrefab();
            var installer = prefab.GetComponent<PlayerInstaller>();
            Assert.That(installer, Is.Not.Null, "Character root has no PlayerInstaller.");

            var serializedInstaller = new SerializedObject(installer);
            SerializedProperty configField = serializedInstaller.FindProperty("_config");

            Assert.That(configField, Is.Not.Null, "PlayerInstaller has no serialized _config field.");
            var config = configField.objectReferenceValue as PlayerConfig;
            Assert.That(config, Is.Not.Null, "PlayerInstaller._config is not assigned.");
            Assert.That(AssetDatabase.GetAssetPath(config), Is.EqualTo("Assets/_Project/Settings/Movement/PlayerConfig.asset"));
        }

        [Test]
        public void CharacterPrefab_RootHasRequiredComponents()
        {
            GameObject prefab = LoadPrefab();

            Assert.That(prefab.GetComponent<CharacterController>(), Is.Not.Null, "Character root has no CharacterController.");
            Assert.That(prefab.GetComponent<InputReader>(), Is.Not.Null, "Character root has no InputReader.");
            Assert.That(prefab.GetComponent<PlayerCharacter>(), Is.Not.Null, "Character root has no PlayerCharacter.");
            Assert.That(prefab.GetComponent<PlayerDamageReceiver>(), Is.Not.Null, "Character root has no PlayerDamageReceiver.");
            Assert.That(prefab.GetComponent<PlayerInputSwitch>(), Is.Not.Null, "Character root has no PlayerInputSwitch.");
            Assert.That(prefab.GetComponent<ActorAudio>(), Is.Not.Null, "Character root has no ActorAudio.");

            var sensor = prefab.GetComponent<GroundSurfaceSensor>();
            Assert.That(sensor, Is.Not.Null, "Character root has no GroundSurfaceSensor.");

            var serializedSensor = new SerializedObject(sensor);
            SerializedProperty pivot = serializedSensor.FindProperty("_pivot");
            Assert.That(pivot.objectReferenceValue, Is.Not.Null, "GroundSurfaceSensor._pivot is not assigned.");

            int playerLayer = LayerMask.NameToLayer(Layers.Player);
            Assert.That(playerLayer, Is.GreaterThanOrEqualTo(0), "The 'Player' layer is not defined in the project.");
            SerializedProperty surfaceMask = serializedSensor.FindProperty("_surfaceMask");
            Assert.That((surfaceMask.intValue & (1 << playerLayer)), Is.Zero,
                "GroundSurfaceSensor._surfaceMask includes the Player layer.");

            Animator[] animators = prefab.GetComponentsInChildren<Animator>(true);
            Animator[] activeAnimators = prefab.GetComponentsInChildren<Animator>(false);
            Assert.That(activeAnimators, Has.Length.EqualTo(1), "Character prefab must have exactly one active skin Animator.");
            Assert.That(animators.First(), Is.EqualTo(activeAnimators[0]),
                "The active skin Animator must be the first Animator found in the prefab subtree, as FromComponentInHierarchy(includeInactive: false) expects.");

            Transform freeLookCamera = prefab.transform.Find("FreeLook Camera");
            Assert.That(freeLookCamera, Is.Not.Null, "Character prefab has no 'FreeLook Camera' child.");
            Assert.That(freeLookCamera.GetComponent<CinemachineCamera>(), Is.Not.Null, "'FreeLook Camera' has no CinemachineCamera.");
            Assert.That(freeLookCamera.GetComponent<CinemachineInputAxisController>(), Is.Not.Null, "'FreeLook Camera' has no CinemachineInputAxisController.");

            Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab), Is.Zero,
                "Character prefab has a component with a missing script.");
        }

        [Test]
        public void Skins_FirstIsTheOnlyActiveOne()
        {
            GameObject prefab = LoadPrefab();
            var skins = prefab.GetComponentInChildren<DreamVille.Gameplay.Player.Skins.PlayerSkins>(true);
            Assert.That(skins, Is.Not.Null, "Character prefab has no PlayerSkins.");

            SerializedProperty list = new SerializedObject(skins).FindProperty("_skins");
            Assert.That(list.arraySize, Is.GreaterThan(0), "PlayerSkins has no skins.");

            // PlayerSkins starts on skin 0; the prefab must show exactly that one, or the driver and the visible
            // model disagree until the first skin change.
            for (int i = 0; i < list.arraySize; i++)
            {
                var skin = list.GetArrayElementAtIndex(i).objectReferenceValue as Animator;
                Assert.That(skin, Is.Not.Null, $"PlayerSkins._skins[{i}] is empty.");
                Assert.That(skin.gameObject.activeSelf, Is.EqualTo(i == 0), $"Skin {i} ('{skin.name}') has the wrong active state.");
            }
        }

        [Test]
        public void CharacterPrefab_HasExactlyOneGroundSensorAndOneRespawn()
        {
            GameObject prefab = LoadPrefab();

            Assert.That(prefab.GetComponentsInChildren<GroundSurfaceSensor>(true), Has.Length.EqualTo(1),
                "Character prefab must have exactly one GroundSurfaceSensor.");
            Assert.That(prefab.GetComponentsInChildren<PlayerRespawn>(true), Has.Length.EqualTo(1),
                "Character prefab must have exactly one PlayerRespawn.");
        }
    }
}
