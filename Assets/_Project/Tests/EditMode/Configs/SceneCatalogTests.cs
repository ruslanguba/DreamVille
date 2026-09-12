using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

using DreamVille.Configs;

namespace DreamVille.Tests.EditMode.Configs
{
    /// <summary>
    /// Lookup rules of a hand-built catalog, plus a guard over the shipped
    /// Settings/Levels/SceneCatalog.asset: a scene rename that forgets the catalog (or a
    /// catalog entry that outlives a deleted scene) becomes a red test here instead of a
    /// runtime crash at a portal.
    /// </summary>
    public class SceneCatalogTests
    {
        private const string RealCatalogAssetPath = "Assets/_Project/Settings/Levels/SceneCatalog.asset";

        private readonly List<Object> _created = new List<Object>();

        private SceneCatalog _catalog;

        [SetUp]
        public void SetUp()
        {
            SceneDefinition bootstrap = Track(SceneDefinition.CreateForTests("Boot"));
            SceneDefinition menu = Track(SceneDefinition.CreateForTests("Menu"));
            LevelDefinition first = Track(LevelDefinition.CreateForTests("Level_01", "1", unlockedByDefault: true));
            LevelDefinition second = Track(LevelDefinition.CreateForTests("Level_02", "2"));

            _catalog = Track(SceneCatalog.CreateForTests(bootstrap, menu, first, second));
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object asset in _created)
                Object.DestroyImmediate(asset);

            _created.Clear();
        }

        [Test]
        public void FindBySceneName_ReturnsTheMatchingDefinition()
        {
            SceneDefinition menu = _catalog.FindBySceneName("Menu");
            SceneDefinition level = _catalog.FindBySceneName("Level_02");

            Assert.That(menu, Is.SameAs(_catalog.MainMenuScene));
            Assert.That(level, Is.SameAs(_catalog.Levels[1]));
        }

        [Test]
        public void FindBySceneName_Unknown_ReturnsNull()
        {
            Assert.That(_catalog.FindBySceneName("NoSuchScene"), Is.Null);
        }

        [Test]
        public void FindLevelById_ReturnsTheLevelWithThatSaveId()
        {
            Assert.That(_catalog.FindLevelById("2"), Is.SameAs(_catalog.Levels[1]));
            Assert.That(_catalog.FindLevelById("42"), Is.Null);
        }

        [Test]
        public void LevelIds_AreUniqueAndNonEmpty()
        {
            IReadOnlyList<LevelDefinition> levels = _catalog.Levels;

            foreach (LevelDefinition level in levels)
                Assert.That(level.LevelId, Is.Not.Null.And.Not.Empty, $"{level.SceneName} has no level id.");

            Assert.That(levels.Select(level => level.LevelId).Distinct().Count(), Is.EqualTo(levels.Count));
        }

        [Test]
        public void LevelIds_OnTheRealCatalog_AreUniqueAndNonEmpty()
        {
            IReadOnlyList<LevelDefinition> levels = LoadRealCatalog().Levels;

            foreach (LevelDefinition level in levels)
                Assert.That(level.LevelId, Is.Not.Null.And.Not.Empty, $"{level.SceneName} has no level id.");

            Assert.That(levels.Select(level => level.LevelId).Distinct().Count(), Is.EqualTo(levels.Count));
        }

        [Test]
        public void EverySceneName_IsAnEnabledSceneInBuildSettings()
        {
            SceneCatalog catalog = LoadRealCatalog();
            List<string> enabledSceneNames = EnabledBuildSceneNames().ToList();

            var catalogSceneNames = new List<string>
            {
                catalog.BootstrapScene.SceneName,
                catalog.MainMenuScene.SceneName
            };
            catalogSceneNames.AddRange(catalog.Levels.Select(level => level.SceneName));

            foreach (string sceneName in catalogSceneNames)
            {
                Assert.That(enabledSceneNames, Does.Contain(sceneName),
                    $"Catalog references scene '{sceneName}', which is not an enabled scene in Build Settings.");
            }
        }

        [Test]
        public void EveryEnabledGameplayScene_HasACatalogEntry()
        {
            SceneCatalog catalog = LoadRealCatalog();
            var knownSceneNames = new HashSet<string>
            {
                catalog.BootstrapScene.SceneName,
                catalog.MainMenuScene.SceneName
            };

            foreach (LevelDefinition level in catalog.Levels)
                knownSceneNames.Add(level.SceneName);

            foreach (string sceneName in EnabledBuildSceneNames())
            {
                Assert.That(knownSceneNames, Does.Contain(sceneName),
                    $"Enabled scene '{sceneName}' is not the bootstrap, the menu or a Levels entry of the catalog.");
            }
        }

        private static SceneCatalog LoadRealCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<SceneCatalog>(RealCatalogAssetPath);

            Assert.That(catalog, Is.Not.Null, $"{RealCatalogAssetPath} is missing.");

            return catalog;
        }

        private static IEnumerable<string> EnabledBuildSceneNames()
            => EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => Path.GetFileNameWithoutExtension(scene.path));

        private T Track<T>(T asset) where T : Object
        {
            _created.Add(asset);
            return asset;
        }
    }
}
