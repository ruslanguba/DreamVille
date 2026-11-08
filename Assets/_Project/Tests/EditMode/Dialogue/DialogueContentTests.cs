using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.SceneManagement;

using DreamVille.Dialogue;

namespace DreamVille.Tests.EditMode.Dialogue
{
    /// <summary>
    /// Every dialogue line of the game lives in a <see cref="DialogueDefinition"/>
    /// whose text sits in the "Dialogues" string table, byte-for-byte equal to the checked-in
    /// <c>DialogueSnapshot.tsv</c>, and the table itself stays clean (no empty, Smart, orphaned or shared keys).
    /// The scan of all scenes and prefabs runs once in the fixture setup; tests then work on plain data and
    /// re-fetch table assets themselves, because opening a scene unloads unused assets.
    /// </summary>
    public class DialogueContentTests
    {
        private const string PlaceholderFolder = "Assets/_Project/Settings/Dialogues/Prefabs";

        private sealed class ScannedTrigger
        {
            public ScannedTrigger(string source, string triggerPath, string definitionAssetPath)
            {
                Source = source;
                TriggerPath = triggerPath;
                DefinitionAssetPath = definitionAssetPath;
            }

            public string Source { get; }

            public string TriggerPath { get; }

            /// <summary>Null or empty when the trigger has no definition.</summary>
            public string DefinitionAssetPath { get; }

            public override string ToString() => $"{Source}: {TriggerPath}";
        }

        private string _originalScenePath;
        private bool _scenesOpened;
        private List<ScannedTrigger> _triggers;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Scene activeScene = EditorSceneManager.GetActiveScene();
            _originalScenePath = activeScene.path;

            // Interactively a dirty scene just skips the fixture; on a build machine it is a real failure.
            const string dirtySceneMessage = "A dirty scene is open; save or discard changes before running this fixture.";
            if (Application.isBatchMode)
                Assert.That(activeScene.isDirty, Is.False, dirtySceneMessage);
            else
                Assume.That(!activeScene.isDirty, dirtySceneMessage);

            _triggers = new List<ScannedTrigger>();
            _scenesOpened = true;

            DialogueTriggerScan.ForEachTrigger((source, triggerPath, trigger) =>
            {
                string definitionPath = trigger.Definition != null ? AssetDatabase.GetAssetPath(trigger.Definition) : null;
                _triggers.Add(new ScannedTrigger(source, triggerPath, definitionPath));
            });
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            // Never reopen a scene when the setup was skipped: that would discard the user's unsaved changes.
            if (!_scenesOpened)
                return;

            if (string.IsNullOrEmpty(_originalScenePath))
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            else
                EditorSceneManager.OpenScene(_originalScenePath, OpenSceneMode.Single);
        }

        private static IEnumerable<object[]> SnapshotTriggers() => DialogueSnapshot.TriggerCases();

        [TestCaseSource(nameof(SnapshotTriggers))]
        public void Snapshot_EveryRowHasAMatchingLine(string source, string triggerPath)
        {
            IReadOnlyList<SnapshotRow> rows = DialogueSnapshot.RowsOf(source, triggerPath);
            ScannedTrigger trigger = FindTrigger(source, triggerPath);
            DialogueDefinition definition = LoadDefinition(trigger);
            DialoguesTable table = DialoguesTable.Load();

            Assert.That(definition.Lines.Count, Is.EqualTo(rows.Count),
                $"{trigger}: '{trigger.DefinitionAssetPath}' has a different number of lines than the snapshot.");

            for (int i = 0; i < rows.Count; i++)
            {
                Assert.That(rows[i].Index, Is.EqualTo(i), $"{trigger}: snapshot indexes must run 0..{rows.Count - 1} without gaps.");
                AssertLineMatchesRow(table, definition.Lines[i], rows[i]);
            }
        }

        [Test]
        public void EveryTriggerInScenesAndPrefabs_HasADefinition()
        {
            string[] withoutDefinition = _triggers
                .Where(trigger => string.IsNullOrEmpty(trigger.DefinitionAssetPath))
                .Select(trigger => trigger.ToString())
                .ToArray();

            Assert.That(withoutDefinition, Is.Empty,
                "Triggers without a DialogueDefinition (their text would be lost with the legacy lists):\n" + string.Join("\n", withoutDefinition));
        }

        [Test]
        public void EverySnapshotTrigger_StillExists()
        {
            string[] missing = DialogueSnapshot.Load()
                .Select(row => (row.Source, row.TriggerPath))
                .Distinct()
                .Where(snapshotTrigger => !_triggers.Any(trigger => trigger.Source == snapshotTrigger.Source && trigger.TriggerPath == snapshotTrigger.TriggerPath))
                .Select(snapshotTrigger => $"{snapshotTrigger.Source}: {snapshotTrigger.TriggerPath}")
                .ToArray();

            Assert.That(missing, Is.Empty,
                "Snapshot triggers that no longer exist (deleted or renamed, their dialogue is lost):\n" + string.Join("\n", missing));
        }

        [Test]
        public void DialoguesTable_EveryKeyHasRussianText()
        {
            DialoguesTable table = DialoguesTable.Load();

            string[] withoutRussian = table.SharedData.Entries
                .Where(key => string.IsNullOrWhiteSpace(table.Russian.GetEntry(key.Id)?.Value))
                .Select(key => key.Key)
                .ToArray();

            Assert.That(withoutRussian, Is.Empty, "Keys without Russian text:\n" + string.Join("\n", withoutRussian));
        }

        [Test]
        public void DialoguesTable_HasNoEmptyEntries()
        {
            DialoguesTable table = DialoguesTable.Load();

            string[] emptyEntries = table.AllTables
                .SelectMany(localeTable => localeTable.Values
                    .Where(entry => string.IsNullOrWhiteSpace(entry.Value))
                    .Select(entry => $"{localeTable.LocaleIdentifier.Code}: {entry.Key}"))
                .ToArray();

            Assert.That(emptyEntries, Is.Empty,
                "Empty entries defeat locale fallback (an absent entry falls back, an empty one shows nothing):\n" + string.Join("\n", emptyEntries));
        }

        [Test]
        public void DialoguesTable_NoEntryIsSmart()
        {
            DialoguesTable table = DialoguesTable.Load();

            string[] smartEntries = table.AllTables
                .SelectMany(localeTable => localeTable.Values
                    .Where(entry => entry.IsSmart)
                    .Select(entry => $"{localeTable.LocaleIdentifier.Code}: {entry.Key}"))
                .ToArray();

            Assert.That(smartEntries, Is.Empty,
                "Dialogue text is plain: Smart Format would swallow literal braces:\n" + string.Join("\n", smartEntries));
        }

        [Test]
        public void DialoguesTable_EveryKeyIsReferencedByExactlyOneLine()
        {
            DialoguesTable table = DialoguesTable.Load();
            var usages = table.SharedData.Entries.ToDictionary(key => key.Id, key => new List<string>());
            var problems = new List<string>();

            foreach (string guid in AssetDatabase.FindAssets("t:DialogueDefinition"))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var definition = AssetDatabase.LoadAssetAtPath<DialogueDefinition>(assetPath);

                for (int i = 0; i < definition.Lines.Count; i++)
                {
                    DialogueLine line = definition.Lines[i];
                    SharedTableData.SharedTableEntry key = table.Contains(line) ? table.KeyOf(line) : null;

                    if (key == null)
                        problems.Add($"{assetPath} line {i} points at no key of '{DialoguesTable.CollectionName}'.");
                    else
                        usages[key.Id].Add($"{assetPath} line {i}");
                }
            }

            foreach (SharedTableData.SharedTableEntry key in table.SharedData.Entries)
            {
                List<string> users = usages[key.Id];
                if (users.Count != 1)
                    problems.Add($"Key '{key.Key}' is used by {users.Count} lines: {string.Join(", ", users)}");
            }

            Assert.That(problems, Is.Empty, string.Join("\n", problems));
        }

        [Test]
        public void DialoguesTable_IsPreloadedForEveryLocale()
        {
            DialoguesTable table = DialoguesTable.Load();
            var problems = new List<string>();

            foreach (Locale locale in LocalizationEditorSettings.GetLocales())
            {
                LocalizationTable localeTable = table.Collection.GetTable(locale.Identifier);

                if (localeTable == null)
                    problems.Add($"{locale.Identifier.Code}: no table in '{DialoguesTable.CollectionName}'.");
                else if (!LocalizationEditorSettings.GetPreloadTableFlag(localeTable))
                    problems.Add($"{locale.Identifier.Code}: preload flag is off, the first line would resolve asynchronously.");
            }

            Assert.That(problems, Is.Empty, string.Join("\n", problems));
        }

        [TestCase("DialogueSample")]
        [TestCase("FinalLevel")]
        public void PrefabAssets_ReferencePlaceholderDefinitions(string prefabName)
        {
            ScannedTrigger trigger = FindTrigger(DialogueTriggerScan.PrefabSource, prefabName);

            Assert.That(trigger.DefinitionAssetPath, Is.EqualTo($"{PlaceholderFolder}/{prefabName}_Placeholder.asset"),
                $"The {prefabName} prefab must default to its placeholder definition so that a new instance still plays something.");
        }

        private ScannedTrigger FindTrigger(string source, string triggerPath)
        {
            ScannedTrigger[] matches = _triggers
                .Where(trigger => trigger.Source == source && trigger.TriggerPath == triggerPath)
                .ToArray();

            Assert.That(matches, Has.Length.EqualTo(1),
                $"Expected exactly one DialogueTrigger at '{triggerPath}' in '{source}', found {matches.Length}.");

            return matches[0];
        }

        private static DialogueDefinition LoadDefinition(ScannedTrigger trigger)
        {
            Assert.That(trigger.DefinitionAssetPath, Is.Not.Null.And.Not.Empty, $"{trigger} has no DialogueDefinition.");

            var definition = AssetDatabase.LoadAssetAtPath<DialogueDefinition>(trigger.DefinitionAssetPath);
            Assert.That(definition, Is.Not.Null, $"{trigger}: '{trigger.DefinitionAssetPath}' is not a DialogueDefinition asset.");

            return definition;
        }

        private static void AssertLineMatchesRow(DialoguesTable table, DialogueLine line, SnapshotRow row)
        {
            string where = $"{row.Source}: {row.TriggerPath} line {row.Index}";

            Assert.That(table.Contains(line), Is.True, $"{where}: the text does not point into '{DialoguesTable.CollectionName}'.");
            Assert.That(line.Speaker, Is.EqualTo(row.Speaker), $"{where}: speaker differs from the snapshot.");

            StringTableEntry russian = table.RussianEntryOf(line);
            Assert.That(russian, Is.Not.Null, $"{where}: the Russian entry is missing.");
            Assert.That(russian.Value, Is.EqualTo(row.Russian), $"{where}: Russian text differs from the snapshot.");

            StringTableEntry english = table.EnglishEntryOf(line);
            if (row.English == null)
            {
                Assert.That(english, Is.Null, $"{where}: the snapshot has no English text, yet an English entry exists.");
                return;
            }

            Assert.That(english, Is.Not.Null, $"{where}: the English entry is missing.");
            Assert.That(english.Value, Is.EqualTo(row.English), $"{where}: English text differs from the snapshot.");
        }
    }
}
