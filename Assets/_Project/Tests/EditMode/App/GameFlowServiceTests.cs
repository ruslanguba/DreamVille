using System.Collections.Generic;
using NUnit.Framework;

using DreamVille.App;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.App
{
    public class GameFlowServiceTests
    {
        [Test]
        public void RestartLevel_ResumesBeforePlacingTheLoadRequest()
        {
            var callLog = new List<string>();
            var pause = new FakePauseService(callLog);
            var loader = new FakeSceneLoader(callLog);
            var flow = new GameFlowService(loader, pause, "Main menu 1");

            flow.RestartLevel();

            Assert.That(callLog, Is.EqualTo(new[] { "Resume", "ReloadActive" }));
        }

        [Test]
        public void ExitToMenu_LoadsTheConfiguredMenuScene()
        {
            var pause = new FakePauseService();
            var loader = new FakeSceneLoader();
            var flow = new GameFlowService(loader, pause, "Main menu 1");

            flow.ExitToMenu();

            Assert.That(loader.Calls, Is.EqualTo(new[] { "Load:Main menu 1" }));
        }
    }
}
