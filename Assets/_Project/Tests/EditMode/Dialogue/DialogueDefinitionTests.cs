using System.Linq;
using NUnit.Framework;
using UnityEngine.Localization;

using DreamVille.Dialogue;

namespace DreamVille.Tests.EditMode.Dialogue
{
    public class DialogueDefinitionTests
    {
        private DialogueDefinition _definition;

        [TearDown]
        public void TearDown()
        {
            if (_definition != null)
                UnityEngine.Object.DestroyImmediate(_definition);
        }

        [Test]
        public void DialogueSpeaker_ValuesMatchLegacyIsPluhFlag()
        {
            Assert.That((int)DialogueSpeaker.Npc, Is.EqualTo(0));
            Assert.That((int)DialogueSpeaker.Player, Is.EqualTo(1));
        }

        [Test]
        public void Create_ExposesLinesInOrder()
        {
            _definition = DialogueDefinition.Create(
                new DialogueLine(DialogueSpeaker.Npc, new LocalizedString("Dialogues", "first")),
                new DialogueLine(DialogueSpeaker.Player, new LocalizedString("Dialogues", "second")));

            Assert.That(_definition.Lines.Select(line => line.Speaker), Is.EqualTo(new[] { DialogueSpeaker.Npc, DialogueSpeaker.Player }));
            Assert.That(_definition.Lines.Select(line => line.Text.TableEntryReference.Key), Is.EqualTo(new[] { "first", "second" }));
        }
    }
}
