using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

using DreamVille.Configs.Enemies;
using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Enemies.Combat;

namespace DreamVille.Tests.EditMode.Enemies
{
    /// <summary>
    /// Content guards for spec 03 §3.2/§3.5 (roadmap 4.1): pins the shipped <see cref="EnemyConfig"/>
    /// assets and the prefabs that reference them, so a batch-3 edit that forgets a config, an
    /// attack spawn or a patrol point turns into a red test instead of an inert or unkillable enemy
    /// discovered at runtime.
    /// </summary>
    public class EnemyContentTests
    {
        private const string EnemyConfigsFolder = "Assets/_Project/Settings/Enemies";
        private const string EnemyExamplePrefabPath = "Assets/_Project/Prefabs/EnemyExample.prefab";
        private const string SimpleRunEnemyPrefabPath = "Assets/_Project/Prefabs/SimpleRunEnemy.prefab";
        private const string FridgeWithEnemyPrefabPath = "Assets/_Project/Prefabs/Misc/FridgeWithEnemy.prefab";
        private const string EnemyBulletPrefabPath = "Assets/_Project/Prefabs/EnemyBullet.prefab";

        private static readonly string[] EnemyPrefabPaths = { EnemyExamplePrefabPath, SimpleRunEnemyPrefabPath };

        private static GameObject LoadPrefab(string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            Assert.That(prefab, Is.Not.Null, $"{prefabPath} is missing.");

            return prefab;
        }

        private static EnemyConfig AssignedConfig(Enemy enemy, string context)
        {
            var serializedEnemy = new SerializedObject(enemy);
            var config = serializedEnemy.FindProperty("_config").objectReferenceValue as EnemyConfig;

            Assert.That(config, Is.Not.Null, $"{context} has no EnemyConfig assigned.");

            return config;
        }

        [Test]
        public void EveryEnemyConfig_HasNonNegativeTimingsAndPositiveDespawnDelay()
        {
            string[] guids = AssetDatabase.FindAssets("t:EnemyConfig", new[] { EnemyConfigsFolder });
            EnemyConfig[] configs = guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<EnemyConfig>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();

            Assert.That(configs.Select(config => config.name), Is.SupersetOf(new[]
            {
                "EnemyConfig_Thrower",
                "EnemyConfig_Runner",
                "EnemyConfig_Walker"
            }));

            foreach (EnemyConfig config in configs)
            {
                Assert.That(config.Acceleration, Is.GreaterThanOrEqualTo(0f), $"{config.name}.Acceleration is negative.");
                Assert.That(config.PatrolArriveRadius, Is.GreaterThanOrEqualTo(0f), $"{config.name}.PatrolArriveRadius is negative.");
                Assert.That(config.HideoutArriveRadius, Is.GreaterThanOrEqualTo(0f), $"{config.name}.HideoutArriveRadius is negative.");
                Assert.That(config.ThrowImpulse, Is.GreaterThanOrEqualTo(0f), $"{config.name}.ThrowImpulse is negative.");
                Assert.That(config.ProjectileLifetime, Is.GreaterThanOrEqualTo(0f), $"{config.name}.ProjectileLifetime is negative.");
                Assert.That(config.HitImmunityDuration, Is.GreaterThanOrEqualTo(0f), $"{config.name}.HitImmunityDuration is negative.");
                Assert.That(config.DespawnDelay, Is.GreaterThan(0f), $"{config.name}.DespawnDelay must be positive.");
                Assert.That(config.MaxLocomotionStartOffset, Is.InRange(0f, 1f), $"{config.name}.MaxLocomotionStartOffset must be within [0, 1].");

                if (!config.SpawnsVulnerable)
                    Assert.That(config.HitImmunityDuration, Is.GreaterThan(0f), $"{config.name} never becomes vulnerable but has no hit-immunity window.");
            }
        }

        [TestCaseSource(nameof(EnemyPrefabPaths))]
        public void EveryEnemyPrefab_HasConfigAndRequiredComponents(string prefabPath)
        {
            GameObject prefab = LoadPrefab(prefabPath);
            Enemy enemy = prefab.GetComponent<Enemy>();

            Assert.That(enemy, Is.Not.Null, $"{prefabPath} has no Enemy component on its root.");
            AssignedConfig(enemy, prefabPath);
            Assert.That(prefab.GetComponent<EnemyHealth>(), Is.Not.Null, $"{prefabPath} has no EnemyHealth.");
            Assert.That(prefab.GetComponent<EnemyRotationHandler>(), Is.Not.Null, $"{prefabPath} has no EnemyRotationHandler.");
            Assert.That(prefab.GetComponent<Rigidbody>(), Is.Not.Null, $"{prefabPath} has no Rigidbody.");
            Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab), Is.Zero, $"{prefabPath} has a component with a missing script.");
        }

        [Test]
        public void FridgeWithEnemyPrefab_NestedWalkersHaveConfigAndTwoPatrolPoints()
        {
            GameObject prefab = LoadPrefab(FridgeWithEnemyPrefabPath);
            Enemy[] walkers = prefab.GetComponentsInChildren<Enemy>(true);

            Assert.That(walkers, Is.Not.Empty, $"{FridgeWithEnemyPrefabPath} has no nested Enemy components.");

            foreach (Enemy walker in walkers)
            {
                AssignedConfig(walker, walker.name);

                var serializedWalker = new SerializedObject(walker);
                SerializedProperty patrolPoints = serializedWalker.FindProperty("patrolPoints");

                Assert.That(patrolPoints.arraySize, Is.EqualTo(2), $"{walker.name} must have exactly two patrol points.");
                for (int i = 0; i < patrolPoints.arraySize; i++)
                    Assert.That(patrolPoints.GetArrayElementAtIndex(i).objectReferenceValue, Is.Not.Null, $"{walker.name} patrol point {i} is not assigned.");
            }
        }

        [Test]
        public void ThrowerPrefabs_HaveAttackSpawnAssigned()
        {
            var enemies = new List<(string Context, Enemy Enemy)>();

            foreach (string prefabPath in EnemyPrefabPaths)
                enemies.Add((prefabPath, LoadPrefab(prefabPath).GetComponent<Enemy>()));

            foreach (Enemy walker in LoadPrefab(FridgeWithEnemyPrefabPath).GetComponentsInChildren<Enemy>(true))
                enemies.Add((walker.name, walker));

            foreach ((string context, Enemy enemy) in enemies)
            {
                var serializedEnemy = new SerializedObject(enemy);
                var config = serializedEnemy.FindProperty("_config").objectReferenceValue as EnemyConfig;

                Assert.That(config, Is.Not.Null, $"{context} has no EnemyConfig assigned.");

                if (config.Attack != EnemyAttackKind.ThrowProjectile)
                    continue;

                SerializedProperty attackSpawn = serializedEnemy.FindProperty("attackSpawn");
                Assert.That(attackSpawn.objectReferenceValue, Is.Not.Null, $"{context} throws projectiles but has no attackSpawn assigned.");
            }
        }

        [Test]
        public void EnemyBulletPrefab_HasProjectileAndRigidbody()
        {
            GameObject prefab = LoadPrefab(EnemyBulletPrefabPath);

            Assert.That(prefab.GetComponent<EnemyProjectile>(), Is.Not.Null, $"{EnemyBulletPrefabPath} has no EnemyProjectile.");
            Assert.That(prefab.GetComponent<Rigidbody>(), Is.Not.Null, $"{EnemyBulletPrefabPath} has no Rigidbody.");
        }
    }
}
