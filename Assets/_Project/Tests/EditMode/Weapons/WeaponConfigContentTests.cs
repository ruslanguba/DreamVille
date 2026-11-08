using NUnit.Framework;
using UnityEditor;
using UnityEngine;

using DreamVille.Configs;

namespace DreamVille.Tests.EditMode.Weapons
{
    /// <summary>
    /// Content guards: pins the shipped <see cref="BulletConfig"/> and
    /// <see cref="PillowConfig"/> assets so a designer edit cannot desync the bullet's collider
    /// radius from the aim preview's sweep, or ship a pillow tuning that stalls the recall loop.
    /// </summary>
    public class WeaponConfigContentTests
    {
        private const string BulletConfigPath = "Assets/_Project/Settings/Bullet/BulletConfig.asset";
        private const string PillowConfigPath = "Assets/_Project/Settings/Weapons/PillowConfig.asset";
        private const string BulletPrefabPath = "Assets/_Project/Prefabs/Bullet.prefab";

        [Test]
        public void BulletConfigAsset_RadiusMatchesBulletPrefabCollider()
        {
            var config = AssetDatabase.LoadAssetAtPath<BulletConfig>(BulletConfigPath);
            Assert.That(config, Is.Not.Null, $"{BulletConfigPath} is missing.");

            var bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BulletPrefabPath);
            Assert.That(bulletPrefab, Is.Not.Null, $"{BulletPrefabPath} is missing.");

            var collider = bulletPrefab.GetComponent<SphereCollider>();
            Assert.That(collider, Is.Not.Null, $"{BulletPrefabPath} has no SphereCollider.");

            Assert.That(config.CollisionRadius, Is.EqualTo(collider.radius).Within(0.0001f),
                "BulletConfig.CollisionRadius must match Bullet.prefab's SphereCollider radius, or the aim preview sweeps a different sphere than the bullet.");
        }

        [Test]
        public void PillowConfigAsset_IsValid()
        {
            var config = AssetDatabase.LoadAssetAtPath<PillowConfig>(PillowConfigPath);
            Assert.That(config, Is.Not.Null, $"{PillowConfigPath} is missing.");

            Assert.That(config.IsValid(out string error), Is.True, error);
        }
    }
}
