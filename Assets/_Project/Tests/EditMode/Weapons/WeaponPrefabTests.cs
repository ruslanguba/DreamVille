using NUnit.Framework;
using UnityEditor;
using UnityEngine;

using DreamVille.Gameplay.Weapons;
using DreamVille.Gameplay.Weapons.Aiming;
using DreamVille.Gameplay.Weapons.Bullets;
using DreamVille.Gameplay.Weapons.Pillow;

namespace DreamVille.Tests.EditMode.Weapons
{
    /// <summary>
    /// Content guards for spec 05 §5 batch 3 (roadmap 4.4/4.5): pins the shipped
    /// <see cref="Character.prefab"/> weapon wiring so a future edit that leaves the muzzle
    /// unassigned, forgets a pillow slot or resurrects <c>WeaponController</c> as a component
    /// turns into a red test instead of a silent misfire at Play.
    /// </summary>
    public class WeaponPrefabTests
    {
        private const string CharacterPrefabPath = "Assets/_Project/Prefabs/Character.prefab";
        private const string BulletPrefabPath = "Assets/_Project/Prefabs/Bullet.prefab";

        private static GameObject LoadPrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterPrefabPath);

            Assert.That(prefab, Is.Not.Null, $"{CharacterPrefabPath} is missing.");

            return prefab;
        }

        [Test]
        public void PillowGun_HasMuzzleAndBulletConfig()
        {
            GameObject prefab = LoadPrefab();
            var gun = prefab.GetComponentInChildren<PillowGun>(true);

            Assert.That(gun, Is.Not.Null, "Character prefab has no PillowGun.");

            var serializedGun = new SerializedObject(gun);
            SerializedProperty muzzle = serializedGun.FindProperty("_muzzle");
            SerializedProperty config = serializedGun.FindProperty("_config");

            Assert.That(muzzle.objectReferenceValue, Is.Not.Null, "PillowGun._muzzle is not assigned.");
            var muzzleTransform = muzzle.objectReferenceValue as Transform;
            Assert.That(muzzleTransform, Is.Not.Null, "PillowGun._muzzle is not a Transform.");
            Assert.That(muzzleTransform.name, Is.EqualTo("WeaponPivot"), "PillowGun._muzzle must be the WeaponPivot.");
            Assert.That(muzzleTransform.localRotation, Is.EqualTo(Quaternion.identity),
                "WeaponPivot must keep an identity local rotation so its forward equals the character's forward.");

            Assert.That(config.objectReferenceValue, Is.Not.Null, "PillowGun._config is not assigned.");
        }

        [Test]
        public void PillowLauncher_HasValidConfigSpawnPointAndTwoPillows()
        {
            GameObject prefab = LoadPrefab();
            var launcher = prefab.GetComponentInChildren<PillowLauncher>(true);

            Assert.That(launcher, Is.Not.Null, "Character prefab has no PillowLauncher.");

            var serializedLauncher = new SerializedObject(launcher);
            SerializedProperty config = serializedLauncher.FindProperty("_config");
            SerializedProperty spawnPoint = serializedLauncher.FindProperty("_spawnPoint");
            SerializedProperty pillows = serializedLauncher.FindProperty("_pillows");

            Assert.That(config.objectReferenceValue, Is.Not.Null, "PillowLauncher._config is not assigned.");
            Assert.That(spawnPoint.objectReferenceValue, Is.Not.Null, "PillowLauncher._spawnPoint is not assigned.");

            Assert.That(pillows.arraySize, Is.EqualTo(2), "PillowLauncher must have exactly two pillow slots.");
            for (int i = 0; i < pillows.arraySize; i++)
            {
                UnityEngine.Object slot = pillows.GetArrayElementAtIndex(i).objectReferenceValue;
                Assert.That(slot, Is.Not.Null, $"PillowLauncher._pillows[{i}] is not assigned.");
                Assert.That(slot, Is.InstanceOf<Pillow>(), $"PillowLauncher._pillows[{i}] must be a Pillow component.");
            }
        }

        [Test]
        public void CharacterPrefab_HasExactlyOneAimPreviewWithLine_AndNoWeaponControllerComponent()
        {
            GameObject prefab = LoadPrefab();

            AimPreview[] previews = prefab.GetComponentsInChildren<AimPreview>(true);
            Assert.That(previews, Has.Length.EqualTo(1), "Character prefab must have exactly one AimPreview.");

            var serializedPreview = new SerializedObject(previews[0]);
            SerializedProperty line = serializedPreview.FindProperty("_line");
            Assert.That(line.objectReferenceValue, Is.Not.Null, "AimPreview._line is not assigned.");
            Assert.That((line.objectReferenceValue as LineRenderer)?.name, Is.EqualTo("TrajectoryLineRenderer"));

            Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab), Is.Zero,
                "Character prefab has a component with a missing script.");

            foreach (Component component in prefab.GetComponentsInChildren<Component>(true))
                Assert.That(component.GetType().Name, Is.Not.EqualTo("WeaponController"),
                    "Character prefab still carries a WeaponController component; it must be a plain class only.");
        }

        [Test]
        public void BulletPrefab_HitEffect_SimulatesInWorldSpace()
        {
            GameObject bullet = AssetDatabase.LoadAssetAtPath<GameObject>(BulletPrefabPath);
            Assert.That(bullet, Is.Not.Null, $"{BulletPrefabPath} is missing.");
            var handler = bullet.GetComponent<BulletVfxHandler>();
            Assert.That(handler, Is.Not.Null, "Bullet prefab has no BulletVfxHandler.");

            var effect = new SerializedObject(handler).FindProperty("_hitEffect").objectReferenceValue as ParticleSystem;

            // The handler detaches the effect at Awake; in local space every earlier burst would follow the effect
            // to the next hit point.
            Assert.That(effect, Is.Not.Null, "BulletVfxHandler._hitEffect is not assigned.");
            Assert.That(effect.main.simulationSpace, Is.EqualTo(ParticleSystemSimulationSpace.World));
        }
    }
}
