using System.Collections.Generic;
using NUnit.Framework;

using DreamVille.Dialogue;
using DialogueLine = DreamVille.Dialogue.Dialogue;

namespace DreamVille.Tests.EditMode.Dialogue
{
    public class DialogueServiceTests
    {
        private DialogueService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new DialogueService();
        }

        private static DialogueLine Line(string sentence) => new DialogueLine { Sentence = sentence };

        [Test]
        public void Begin_WithLines_RaisesActiveChangedTrueAndShowsFirstSentence()
        {
            bool? active = null;
            DialogueLine shown = null;
            _service.ActiveChanged += value => active = value;
            _service.SentenceShown += dialogue => shown = dialogue;

            bool started = _service.Begin(new[] { Line("first"), Line("second") });

            Assert.That(started, Is.True);
            Assert.That(active, Is.True);
            Assert.That(shown.Sentence, Is.EqualTo("first"));
        }

        [Test]
        public void Begin_WithNullOrEmptyList_ReturnsFalseAndRaisesNothing()
        {
            bool anyEventRaised = false;
            _service.ActiveChanged += _ => anyEventRaised = true;
            _service.SentenceShown += _ => anyEventRaised = true;

            bool startedWithNull = _service.Begin(null);
            bool startedWithEmpty = _service.Begin(new DialogueLine[0]);

            Assert.That(startedWithNull, Is.False);
            Assert.That(startedWithEmpty, Is.False);
            Assert.That(anyEventRaised, Is.False);
        }

        [Test]
        public void Begin_WhileActive_ReturnsFalseAndDoesNotChangeCurrentQueue()
        {
            var shown = new List<string>();
            _service.SentenceShown += dialogue => shown.Add(dialogue.Sentence);
            _service.Begin(new[] { Line("first"), Line("second") });

            bool startedAgain = _service.Begin(new[] { Line("intruder") });
            _service.ShowNext();

            Assert.That(startedAgain, Is.False);
            Assert.That(shown, Is.EqualTo(new[] { "first", "second" }));
        }

        [Test]
        public void ShowNext_DequeuesInOrder()
        {
            var shown = new List<string>();
            _service.SentenceShown += dialogue => shown.Add(dialogue.Sentence);

            _service.Begin(new[] { Line("first"), Line("second"), Line("third") });
            _service.ShowNext();
            _service.ShowNext();

            Assert.That(shown, Is.EqualTo(new[] { "first", "second", "third" }));
        }

        [Test]
        public void ShowNext_LastLine_EndsDialogueAndInvokesCompletionExactlyOnce()
        {
            int completedCount = 0;
            bool? active = null;
            _service.ActiveChanged += value => active = value;
            _service.SentenceShown += _ => { };

            _service.Begin(new[] { Line("first"), Line("second") }, () => completedCount++);
            _service.ShowNext();
            _service.ShowNext();

            Assert.That(active, Is.False);
            Assert.That(completedCount, Is.EqualTo(1));
        }

        [Test]
        public void ShowNext_WhenInactive_DoesNothing()
        {
            bool anyEventRaised = false;
            _service.ActiveChanged += _ => anyEventRaised = true;
            _service.SentenceShown += _ => anyEventRaised = true;

            _service.ShowNext();

            Assert.That(anyEventRaised, Is.False);
        }
    }
}
