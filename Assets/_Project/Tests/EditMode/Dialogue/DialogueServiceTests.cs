using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.TestTools;

using DreamVille.Dialogue;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Dialogue
{
    /// <summary>
    /// The run contract of <see cref="DialogueService"/>: synchronous acceptance and end, line order,
    /// text resolution and cancellation. Every run uses a cancellable token, like the trigger's destroy token.
    /// </summary>
    public class DialogueServiceTests
    {
        private const string Table = "Dialogues";

        private readonly List<DialogueDefinition> _definitions = new List<DialogueDefinition>();
        private readonly List<string> _events = new List<string>();
        private readonly List<SpokenLine> _shown = new List<SpokenLine>();

        private FakeLocalizedTextProvider _provider;
        private DialogueService _service;
        private CancellationTokenSource _cancellation;

        [SetUp]
        public void SetUp()
        {
            _events.Clear();
            _shown.Clear();

            _provider = new FakeLocalizedTextProvider();
            _service = new DialogueService(_provider);
            _cancellation = new CancellationTokenSource();

            _service.ActiveChanged += isActive => _events.Add($"ActiveChanged:{isActive}");
            _service.LineShown += line =>
            {
                _shown.Add(line);
                _events.Add($"Line:{line.Text}");
            };
        }

        [TearDown]
        public void TearDown()
        {
            _cancellation.Dispose();

            foreach (DialogueDefinition definition in _definitions)
                UnityEngine.Object.DestroyImmediate(definition);

            _definitions.Clear();
        }

        private static DialogueLine Line(string key, DialogueSpeaker speaker = DialogueSpeaker.Npc)
            => new DialogueLine(speaker, new LocalizedString(Table, key));

        private static string TextOf(string key) => FakeLocalizedTextProvider.TextFor(Table, key);

        private DialogueDefinition Define(params DialogueLine[] lines)
        {
            DialogueDefinition definition = DialogueDefinition.Create(lines);
            _definitions.Add(definition);
            return definition;
        }

        private UniTask Start(params DialogueLine[] lines)
        {
            _service.TryStart(Define(lines), _cancellation.Token, out UniTask completion);
            return completion;
        }

        // Appends "Completed" or "Canceled" to the event log at the moment the run's task finishes.
        private void RecordOutcome(UniTask completion) => RecordOutcomeAsync(completion).Forget();

        private async UniTaskVoid RecordOutcomeAsync(UniTask completion)
        {
            try
            {
                await completion;
                _events.Add("Completed");
            }
            catch (OperationCanceledException)
            {
                _events.Add("Canceled");
            }
        }

        [Test]
        public void TryStart_WhenIdle_ReturnsTrueAndRaisesActiveChangedTrue()
        {
            bool started = _service.TryStart(Define(Line("a")), _cancellation.Token, out _);

            Assert.That(started, Is.True);
            Assert.That(_service.IsActive, Is.True);
            Assert.That(_events, Is.EqualTo(new[] { "ActiveChanged:True", $"Line:{TextOf("a")}" }));
        }

        [Test]
        public void TryStart_WhenActive_ReturnsFalseAndKeepsCurrentRun()
        {
            _service.TryStart(Define(Line("first"), Line("second")), _cancellation.Token, out _);

            bool startedAgain = _service.TryStart(Define(Line("intruder")), _cancellation.Token, out _);
            _service.ShowNext();

            Assert.That(startedAgain, Is.False);
            Assert.That(_shown.Select(line => line.Text), Is.EqualTo(new[] { TextOf("first"), TextOf("second") }));
        }

        [Test]
        public void TryStart_WithNullDefinition_ReturnsFalseAndStaysIdle()
        {
            bool started = _service.TryStart(null, _cancellation.Token, out _);

            Assert.That(started, Is.False);
            Assert.That(_service.IsActive, Is.False);
            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void TryStart_WithEmptyDefinition_ReturnsFalseAndStaysIdle()
        {
            bool started = _service.TryStart(Define(), _cancellation.Token, out _);

            Assert.That(started, Is.False);
            Assert.That(_service.IsActive, Is.False);
            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void TryStart_ShowsFirstLineWithResolvedTextAndSpeaker()
        {
            _service.TryStart(Define(Line("hello", DialogueSpeaker.Player), Line("bye")), _cancellation.Token, out _);

            Assert.That(_shown, Has.Count.EqualTo(1));
            Assert.That(_shown[0].Speaker, Is.EqualTo(DialogueSpeaker.Player));
            Assert.That(_shown[0].Text, Is.EqualTo(TextOf("hello")));
        }

        [Test]
        public void ShowNext_ShowsLinesInDefinitionOrder()
        {
            Start(Line("first"), Line("second", DialogueSpeaker.Player), Line("third"));

            _service.ShowNext();
            _service.ShowNext();

            Assert.That(_shown.Select(line => line.Text), Is.EqualTo(new[] { TextOf("first"), TextOf("second"), TextOf("third") }));
            Assert.That(_shown.Select(line => line.Speaker), Is.EqualTo(new[] { DialogueSpeaker.Npc, DialogueSpeaker.Player, DialogueSpeaker.Npc }));
        }

        [Test]
        public void ShowNext_AfterLastLine_RaisesActiveChangedFalseThenCompletes()
        {
            RecordOutcome(Start(Line("only")));

            _service.ShowNext();

            Assert.That(_events, Is.EqualTo(new[] { "ActiveChanged:True", $"Line:{TextOf("only")}", "ActiveChanged:False", "Completed" }));
        }

        [Test]
        public void ShowNext_AfterLastLine_CompletesSynchronously()
        {
            UniTask completion = Start(Line("only"));

            _service.ShowNext();

            Assert.That(completion.Status, Is.EqualTo(UniTaskStatus.Succeeded));
        }

        [Test]
        public void ShowNext_WhenIdle_DoesNothing()
        {
            _service.ShowNext();

            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void ShowNext_WhileTextsResolve_IsIgnored()
        {
            _provider.Deferred = true;
            Start(Line("first"), Line("second"));

            _service.ShowNext();
            _provider.CompletePending();

            Assert.That(_shown.Select(line => line.Text), Is.EqualTo(new[] { TextOf("first") }));
            Assert.That(_service.IsActive, Is.True);
        }

        [Test]
        public void Cancel_DuringRun_EndsRunAndCompletionThrowsCanceled()
        {
            RecordOutcome(Start(Line("first"), Line("second")));

            _cancellation.Cancel();

            Assert.That(_service.IsActive, Is.False);
            Assert.That(_events, Is.EqualTo(new[] { "ActiveChanged:True", $"Line:{TextOf("first")}", "ActiveChanged:False", "Canceled" }));
        }

        [Test]
        public void Cancel_DuringRun_AllowsNextStart()
        {
            Start(Line("first"));
            _cancellation.Cancel();

            bool startedAgain = _service.TryStart(Define(Line("second")), CancellationToken.None, out _);

            Assert.That(startedAgain, Is.True);
            Assert.That(_service.IsActive, Is.True);
        }

        [Test]
        public void Cancel_WhileTextsResolve_EndsRunEvenIfProviderIgnoresTheToken()
        {
            _provider.Deferred = true;
            RecordOutcome(Start(Line("first")));

            _cancellation.Cancel();

            Assert.That(_events, Is.EqualTo(new[] { "ActiveChanged:True", "ActiveChanged:False", "Canceled" }));
        }

        [Test]
        public void TryStart_WithCancelledToken_ReturnsFalseAndStaysIdle()
        {
            _cancellation.Cancel();

            bool started = _service.TryStart(Define(Line("first")), _cancellation.Token, out UniTask _);

            Assert.That(started, Is.False);
            Assert.That(_service.IsActive, Is.False);
            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void LineShownListener_DismissingItsLine_ShowsTheNextOne()
        {
            bool dismissed = false;
            _service.LineShown += line =>
            {
                if (dismissed)
                    return;

                dismissed = true;
                _service.ShowNext();
            };

            Start(Line("first"), Line("second"));

            Assert.That(_shown.Select(line => line.Text), Is.EqualTo(new[] { TextOf("first"), TextOf("second") }));
            Assert.That(_service.IsActive, Is.True);
        }

        [Test]
        public void ActiveChangedFalseListener_StartingTheNextRun_IsAccepted()
        {
            DialogueDefinition next = Define(Line("next"));
            bool nextStarted = false;
            _service.ActiveChanged += isActive =>
            {
                if (!isActive && !nextStarted)
                    nextStarted = _service.TryStart(next, _cancellation.Token, out UniTask _);
            };
            RecordOutcome(Start(Line("first")));

            _service.ShowNext();

            Assert.That(nextStarted, Is.True);
            Assert.That(_service.IsActive, Is.True);
            Assert.That(_events, Is.EqualTo(new[]
            {
                "ActiveChanged:True", $"Line:{TextOf("first")}", "ActiveChanged:False",
                "ActiveChanged:True", $"Line:{TextOf("next")}", "Completed"
            }));
        }

        [Test]
        public void ProviderThrows_EndsRunAndLogsError()
        {
            _provider.Failure = new InvalidOperationException("table is missing");
            LogAssert.Expect(LogType.Error, new Regex("Could not resolve"));

            RecordOutcome(Start(Line("first")));

            Assert.That(_service.IsActive, Is.False);
            Assert.That(_events, Is.EqualTo(new[] { "ActiveChanged:True", "ActiveChanged:False", "Completed" }));
        }

        [Test]
        public void RunAsync_WhenRejected_ReturnsFalse()
        {
            Start(Line("running"));

            UniTask<bool> run = _service.RunAsync(Define(Line("intruder")), _cancellation.Token);

            Assert.That(run.GetAwaiter().GetResult(), Is.False);
        }

        [Test]
        public void RunAsync_WhenCompleted_ReturnsTrue()
        {
            UniTask<bool> run = _service.RunAsync(Define(Line("only")), _cancellation.Token);

            _service.ShowNext();

            Assert.That(run.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            Assert.That(run.GetAwaiter().GetResult(), Is.True);
        }

        [Test]
        public void Texts_AreResolvedOncePerRun()
        {
            Start(Line("first"), Line("second"), Line("third"));

            _service.ShowNext();
            _service.ShowNext();
            _service.ShowNext();

            Assert.That(_service.IsActive, Is.False);
            Assert.That(_provider.CallCount, Is.EqualTo(3));
        }
    }
}
