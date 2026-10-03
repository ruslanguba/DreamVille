using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

using DreamVille.Dialogue;

namespace DreamVille.Tests.EditMode.Dialogue
{
    /// <summary>One line of the checked-in dialogue snapshot, already unescaped.</summary>
    internal sealed class SnapshotRow
    {
        public SnapshotRow(string source, string triggerPath, int index, DialogueSpeaker speaker, string russian, string english)
        {
            Source = source;
            TriggerPath = triggerPath;
            Index = index;
            Speaker = speaker;
            Russian = russian;
            English = english;
        }

        /// <summary>Scene name, or <see cref="DialogueTriggerScan.PrefabSource"/> for a prefab asset.</summary>
        public string Source { get; }

        /// <summary>Hierarchy path of the trigger from the scene root (or the prefab root).</summary>
        public string TriggerPath { get; }

        public int Index { get; }

        public DialogueSpeaker Speaker { get; }

        public string Russian { get; }

        /// <summary>Null when the snapshot has no English text for the line.</summary>
        public string English { get; }
    }

    /// <summary>
    /// Reads <c>DialogueSnapshot.tsv</c>: <c>source, triggerPath, index, speaker, ru, en</c> separated by tabs,
    /// with <c>\\</c>, <c>\t</c>, <c>\r</c> and <c>\n</c> escaped inside the text columns.
    /// </summary>
    internal static class DialogueSnapshot
    {
        private const int ColumnCount = 6;

        private static string FilePath
            => Path.Combine(Application.dataPath, "_Project", "Tests", "EditMode", "Dialogue", "DialogueSnapshot.tsv");

        public static IReadOnlyList<SnapshotRow> Load()
        {
            string[] lines = File.ReadAllLines(FilePath, new UTF8Encoding(false));
            var rows = new List<SnapshotRow>(lines.Length);

            // Line 0 is the header; blank lines can only be a trailing newline because real line breaks are escaped.
            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].Length == 0)
                    continue;

                rows.Add(Parse(lines[i], i + 1));
            }

            return rows;
        }

        /// <summary>The rows of one trigger in line order.</summary>
        public static IReadOnlyList<SnapshotRow> RowsOf(string source, string triggerPath)
            => Load().Where(row => row.Source == source && row.TriggerPath == triggerPath).OrderBy(row => row.Index).ToArray();

        /// <summary>NUnit case data: one <c>(source, triggerPath)</c> pair per distinct trigger in the snapshot.</summary>
        public static IEnumerable<object[]> TriggerCases()
            => Load()
                .Select(row => (row.Source, row.TriggerPath))
                .Distinct()
                .Select(trigger => new object[] { trigger.Source, trigger.TriggerPath })
                .ToArray();

        private static SnapshotRow Parse(string line, int lineNumber)
        {
            string[] columns = line.Split('\t');
            if (columns.Length != ColumnCount)
                throw new FormatException($"DialogueSnapshot.tsv line {lineNumber}: expected {ColumnCount} columns, found {columns.Length}.");

            return new SnapshotRow(
                source: columns[0],
                triggerPath: columns[1],
                index: int.Parse(columns[2]),
                speaker: (DialogueSpeaker)Enum.Parse(typeof(DialogueSpeaker), columns[3]),
                russian: Unescape(columns[4], lineNumber),
                english: columns[5].Length == 0 ? null : Unescape(columns[5], lineNumber));
        }

        private static string Unescape(string field, int lineNumber)
        {
            if (field.IndexOf('\\') < 0)
                return field;

            var text = new StringBuilder(field.Length);

            for (int i = 0; i < field.Length; i++)
            {
                if (field[i] != '\\')
                {
                    text.Append(field[i]);
                    continue;
                }

                i++;
                if (i == field.Length)
                    throw new FormatException($"DialogueSnapshot.tsv line {lineNumber}: dangling backslash at the end of a column.");

                text.Append(field[i] switch
                {
                    '\\' => '\\',
                    't' => '\t',
                    'r' => '\r',
                    'n' => '\n',
                    _ => throw new FormatException($"DialogueSnapshot.tsv line {lineNumber}: unknown escape '\\{field[i]}'.")
                });
            }

            return text.ToString();
        }
    }
}
