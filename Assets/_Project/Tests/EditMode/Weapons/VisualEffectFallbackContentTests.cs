using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

using DreamVille.Gameplay.Enemies.Combat;
using DreamVille.Gameplay.Rendering;
using DreamVille.Gameplay.Weapons.Bullets;
using DreamVille.Tests.EditMode.Composition;

namespace DreamVille.Tests.EditMode.Weapons
{
    /// <summary>
    /// Visual Effect Graph does not run where there are no compute shaders (WebGL 2). Every prefab must still look right
    /// there: with each <see cref="VisualEffectSupportSwitch"/> told "unsupported", no effect stays active (it would only
    /// log an error) and every projectile keeps a visible renderer. The prefabs are opened in isolation, so no scene is
    /// touched.
    /// </summary>
    public class VisualEffectFallbackContentTests
    {
        private const string PrefabsFolder = "Assets/_Project/Prefabs";

        [Test]
        public void Prefabs_WithoutVisualEffectGraph_KeepNoEffectActive()
        {
            var offenders = new List<string>();

            foreach (string prefabPath in PrefabsWith<VisualEffect>())
            {
                GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
                try
                {
                    ApplyUnsupported(root);
                    offenders.AddRange(root.GetComponentsInChildren<VisualEffect>(true)
                        .Where(effect => effect.gameObject.activeInHierarchy)
                        .Select(effect => $"{prefabPath}: {SceneContentScan.HierarchyPathOf(effect.transform)}"));
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            Assert.That(offenders, Is.Empty,
                "Effects still active without Visual Effect Graph (list them in a VisualEffectSupportSwitch):\n" + string.Join("\n", offenders));
        }

        [Test]
        public void Projectiles_WithoutVisualEffectGraph_StayVisible()
        {
            string[] projectiles = PrefabsWith<Bullet>().Concat(PrefabsWith<EnemyProjectile>()).ToArray();
            Assert.That(projectiles, Is.Not.Empty, "No projectile prefab found.");

            var offenders = new List<string>();

            foreach (string prefabPath in projectiles)
            {
                GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
                try
                {
                    ApplyUnsupported(root);
                    bool visible = root.GetComponentsInChildren<Renderer>(true)
                        .Any(renderer => !(renderer is VFXRenderer) && renderer.enabled && renderer.gameObject.activeInHierarchy);

                    if (!visible)
                        offenders.Add(prefabPath);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            Assert.That(offenders, Is.Empty,
                "Projectiles with no visible renderer without Visual Effect Graph (add a fallback):\n" + string.Join("\n", offenders));
        }

        private static void ApplyUnsupported(GameObject root)
        {
            foreach (VisualEffectSupportSwitch supportSwitch in root.GetComponentsInChildren<VisualEffectSupportSwitch>(true))
                supportSwitch.Apply(visualEffectGraphSupported: false);
        }

        private static IEnumerable<string> PrefabsWith<T>() where T : Component
            => SceneContentScan.AssetPaths("t:Prefab", PrefabsFolder)
                .Where(path =>
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    return prefab != null && prefab.GetComponentInChildren<T>(true) != null;
                });
    }
}
