using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.SceneManagement;

using DreamVille.Dialogue;

namespace DreamVille.Tests.EditMode.Dialogue
{
    /// <summary>
    /// Transitional (spec 11 §5 item 27): proves, independently of <c>DialogueSnapshot.tsv</c>, that each trigger's
    /// <see cref="DialogueDefinition"/> says what the trigger's own resolved legacy <c>_sentencesRu</c> /
    /// <c>_sentencesEn</c> lists say. The lists are read through <see cref="SerializedObject"/> so prefab-instance
    /// overrides are resolved exactly as the migration saw them. Deleted in spec 11 B4 together with the lists;
    /// the snapshot test in <see cref="DialogueContentTests"/> stays as the permanent proof.
    /// </summary>
    public class DialogueLegacyEquivalenceTests
    {
        // Spec 11 §8 Q2 (decided): this finale keeps its single Russian line and gets no English entry, so English
        // falls back to it. The three old English lines were an unrelated stale copy and are dropped on purpose.
        private static readonly (string Source, string TriggerPath)[] DeliberatelyDroppedEnglish =
        {
            ("Level_02", "AllLevel_1/FinalLevel")
        };

        private sealed class LegacyLine
        {
            public LegacyLine(bool isPluh, string sentence)
            {
                IsPluh = isPluh;
                Sentence = sentence;
            }

            public bool IsPluh { get; }

            public string Sentence { get; }
        }

        private sealed class LegacyTrigger
        {
            public LegacyTrigger(string source, string triggerPath, string definitionAssetPath, List<LegacyLine> russian, List<LegacyLine> english)
            {
                Source = source;
                TriggerPath = triggerPath;
                DefinitionAssetPath = definitionAssetPath;
                Russian = russian;
                English = english;
            }

            public string Source { get; }

            public string TriggerPath { get; }

            public string DefinitionAssetPath { get; }

            public List<LegacyLine> Russian { get; }

            public List<LegacyLine> English { get; }

            public override string ToString() => $"{Source}: {TriggerPath}";
        }

        private string _originalScenePath;
        private bool _scenesOpened;
        private List<LegacyTrigger> _triggers;

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

            _triggers = new List<LegacyTrigger>();
            _scenesOpened = true;

            DialogueTriggerScan.ForEachTrigger((source, triggerPath, trigger) =>
            {
                var serializedTrigger = new SerializedObject(trigger);
                string definitionPath = trigger.Definition != null ? AssetDatabase.GetAssetPath(trigger.Definition) : null;

                _triggers.Add(new LegacyTrigger(
                    source,
                    triggerPath,
                    definitionPath,
                    ReadLegacyList(serializedTrigger, "_sentencesRu"),
                    ReadLegacyList(serializedTrigger, "_sentencesEn")));
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
        public void Definition_MatchesResolvedLegacyLists(string source, string triggerPath)
        {
            LegacyTrigger trigger = FindTrigger(source, triggerPath);
            DialogueDefinition definition = LoadDefinition(trigger);
            DialoguesTable table = DialoguesTable.Load();
            bool dropsEnglish = DeliberatelyDroppedEnglish.Contains((source, triggerPath));

            Assert.That(definition.Lines.Count, Is.EqualTo(trigger.Russian.Count),
                $"{trigger}: the definition and the legacy Russian list differ in length.");

            for (int i = 0; i < trigger.Russian.Count; i++)
            {
                DialogueLine line = definition.Lines[i];
                LegacyLine russian = trigger.Russian[i];
                string where = $"{trigger} line {i}";

                Assert.That(line.Speaker, Is.EqualTo(russian.IsPluh ? DialogueSpeaker.Player : DialogueSpeaker.Npc), $"{where}: speaker.");
                Assert.That(table.RussianEntryOf(line)?.Value, Is.EqualTo(russian.Sentence), $"{where}: Russian text.");

                if (dropsEnglish || trigger.English.Count == 0)
                    Assert.That(table.EnglishEntryOf(line), Is.Null, $"{where}: the legacy lists have no English for this line, yet an English entry exists.");
                else
                    AssertEnglishMatches(table, line, trigger, i);
            }

            if (dropsEnglish)
            {
                Assert.That(trigger.English.Count, Is.Not.EqualTo(trigger.Russian.Count),
                    $"{trigger} is listed as dropping its legacy English, but that list now matches the Russian one: remove the exception.");
            }
        }

        private static void AssertEnglishMatches(DialoguesTable table, DialogueLine line, LegacyTrigger trigger, int index)
        {
            string where = $"{trigger} line {index}";

            Assert.That(trigger.English.Count, Is.EqualTo(trigger.Russian.Count),
                $"{trigger}: the legacy English list differs in length from the Russian one and is not a listed exception.");
            Assert.That(trigger.English[index].IsPluh, Is.EqualTo(trigger.Russian[index].IsPluh), $"{where}: legacy English speaker differs from the Russian one.");

            StringTableEntry english = table.EnglishEntryOf(line);
            Assert.That(english, Is.Not.Null, $"{where}: the legacy English text was not migrated.");
            Assert.That(english.Value, Is.EqualTo(trigger.English[index].Sentence), $"{where}: English text.");
        }

        private LegacyTrigger FindTrigger(string source, string triggerPath)
        {
            LegacyTrigger[] matches = _triggers
                .Where(trigger => trigger.Source == source && trigger.TriggerPath == triggerPath)
                .ToArray();

            Assert.That(matches, Has.Length.EqualTo(1),
                $"Expected exactly one DialogueTrigger at '{triggerPath}' in '{source}', found {matches.Length}.");

            return matches[0];
        }

        private static DialogueDefinition LoadDefinition(LegacyTrigger trigger)
        {
            Assert.That(trigger.DefinitionAssetPath, Is.Not.Null.And.Not.Empty, $"{trigger} has no DialogueDefinition.");

            var definition = AssetDatabase.LoadAssetAtPath<DialogueDefinition>(trigger.DefinitionAssetPath);
            Assert.That(definition, Is.Not.Null, $"{trigger}: '{trigger.DefinitionAssetPath}' is not a DialogueDefinition asset.");

            return definition;
        }

        private static List<LegacyLine> ReadLegacyList(SerializedObject serializedTrigger, string fieldName)
        {
            SerializedProperty list = serializedTrigger.FindProperty(fieldName);
            Assert.That(list, Is.Not.Null, $"DialogueTrigger has no serialized field '{fieldName}'.");

            var lines = new List<LegacyLine>(list.arraySize);
            for (int i = 0; i < list.arraySize; i++)
            {
                SerializedProperty element = list.GetArrayElementAtIndex(i);
                lines.Add(new LegacyLine(element.FindPropertyRelative("IsPluh").boolValue, element.FindPropertyRelative("Sentence").stringValue));
            }

            return lines;
        }
    }
}
