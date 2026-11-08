using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using DreamVille.Gameplay.Interactables;
using DreamVille.Gameplay.Interactables.Effects;
using DreamVille.Gameplay.Player.Skins;
using DreamVille.Gameplay.Weapons;

namespace DreamVille.Tests.EditMode.Interactables
{
    /// <summary>
    /// Pins the interaction content wiring (which pickup gives what score,
    /// which effect has a host, which reference is assigned) so a scene or prefab edit cannot silently
    /// drop an effect's host or reference. Opens every scene through <see cref="EditorSceneManager"/> and
    /// every prefab through <see cref="AssetDatabase"/>, for the same reason as SceneCompositionTests:
    /// installers and hosts reach into nested prefabs, so a GUID scan of the raw files is not enough.
    /// </summary>
    public class InteractablesContentTests
    {
        private const string CharacterPrefabPath = "Assets/_Project/Prefabs/Character.prefab";
        private const string CollectablePrefabPath = "Assets/_Project/Prefabs/Collectable.prefab";
        private const string ShaderPaintablePrefabPath = "Assets/_Project/Prefabs/ShaderPaintable.prefab";
        private const string RocketPrefabPath = "Assets/_Project/Prefabs/Rocket.prefab";
        private const string FridgeWithWalkersPrefabPath = "Assets/_Project/Prefabs/Misc/FridgeWithEnemy.prefab";
        private const string Level02ScenePath = "Assets/_Project/Scenes/Level_02.unity";
        private const string Lobby01ScenePath = "Assets/_Project/Scenes/Lobby_01.unity";

        private static readonly (string FileName, float Score)[] SkinPickups =
        {
            ("CosmomanSkin.prefab", 50f),
            ("Doctor.prefab", 50f),
            ("Fireman.prefab", 50f),
            ("Pilot.prefab", 50f),
            ("Polisman.prefab", 50f),
            ("Seaman.prefab", 50f),
            ("Sheriff Skin.prefab", 0f)
        };

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

        private static IEnumerable<string> AllScenePaths()
            => EditorBuildSettings.scenes.Select(scene => scene.path);

        private static void OpenScene(string scenePath) => EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        private static T[] FindAll<T>() where T : UnityEngine.Object
            => UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        private static GameObject LoadPrefab(string assetPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            Assert.That(prefab, Is.Not.Null, $"{assetPath} did not load.");
            return prefab;
        }

        private static void AssertFieldAssigned(Component component, string fieldName, string context)
        {
            var serialized = new SerializedObject(component);
            SerializedProperty field = serialized.FindProperty(fieldName);

            Assert.That(field, Is.Not.Null, $"{component.GetType().Name} has no serialized field named {fieldName}.");
            Assert.That(field.objectReferenceValue, Is.Not.Null,
                $"{context}: '{component.name}' {component.GetType().Name}.{fieldName} is not assigned.");
        }

        private static void AssertScore(Pickup pickup, float expected, string context)
        {
            Assert.That(pickup, Is.Not.Null, $"{context}: no Pickup component found.");

            var serialized = new SerializedObject(pickup);
            float actual = serialized.FindProperty("_scoreToAdd").floatValue;

            Assert.That(actual, Is.EqualTo(expected), $"{context}: Pickup._scoreToAdd is {actual}, expected {expected}.");
        }

        [Test]
        public void EveryInteractionEffect_HasExactlyOneHostOnItsGameObject()
        {
            foreach (string scenePath in AllScenePaths())
            {
                OpenScene(scenePath);

                foreach (MonoBehaviour candidate in FindAll<MonoBehaviour>())
                {
                    if (candidate is not IInteractionEffect)
                        continue;

                    GameObject host = candidate.gameObject;
                    int hostCount = host.GetComponents<Interactable>().Length + host.GetComponents<Pickup>().Length;

                    Assert.That(hostCount, Is.EqualTo(1),
                        $"{scenePath}: '{host.name}' has {candidate.GetType().Name} but {hostCount} hosts (Interactable/Pickup); expected exactly one.");
                }
            }
        }

        [Test]
        public void NoGameObject_HasTwoInteractables()
        {
            foreach (string scenePath in AllScenePaths())
            {
                OpenScene(scenePath);

                var counts = new Dictionary<GameObject, int>();
                foreach (MonoBehaviour candidate in FindAll<MonoBehaviour>())
                {
                    if (candidate is not IInteractable)
                        continue;

                    counts.TryGetValue(candidate.gameObject, out int count);
                    counts[candidate.gameObject] = count + 1;
                }

                foreach (KeyValuePair<GameObject, int> entry in counts)
                    Assert.That(entry.Value, Is.LessThanOrEqualTo(1),
                        $"{scenePath}: '{entry.Key.name}' has {entry.Value} IInteractable components.");
            }
        }

        [Test]
        public void EveryInteractable_HasAtLeastOneEffect()
        {
            foreach (string scenePath in AllScenePaths())
            {
                OpenScene(scenePath);

                foreach (Interactable interactable in FindAll<Interactable>())
                {
                    int effectCount = interactable.GetComponents<IInteractionEffect>().Length;

                    Assert.That(effectCount, Is.GreaterThan(0),
                        $"{scenePath}: '{interactable.name}' is an Interactable with no effect.");
                }
            }
        }

        [Test]
        public void EffectReferences_AreAssigned()
        {
            foreach (string scenePath in AllScenePaths())
            {
                OpenScene(scenePath);

                foreach (ToggleLightEffect effect in FindAll<ToggleLightEffect>())
                    AssertFieldAssigned(effect, "_light", scenePath);

                foreach (PlayAudioSourceEffect effect in FindAll<PlayAudioSourceEffect>())
                    AssertFieldAssigned(effect, "_source", scenePath);

                foreach (FloatAwayEffect effect in FindAll<FloatAwayEffect>())
                    AssertFieldAssigned(effect, "_mover", scenePath);

                foreach (RocketThrustEffect effect in FindAll<RocketThrustEffect>())
                    AssertFieldAssigned(effect, "_particle", scenePath);

                foreach (EquipWeaponEffect effect in FindAll<EquipWeaponEffect>())
                    AssertFieldAssigned(effect, "_weapon", scenePath);

                foreach (PaintEffect effect in FindAll<PaintEffect>())
                {
                    AssertFieldAssigned(effect, "_colorChangeMaterial", scenePath);

                    var serialized = new SerializedObject(effect);
                    SerializedProperty renderers = serialized.FindProperty("_renderer");

                    Assert.That(renderers.arraySize, Is.GreaterThan(0),
                        $"{scenePath}: '{effect.name}' PaintEffect._renderer is empty.");

                    for (int i = 0; i < renderers.arraySize; i++)
                        Assert.That(renderers.GetArrayElementAtIndex(i).objectReferenceValue, Is.Not.Null,
                            $"{scenePath}: '{effect.name}' PaintEffect._renderer[{i}] is not assigned.");
                }

                foreach (ActivateObjectsEffect effect in FindAll<ActivateObjectsEffect>())
                {
                    var serialized = new SerializedObject(effect);
                    SerializedProperty activations = serialized.FindProperty("_activations");

                    Assert.That(activations.arraySize, Is.GreaterThan(0),
                        $"{scenePath}: '{effect.name}' ActivateObjectsEffect has no activations.");

                    for (int i = 0; i < activations.arraySize; i++)
                    {
                        SerializedProperty target = activations.GetArrayElementAtIndex(i).FindPropertyRelative("_target");
                        Assert.That(target.objectReferenceValue, Is.Not.Null,
                            $"{scenePath}: '{effect.name}' ActivateObjectsEffect activation {i} has no target.");
                    }
                }
            }
        }

        [Test]
        public void PickupScores_MatchShippedValues()
        {
            Pickup collectablePrefabPickup = LoadPrefab(CollectablePrefabPath).GetComponent<Pickup>();
            AssertScore(collectablePrefabPickup, 100f, CollectablePrefabPath);

            OpenScene(Level02ScenePath);
            Pickup[] level02ScorePickups = FindAll<Pickup>()
                .Where(pickup => pickup.GetComponent<ChangeSkinEffect>() == null && pickup.GetComponent<EquipWeaponEffect>() == null)
                .ToArray();

            Assert.That(level02ScorePickups, Has.Length.EqualTo(1),
                $"Level_02 must have exactly one plain score Pickup, found {level02ScorePickups.Length}.");
            AssertScore(level02ScorePickups[0], 50f, "Level_02");

            OpenScene(Lobby01ScenePath);
            Pickup[] gunPickups = FindAll<Pickup>().Where(pickup => pickup.GetComponent<EquipWeaponEffect>() != null).ToArray();

            Assert.That(gunPickups, Has.Length.EqualTo(1), "Lobby_01 must have exactly one gun Pickup.");
            AssertScore(gunPickups[0], 0f, "Lobby_01 gun pickup");

            foreach ((string fileName, float expectedScore) in SkinPickups)
            {
                string path = $"Assets/_Project/Prefabs/SkinsCollectableItem/{fileName}";
                Pickup pickup = LoadPrefab(path).GetComponent<Pickup>();
                AssertScore(pickup, expectedScore, path);
            }
        }

        [Test]
        public void SkinPickups_PointAtExistingPlayerSkins()
        {
            GameObject character = LoadPrefab(CharacterPrefabPath);
            PlayerSkins skins = character.GetComponent<PlayerSkins>();
            Assert.That(skins, Is.Not.Null, $"{CharacterPrefabPath} has no PlayerSkins.");

            var serializedSkins = new SerializedObject(skins);
            int skinCount = serializedSkins.FindProperty("_skins").arraySize;

            foreach ((string fileName, _) in SkinPickups)
            {
                string path = $"Assets/_Project/Prefabs/SkinsCollectableItem/{fileName}";
                GameObject prefab = LoadPrefab(path);
                ChangeSkinEffect effect = prefab.GetComponent<ChangeSkinEffect>();
                Assert.That(effect, Is.Not.Null, $"{path} has no ChangeSkinEffect.");

                var serializedEffect = new SerializedObject(effect);
                int skinIndex = serializedEffect.FindProperty("_skinIndex").intValue;

                Assert.That(skinIndex, Is.LessThan(skinCount),
                    $"{path}: ChangeSkinEffect._skinIndex {skinIndex} is out of range for {skinCount} player skins.");
            }
        }

        [Test]
        public void Lobby01GunPickup_EquipsThePlayersPillowGun()
        {
            OpenScene(Lobby01ScenePath);

            EquipWeaponEffect[] gunEffects = FindAll<EquipWeaponEffect>();
            Assert.That(gunEffects, Has.Length.EqualTo(1), "Lobby_01 must have exactly one EquipWeaponEffect.");

            var serialized = new SerializedObject(gunEffects[0]);
            var weapon = serialized.FindProperty("_weapon").objectReferenceValue as PillowGun;
            Assert.That(weapon, Is.Not.Null, "Lobby_01's EquipWeaponEffect._weapon is not assigned.");

            PillowGun[] playerGuns = FindAll<PillowGun>();
            Assert.That(playerGuns, Does.Contain(weapon),
                "Lobby_01's EquipWeaponEffect must point at the PillowGun inside the scene's Character instance.");
        }

        [Test]
        public void FridgeWithWalkers_OpensOnceAndActivatesBothWalkers()
        {
            GameObject prefab = LoadPrefab(FridgeWithWalkersPrefabPath);

            ToggleDoorEffect doorEffect = prefab.GetComponent<ToggleDoorEffect>();
            Assert.That(doorEffect, Is.Not.Null, $"{FridgeWithWalkersPrefabPath} has no ToggleDoorEffect.");

            ActivateObjectsEffect activateEffect = prefab.GetComponent<ActivateObjectsEffect>();
            Assert.That(activateEffect, Is.Not.Null, $"{FridgeWithWalkersPrefabPath} has no ActivateObjectsEffect.");

            var serializedActivate = new SerializedObject(activateEffect);
            SerializedProperty activations = serializedActivate.FindProperty("_activations");
            Assert.That(activations.arraySize, Is.EqualTo(2), $"{FridgeWithWalkersPrefabPath} must activate exactly two walkers.");

            var delays = new float[activations.arraySize];
            for (int i = 0; i < activations.arraySize; i++)
            {
                SerializedProperty entry = activations.GetArrayElementAtIndex(i);
                var target = entry.FindPropertyRelative("_target").objectReferenceValue as GameObject;
                delays[i] = entry.FindPropertyRelative("_delay").floatValue;

                Assert.That(target, Is.Not.Null, $"{FridgeWithWalkersPrefabPath} activation {i} has no target.");
                Assert.That(target.activeSelf, Is.False, $"{FridgeWithWalkersPrefabPath} activation {i}'s target must start inactive.");
            }

            Assert.That(delays, Is.EquivalentTo(new[] { 1.3f, 2.3f }),
                $"{FridgeWithWalkersPrefabPath} must activate its walkers at 1.3s and 2.3s.");
        }

        [Test]
        public void ShaderPaintable_PaintsInFiveStrokesOfTenPoints()
        {
            GameObject prefab = LoadPrefab(ShaderPaintablePrefabPath);
            PaintEffect paint = prefab.GetComponent<PaintEffect>();
            Assert.That(paint, Is.Not.Null, $"{ShaderPaintablePrefabPath} has no PaintEffect.");

            var serialized = new SerializedObject(paint);
            Assert.That(serialized.FindProperty("_strokeCount").intValue, Is.EqualTo(5),
                $"{ShaderPaintablePrefabPath}: PaintEffect._strokeCount changed from 5.");
            Assert.That(serialized.FindProperty("_scorePerStroke").floatValue, Is.EqualTo(10f),
                $"{ShaderPaintablePrefabPath}: PaintEffect._scorePerStroke changed from 10.");
        }

        [Test]
        public void RocketPrefab_KeepsItsThrustTuning()
        {
            GameObject prefab = LoadPrefab(RocketPrefabPath);
            RocketThrustEffect rocket = prefab.GetComponentInChildren<RocketThrustEffect>(true);
            Assert.That(rocket, Is.Not.Null, $"{RocketPrefabPath} has no RocketThrustEffect.");

            var serialized = new SerializedObject(rocket);
            Assert.That(serialized.FindProperty("_thrustPower").floatValue, Is.EqualTo(200f));
            Assert.That(serialized.FindProperty("_continuousThrust").floatValue, Is.EqualTo(500f));
            Assert.That(serialized.FindProperty("_lifetimeAfterLaunch").floatValue, Is.EqualTo(20f));

            var particle = serialized.FindProperty("_particle").objectReferenceValue as ParticleSystem;
            Assert.That(particle, Is.Not.Null, $"{RocketPrefabPath}: RocketThrustEffect._particle is not assigned.");
            Assert.That(particle.name, Is.EqualTo("RocketFire"),
                $"{RocketPrefabPath}: RocketThrustEffect._particle must be the RocketFire child.");
        }
    }
}
