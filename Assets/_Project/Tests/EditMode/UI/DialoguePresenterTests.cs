using NUnit.Framework;

using DreamVille.Dialogue;
using DreamVille.Tests.EditMode.Fakes;
using DreamVille.UI.Dialogues;

namespace DreamVille.Tests.EditMode.UI
{
    /// <summary>
    /// The dialogue canvas as a table of "service event in, view call out" and "Next in, ShowNext out".
    /// The line queue and its transitions are pinned by <c>DialogueServiceTests</c>.
    /// </summary>
    public class DialoguePresenterTests
    {
        private FakeDialogueService _service;
        private FakeDialogueView _view;
        private FakePauseService _pause;
        private DialoguePresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            _service = new FakeDialogueService();
            _view = new FakeDialogueView();
            _pause = new FakePauseService();
            _presenter = new DialoguePresenter(_service, _view, _pause);
        }

        [TearDown]
        public void TearDown()
        {
            _presenter.Dispose();
        }

        [Test]
        public void ActiveChangedTrue_ShowsView()
        {
            _presenter.Initialize();

            _service.RaiseActiveChanged(true);

            Assert.That(_view.Calls, Is.EqualTo(new[] { "Show" }));
        }

        [Test]
        public void ActiveChangedFalse_HidesView()
        {
            _presenter.Initialize();

            _service.RaiseActiveChanged(false);

            Assert.That(_view.Calls, Is.EqualTo(new[] { "Hide" }));
        }

        [Test]
        public void LineShown_ByPlayer_ShowsOnRight()
        {
            _presenter.Initialize();

            _service.RaiseLineShown(new SpokenLine(DialogueSpeaker.Player, "Hello"));

            Assert.That(_view.Lines, Has.Count.EqualTo(1));
            Assert.That(_view.Lines[0].SpeakerOnRight, Is.True);
            Assert.That(_view.Lines[0].Text, Is.EqualTo("Hello"));
        }

        [Test]
        public void LineShown_ByNpc_ShowsOnLeft()
        {
            _presenter.Initialize();

            _service.RaiseLineShown(new SpokenLine(DialogueSpeaker.Npc, "Hi there"));

            Assert.That(_view.Lines, Has.Count.EqualTo(1));
            Assert.That(_view.Lines[0].SpeakerOnRight, Is.False);
            Assert.That(_view.Lines[0].Text, Is.EqualTo("Hi there"));
        }

        [Test]
        public void NextRequested_AdvancesDialogue()
        {
            _presenter.Initialize();

            _view.PressNext();

            Assert.That(_service.ShowNextCount, Is.EqualTo(1));
        }

        [Test]
        public void NextClicked_WhilePaused_DoesNotAdvance()
        {
            _presenter.Initialize();
            _pause.Pause();

            _view.PressNext();

            Assert.That(_service.ShowNextCount, Is.EqualTo(0));
        }

        [Test]
        public void Dispose_DetachesFromServiceAndView()
        {
            _presenter.Initialize();
            _presenter.Dispose();

            _service.RaiseActiveChanged(true);
            _service.RaiseLineShown(new SpokenLine(DialogueSpeaker.Player, "Hello"));
            _view.PressNext();

            Assert.That(_service.HandlerCount, Is.EqualTo(0));
            Assert.That(_view.NextRequestedHandlerCount, Is.EqualTo(0));
            Assert.That(_view.Calls, Is.Empty);
            Assert.That(_service.ShowNextCount, Is.EqualTo(0));
        }
    }
}
