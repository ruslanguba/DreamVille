using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

using DreamVille.Dialogue;

namespace DreamVille.Tests.EditMode.Dialogue
{
    /// <summary>
    /// Editor-side view of the "Dialogues" string table collection. Opening a scene unloads unused assets,
    /// so a test must call <see cref="Load"/> after the last scene it opens and never keep the instance
    /// (or its tables) across a scene change.
    /// </summary>
    internal sealed class DialoguesTable
    {
        public const string CollectionName = "Dialogues";

        private DialoguesTable(StringTableCollection collection, StringTable russian, StringTable english)
        {
            Collection = collection;
            Russian = russian;
            English = english;
        }

        public StringTableCollection Collection { get; }

        public StringTable Russian { get; }

        public StringTable English { get; }

        public SharedTableData SharedData => Collection.SharedData;

        public IEnumerable<StringTable> AllTables => Collection.StringTables;

        public static DialoguesTable Load()
        {
            StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(CollectionName);
            Assert.That(collection, Is.Not.Null, $"The string table collection '{CollectionName}' does not exist.");

            var russian = collection.GetTable(new LocaleIdentifier("ru")) as StringTable;
            var english = collection.GetTable(new LocaleIdentifier("en")) as StringTable;
            Assert.That(russian, Is.Not.Null, $"'{CollectionName}' has no Russian table.");
            Assert.That(english, Is.Not.Null, $"'{CollectionName}' has no English table.");

            return new DialoguesTable(collection, russian, english);
        }

        /// <summary>True when the line's text points into this collection (not into UI Text or another table).</summary>
        public bool Contains(DialogueLine line)
        {
            TableReference reference = line.Text.TableReference;

            return reference.ReferenceType != TableReference.Type.Empty
                && LocalizationEditorSettings.GetStringTableCollection(reference) == Collection;
        }

        /// <summary>The shared key the line points at, or null when the key is not in the collection.</summary>
        public SharedTableData.SharedTableEntry KeyOf(DialogueLine line)
            => SharedData.GetEntryFromReference(line.Text.TableEntryReference);

        public StringTableEntry RussianEntryOf(DialogueLine line) => Russian.GetEntryFromReference(line.Text.TableEntryReference);

        /// <summary>Null when the line has no English entry (deliberately absent, so fallback can act).</summary>
        public StringTableEntry EnglishEntryOf(DialogueLine line) => English.GetEntryFromReference(line.Text.TableEntryReference);
    }
}
