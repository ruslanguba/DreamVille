using NUnit.Framework;
using UnityEditor;
using UnityEngine;

using DreamVille.Bootstrap;
using DreamVille.Configs;
using DreamVille.UI;
using DreamVille.UI.Hud;

namespace DreamVille.Tests.EditMode.UI
{
    /// <summary>
    /// Guards spec 09 §5 group H: the HUD prefab still carries every view after the views were renamed and
    /// moved, and its content matches the rules the presenter indexes into (stars per level, one ricochet
    /// badge per hit count). The views ignore out-of-range indices, so a mismatch would otherwise be silent.
    /// </summary>
    public class HudPrefabContentTests
    {
        private const string HudPrefabPath = "Assets/_Project/Prefabs/UI/GamePlayHUDCanvas.prefab";
        private const string SceneContextPrefabPath = "Assets/_Project/Prefabs/Bootstrap/GameplaySceneContext.prefab";
        private const string CatalogAssetPath = "Assets/_Project/Settings/Levels/SceneCatalog.asset";
        private const string BulletConfigPath = "Assets/_Project/Settings/Bullet/BulletConfig.asset";

        private static readonly string[] HudBlockFields = { "_ricochet", "_pillows", "_enemyCounter", "_scorePopup", "_stars" };

        [Test]
        public void HudPrefab_GameUIRoot_HasEveryViewAssigned()
        {
            GameObject hud = LoadHudPrefab();
            var root = hud.GetComponent<GameUIRoot>();
            Assert.That(root, Is.Not.Null, $"{HudPrefabPath} root has no {nameof(GameUIRoot)}.");

            var serialized = new SerializedObject(root);

            foreach (string field in HudBlockFields)
            {
                var block = serialized.FindProperty(field).objectReferenceValue as Component;

                Assert.That(block, Is.Not.Null, $"{nameof(GameUIRoot)}.{field} is not assigned.");
                Assert.That(block.transform.root, Is.SameAs(hud.transform), $"{nameof(GameUIRoot)}.{field} points outside the HUD prefab.");
            }
        }

        [Test]
        public void HudPrefab_StarCount_MatchesEveryLevelsThresholdCount()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<SceneCatalog>(CatalogAssetPath);
            Assert.That(catalog, Is.Not.Null, $"{CatalogAssetPath} is missing.");

            var stars = LoadHudPrefab().GetComponentInChildren<StarsView>(true);
            Assert.That(stars, Is.Not.Null, $"{HudPrefabPath} has no {nameof(StarsView)}.");

            int starCount = new SerializedObject(stars).FindProperty("_scoreStars").arraySize;

            Assert.That(catalog.Levels, Is.Not.Empty, "The catalog lists no levels.");
            foreach (LevelDefinition level in catalog.Levels)
            {
                Assert.That(level.StarRules, Is.Not.Null, $"Level '{level.LevelId}' has no StarRules.");
                Assert.That(starCount, Is.EqualTo(level.StarRules.Thresholds.Count),
                    $"HUD star count vs the threshold count of level '{level.LevelId}'.");
            }
        }

        [Test]
        public void HudPrefab_RicochetBadges_CoverEveryHitCountFromTwoToMax()
        {
            var bulletConfig = AssetDatabase.LoadAssetAtPath<BulletConfig>(BulletConfigPath);
            Assert.That(bulletConfig, Is.Not.Null, $"{BulletConfigPath} is missing.");

            var ricochet = LoadHudPrefab().GetComponentInChildren<RicochetView>(true);
            Assert.That(ricochet, Is.Not.Null, $"{HudPrefabPath} has no {nameof(RicochetView)}.");

            int badgeCount = new SerializedObject(ricochet).FindProperty("_images").arraySize;

            Assert.That(badgeCount, Is.EqualTo(bulletConfig.MaxCollisionCount - HudPresenter.FirstBadgeHitCount + 1),
                "One badge per ricochet count from the first badge up to BulletConfig.MaxCollisionCount.");
        }

        [Test]
        public void UiInstaller_HudPrefab_ProvidesIGameHudView()
        {
            var context = AssetDatabase.LoadAssetAtPath<GameObject>(SceneContextPrefabPath);
            Assert.That(context, Is.Not.Null, $"{SceneContextPrefabPath} is missing.");

            var installer = context.GetComponentInChildren<UiInstaller>(true);
            Assert.That(installer, Is.Not.Null, $"{SceneContextPrefabPath} has no {nameof(UiInstaller)}.");

            var hudRoot = new SerializedObject(installer).FindProperty("_uiPrefab").objectReferenceValue as Component;
            Assert.That(hudRoot, Is.Not.Null, $"{nameof(UiInstaller)}._uiPrefab is not assigned.");

            Assert.That(hudRoot.GetComponent<IGameHudView>(), Is.Not.Null,
                $"The prefab behind {nameof(UiInstaller)}._uiPrefab has no {nameof(IGameHudView)} component.");
        }

        [Test]
        public void ParticleGraphics_AreNotRaycastTargets()
        {
            UIParticleSystem[] graphics = LoadHudPrefab().GetComponentsInChildren<UIParticleSystem>(true);

            Assert.That(graphics, Is.Not.Empty, $"{HudPrefabPath} has no {nameof(UIParticleSystem)}.");
            foreach (UIParticleSystem graphic in graphics)
                Assert.That(graphic.raycastTarget, Is.False, $"{graphic.name} would swallow clicks meant for the buttons behind it.");
        }

        private static GameObject LoadHudPrefab()
        {
            var hud = AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath);
            Assert.That(hud, Is.Not.Null, $"{HudPrefabPath} is missing.");
            return hud;
        }
    }
}
