using NUnit.Framework;
using UnityEngine;
using Zenject;

using DreamVille.Configs;
using DreamVille.Gameplay.Enemies;
using DreamVille.Persistence;
using DreamVille.Progression;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Progression
{
    /// <summary>
    /// LevelActors is a hard requirement of LevelDirector: an optional cast once vanished from every gameplay
    /// scene and the finale silently stopped working. Composition must now fail loudly instead.
    /// </summary>
    public class LevelDirectorCompositionTests
    {
        private LevelDefinition _level;

        [SetUp]
        public void SetUp()
        {
            _level = LevelDefinition.CreateForTests("Level_Test", "test");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_level);
        }

        [Test]
        public void Resolve_WithoutLevelActors_Throws()
        {
            var container = new DiContainer();
            container.Bind<IEnemyRegistry>().FromInstance(new FakeEnemyRegistry());
            container.Bind<LevelDefinition>().FromInstance(_level);
            container.Bind<IProgressRepository>().FromInstance(new ProgressRepository(new InMemorySaveStorage()));
            container.Bind<LevelDirector>().AsSingle();

            Assert.Throws<ZenjectException>(() => container.Resolve<LevelDirector>());
        }
    }
}
