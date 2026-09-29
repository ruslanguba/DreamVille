using System.Collections.Generic;
using NUnit.Framework;

using DreamVille.App;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.App
{
    /// <summary>
    /// The pause rules: which key opens what, the order of "tell the UI" and "stop time", and how a running
    /// dialogue keeps the player's control off. Menu events and pause calls share one log to pin their order.
    /// </summary>
    public class InGameMenuControllerTests
    {
        private List<string> _log;
        private FakePauseService _pause;
        private FakeGameFlowService _flow;
        private FakeDialogueState _dialogue;
        private FakeMenuInput _input;
        private InGameMenuController _controller;

        [SetUp]
        public void SetUp()
        {
            _log = new List<string>();
            _pause = new FakePauseService(_log);
            _flow = new FakeGameFlowService();
            _dialogue = new FakeDialogueState();
            _input = new FakeMenuInput();
            _controller = new InGameMenuController(_pause, _flow, _dialogue, _input);

            _controller.MenuOpened += () => _log.Add("MenuOpened");
            _controller.MenuClosed += () => _log.Add("MenuClosed");
            _controller.ComicsOpened += () => _log.Add("ComicsOpened");
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public void Initialize_ResumesTheGame()
        {
            _controller.Initialize();

            Assert.That(_log, Is.EqualTo(new[] { "Resume" }));
        }

        [Test]
        public void PausePressed_WhileRunning_RaisesMenuOpenedThenPauses()
        {
            _controller.Initialize();
            _log.Clear();

            _input.PressPause();

            Assert.That(_log, Is.EqualTo(new[] { "MenuOpened", "Pause" }));
        }

        [Test]
        public void PausePressed_WhilePaused_RaisesMenuClosedAndResumes()
        {
            _controller.Initialize();
            _input.PressPause();
            _log.Clear();

            _input.PressPause();

            Assert.That(_log, Is.EqualTo(new[] { "MenuClosed", "Resume" }));
        }

        [Test]
        public void ComicsPressed_WhileRunning_RaisesComicsOpenedAndPauses()
        {
            _controller.Initialize();
            _log.Clear();

            _input.PressComics();

            Assert.That(_log, Is.EqualTo(new[] { "ComicsOpened", "Pause" }));
        }

        [Test]
        public void ContinueGame_DuringDialogue_ClosesMenuButKeepsTimeStopped()
        {
            _controller.Initialize();
            _dialogue.SetActive(true);
            _input.PressPause();
            _log.Clear();

            _controller.ContinueGame();

            Assert.That(_log, Is.EqualTo(new[] { "MenuClosed" }));
            Assert.That(_pause.IsPaused, Is.True);
        }

        [Test]
        public void DialogueActiveChanged_TogglesPlayerControl()
        {
            _controller.Initialize();
            _log.Clear();

            _dialogue.SetActive(true);
            _dialogue.SetActive(false);

            Assert.That(_log, Is.EqualTo(new[] { "SetPlayerControl:false", "SetPlayerControl:true" }));
        }
    }
}
