using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.TestTools;

using DreamVille.Dialogue;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Dialogue
{
    /// <summary>
    /// The trigger's half of the event contract: DialogueStarted in the accepting call,
    /// DialogueEnded once the service completes, nothing when it may not start. deleteOnEnd stays off here:
    /// Destroy is not allowed in edit mode, and the smoke runs cover it.
    /// </summary>
    public class DialogueTriggerTests
    {
        private readonly List<string> _events = new List<string>();

        private GameObject _gameObject;
        private DialogueTrigger _trigger;
        private FakeDialogueService _service;
        private DialogueDefinition _definition;

        [SetUp]
        public void SetUp()
        {
            _events.Clear();
            _service = new FakeDialogueService();
            _service.ActiveChanged += isActive => _events.Add($"ActiveChanged:{isActive}");

            _definition = DialogueDefinition.Create(new DialogueLine(DialogueSpeaker.Npc, new LocalizedString("Dialogues", "line")));

            _gameObject = new GameObject("Trigger");
            _trigger = _gameObject.AddComponent<DialogueTrigger>();
            _trigger.Construct(_service);
            _trigger.DialogueStarted += () => _events.Add("DialogueStarted");
            _trigger.DialogueEnded += () => _events.Add("DialogueEnded");
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_gameObject);
            UnityEngine.Object.DestroyImmediate(_definition);
        }

        private void AssignDefinition(DialogueDefinition definition)
        {
            var serialized = new SerializedObject(_trigger);
            serialized.FindProperty("_definition").objectReferenceValue = definition;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [Test]
        public void StartDialogue_WhenAccepted_StartsItsDefinitionAndRaisesStartedInTheSameCall()
        {
            AssignDefinition(_definition);

            _trigger.StartDialogue();

            Assert.That(_service.StartCount, Is.EqualTo(1));
            Assert.That(_service.LastStartedDefinition, Is.SameAs(_definition));
            Assert.That(_events, Is.EqualTo(new[] { "ActiveChanged:True", "DialogueStarted" }));
        }

        [Test]
        public void ServiceCompletes_RaisesDialogueEndedAfterActiveChangedFalse()
        {
            AssignDefinition(_definition);
            _trigger.StartDialogue();

            _service.Finish();

            Assert.That(_events, Is.EqualTo(new[] { "ActiveChanged:True", "DialogueStarted", "ActiveChanged:False", "DialogueEnded" }));
        }

        [Test]
        public void StartDialogue_WhenDisarmed_DoesNotStart()
        {
            AssignDefinition(_definition);
            _trigger.IsArmed = false;

            _trigger.StartDialogue();

            Assert.That(_service.StartCount, Is.Zero);
            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void StartDialogue_WithoutDefinition_LogsErrorAndDoesNotStart()
        {
            LogAssert.Expect(LogType.Error, new Regex("has no dialogue definition assigned"));

            _trigger.StartDialogue();

            Assert.That(_service.StartCount, Is.Zero);
            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void StartDialogue_WhileAnotherDialogueRuns_LogsWarningAndDoesNotStart()
        {
            AssignDefinition(_definition);
            _service.RaiseActiveChanged(true);
            _events.Clear();
            LogAssert.Expect(LogType.Warning, new Regex("a dialogue is already active"));

            _trigger.StartDialogue();

            Assert.That(_service.StartCount, Is.Zero);
            Assert.That(_events, Is.Empty);
        }

        [Test]
        public void StartedListenerThrows_DialogueStillEnds()
        {
            AssignDefinition(_definition);
            _trigger.DialogueStarted += () => throw new InvalidOperationException("listener failure");
            LogAssert.Expect(LogType.Exception, new Regex("listener failure"));
            _trigger.StartDialogue();

            _service.Finish();

            Assert.That(_events, Does.Contain("DialogueEnded"));
        }

        [Test]
        public void DialogueStarted_WhenAnEarlierListenerThrows_StillReachesTheLaterOnes()
        {
            AssignDefinition(_definition);
            _trigger.DialogueStarted += () => throw new InvalidOperationException("listener failure");
            _trigger.DialogueStarted += () => _events.Add("LaterDialogueStarted");
            LogAssert.Expect(LogType.Exception, new Regex("listener failure"));

            _trigger.StartDialogue();

            Assert.That(_events, Does.Contain("LaterDialogueStarted"));
        }

        [Test]
        public void DialogueEnded_WhenAnEarlierListenerThrows_StillReachesTheLaterOnes()
        {
            AssignDefinition(_definition);
            _trigger.DialogueEnded += () => throw new InvalidOperationException("listener failure");
            _trigger.DialogueEnded += () => _events.Add("LaterDialogueEnded");
            LogAssert.Expect(LogType.Exception, new Regex("listener failure"));
            _trigger.StartDialogue();

            _service.Finish();

            Assert.That(_events, Does.Contain("LaterDialogueEnded"));
        }
    }
}
