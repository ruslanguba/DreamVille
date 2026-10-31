using System;
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
    /// Content guards for the shipped <see cref="EnemyConfig"/> archetypes and the prefabs that reference them:
    /// a forgotten config, attack spawn or patrol point, or an archetype whose numbers the state machine
    /// cannot honour (a chase range that never chases, a thrower that never throws or never turns), turns
    /// into a red test instead of an inert or unkillable enemy discovered at runtime (ADR 0011).
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
            var config = serializedEnemy.FindProperty(EnemyTuningFields.Config).objectReferenceValue as EnemyConfig;

            Assert.That(config, Is.Not.Null, $"{context} has no EnemyConfig assigned.");

            return config;
        }

        private static EnemyConfig[] LoadEnemyConfigs()
        {
            string[] guids = AssetDatabase.FindAssets("t:EnemyConfig", new[] { EnemyConfigsFolder });
            EnemyConfig[] configs = guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<EnemyConfig>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();

            Assert.That(configs, Is.Not.Empty, $"No EnemyConfig assets under {EnemyConfigsFolder}; every config rule would pass vacuously.");

            return configs;
        }

        [Test]
        public void EveryEnemyConfig_HasNonNegativeTimingsAndPositiveDespawnDelay()
        {
            EnemyConfig[] configs = LoadEnemyConfigs();

            Assert.That(configs.Select(config => config.name), Is.SupersetOf(new[]
            {
                "EnemyConfig_Hunter",
                "EnemyConfig_Stalker",
                "EnemyConfig_RapidSentry",
                "EnemyConfig_Sentry",
                "EnemyConfig_Lookout",
                "EnemyConfig_Runner",
                "EnemyConfig_Walker"
            }));

            foreach (EnemyConfig config in configs)
            {
                Assert.That(config.MoveSpeed, Is.GreaterThanOrEqualTo(0f), $"{config.name}.MoveSpeed is negative.");
                Assert.That(config.ChaseRange, Is.GreaterThanOrEqualTo(0f), $"{config.name}.ChaseRange is negative.");
                Assert.That(config.AttackRange, Is.GreaterThanOrEqualTo(0f), $"{config.name}.AttackRange is negative.");
                Assert.That(config.AttackCooldown, Is.GreaterThanOrEqualTo(0f), $"{config.name}.AttackCooldown is negative.");
                Assert.That(config.RotationSpeed, Is.GreaterThanOrEqualTo(0f), $"{config.name}.RotationSpeed is negative.");
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

        [Test]
        public void EveryEnemyConfig_ChaseRangeIsZeroOrBeyondAttackRange()
        {
            string[] offenders = LoadEnemyConfigs()
                .Where(config => config.ChaseRange > 0f && config.ChaseRange <= config.AttackRange)
                .Select(config => $"{config.name} (chaseRange {config.ChaseRange}, attackRange {config.AttackRange})")
                .OrderBy(offender => offender, StringComparer.Ordinal)
                .ToArray();

            Assert.That(offenders, Is.Empty,
                "A chase range at or below the attack range never chases: the enemy attacks as soon as the player is inside the " +
                "attack range, so the chase range is never reached first. 0 is the only honest value for 'never chases':\n" +
                string.Join("\n", offenders));
        }

        [Test]
        public void ThrowingEnemyConfigs_HavePositiveRangeCooldownAndTurnSpeed()
        {
            string[] offenders = LoadEnemyConfigs()
                .Where(config => config.Attack == EnemyAttackKind.ThrowProjectile)
                .Where(config => config.AttackRange <= 0f || config.AttackCooldown <= 0f || config.RotationSpeed <= 0f)
                .Select(config => $"{config.name} (attackRange {config.AttackRange}, attackCooldown {config.AttackCooldown}, rotationSpeed {config.RotationSpeed})")
                .OrderBy(offender => offender, StringComparer.Ordinal)
                .ToArray();

            Assert.That(offenders, Is.Empty,
                "A thrower needs a positive attack range (it never attacks otherwise), a positive cooldown (it throws every frame " +
                "otherwise) and a positive rotation speed (0 snaps once when an attack starts and never tracks the player):\n" +
                string.Join("\n", offenders));
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
                SerializedProperty patrolPoints = serializedWalker.FindProperty(EnemyTuningFields.PatrolPoints);

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
                var config = serializedEnemy.FindProperty(EnemyTuningFields.Config).objectReferenceValue as EnemyConfig;

                Assert.That(config, Is.Not.Null, $"{context} has no EnemyConfig assigned.");

                if (config.Attack != EnemyAttackKind.ThrowProjectile)
                    continue;

                SerializedProperty attackSpawn = serializedEnemy.FindProperty(EnemyTuningFields.AttackSpawn);
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
