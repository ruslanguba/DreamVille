using System;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using DreamVille.Bootstrap;
using DreamVille.Configs;
using DreamVille.Tests.EditMode.Fakes;
using DreamVille.UI;

using Object = UnityEngine.Object;

namespace DreamVille.Tests.EditMode.Bootstrap
{
    /// <summary>The game entry point: the intro is decoration, so whatever happens to it the menu still loads.</summary>
    public class GameBootstrapTests
    {
        private const string MenuSceneName = "MainMenu";

        private SceneDefinition _menuScene;
        private SceneCatalog _catalog;
        private FakeSceneLoader _loader;

        [SetUp]
        public void SetUp()
        {
            _menuScene = SceneDefinition.CreateForTests(MenuSceneName);
            _catalog = SceneCatalog.CreateForTests(null, _menuScene);
            _loader = new FakeSceneLoader();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_catalog);
            Object.DestroyImmediate(_menuScene);
        }

        [Test]
        public void Initialize_AfterTheIntro_LoadsTheMenu()
        {
            var bootstrap = new GameBootstrap(_loader, _catalog, new ScriptedIntro(null));

            bootstrap.Initialize();
            bootstrap.Dispose();

            Assert.That(_loader.Calls, Is.EqualTo(new[] { $"Load:{MenuSceneName}" }));
        }

        [Test]
        public void Initialize_WhenIntroThrows_LogsAndStillLoadsTheMenu()
        {
            var bootstrap = new GameBootstrap(_loader, _catalog, new ScriptedIntro(new InvalidOperationException("intro failure")));
            LogAssert.Expect(LogType.Exception, new Regex("intro failure"));

            bootstrap.Initialize();
            bootstrap.Dispose();

            Assert.That(_loader.Calls, Is.EqualTo(new[] { $"Load:{MenuSceneName}" }));
        }

        private sealed class ScriptedIntro : IGameIntro
        {
            private readonly Exception _failure;

            public ScriptedIntro(Exception failure)
            {
                _failure = failure;
            }

            public UniTask PlayAsync(CancellationToken token)
                => _failure == null ? UniTask.CompletedTask : UniTask.FromException(_failure);
        }
    }
}
