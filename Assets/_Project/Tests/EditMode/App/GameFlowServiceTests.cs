using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using DreamVille.App;
using DreamVille.Configs;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.App
{
    public class GameFlowServiceTests
    {
        private const string MenuSceneName = "__test_menu";

        private SceneDefinition _menuScene;
        private SceneCatalog _catalog;

        [SetUp]
        public void SetUp()
        {
            _menuScene = SceneDefinition.CreateForTests(MenuSceneName);
            _catalog = SceneCatalog.CreateForTests(null, _menuScene);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_catalog);
            Object.DestroyImmediate(_menuScene);
        }

        [Test]
        public void RestartLevel_ResumesBeforePlacingTheLoadRequest()
        {
            var callLog = new List<string>();
            var pause = new FakePauseService(callLog);
            var loader = new FakeSceneLoader(callLog);
            var flow = new GameFlowService(loader, pause, _catalog);

            flow.RestartLevel();

            Assert.That(callLog, Is.EqualTo(new[] { "Resume", "ReloadActive" }));
        }

        [Test]
        public void ExitToMenu_LoadsTheMenuSceneFromTheCatalog()
        {
            var pause = new FakePauseService();
            var loader = new FakeSceneLoader();
            var flow = new GameFlowService(loader, pause, _catalog);

            flow.ExitToMenu();

            Assert.That(loader.Calls, Is.EqualTo(new[] { "Load:" + MenuSceneName }));
        }
    }
}
