using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

using DreamVille.Configs;

namespace DreamVille.Tests.EditMode.Configs
{
    /// <summary>
    /// Lookup rules of a hand-built catalog. The tests over the shipped
    /// Settings/Levels/SceneCatalog.asset live in the composition fixture.
    /// </summary>
    public class SceneCatalogTests
    {
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

        private T Track<T>(T asset) where T : Object
        {
            _created.Add(asset);
            return asset;
        }
    }
}
