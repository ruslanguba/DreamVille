using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;
using UnityEngine.SceneManagement;

using DreamVille.Tests.EditMode.Composition;
using DreamVille.UI.Hints;

namespace DreamVille.Tests.EditMode.UI
{
    /// <summary>
    /// Every proximity hint text is driven by a <see cref="LocalizeStringEvent"/> on the
    /// "Hints" string table, its Russian entry is the text the scene shows, and the table holds nothing else.
    /// </summary>
    public class HintLocalizationContentTests
    {
        private const string TableName = "Hints";

        private sealed class ScannedHintText
        {
            public string Scene { get; set; }
            public string TriggerPath { get; set; }
            public string Text { get; set; }
            public int LocalizeEvents { get; set; }
            public string TableGuid { get; set; }
            public long KeyId { get; set; }
            public bool UpdatesItsOwnText { get; set; }

            public override string ToString() => $"{Scene}: {TriggerPath}";
        }

        private string _originalScenePath;
        private bool _scenesOpened;
        private List<ScannedHintText> _texts;

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

            _texts = new List<ScannedHintText>();
            _scenesOpened = true;

            SceneContentScan.ForEach<ProximityHint>((scene, triggerPath, hint) => _texts.Add(Scan(scene, triggerPath, hint)));
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

        private static ScannedHintText Scan(string scene, string triggerPath, ProximityHint hint)
        {
            var target = new SerializedObject(hint).FindProperty("_object").objectReferenceValue as GameObject;
            TMP_Text text = target != null ? target.GetComponentInChildren<TMP_Text>(true) : null;
            LocalizeStringEvent[] events = text != null ? text.GetComponents<LocalizeStringEvent>() : new LocalizeStringEvent[0];
            LocalizeStringEvent localize = events.FirstOrDefault();

            return new ScannedHintText
            {
                Scene = scene,
                TriggerPath = triggerPath,
                Text = text != null ? text.text : null,
                LocalizeEvents = events.Length,
                TableGuid = localize != null ? localize.StringReference.TableReference.TableCollectionNameGuid.ToString() : null,
                KeyId = localize != null ? localize.StringReference.TableEntryReference.KeyId : 0,
                UpdatesItsOwnText = localize != null
                    && localize.OnUpdateString.GetPersistentEventCount() == 1
                    && localize.OnUpdateString.GetPersistentTarget(0) == text
                    && localize.OnUpdateString.GetPersistentMethodName(0) == "set_text",
            };
        }

        // Re-fetched per test: opening a scene unloads unused assets, so table references never outlive the setup.
        private static StringTableCollection Collection()
        {
            StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(TableName);
            Assert.That(collection, Is.Not.Null, $"The '{TableName}' string table collection is missing.");
            return collection;
        }

        private static StringTable Table(string localeCode) => (StringTable)Collection().GetTable(localeCode);

        [Test]
        public void EveryHintText_HasOneLocalizeStringEventOnTheHintsTable()
        {
            string tableGuid = Collection().SharedData.TableCollectionNameGuid.ToString();

            string[] offenders = _texts
                .Where(text => text.LocalizeEvents != 1 || text.TableGuid != tableGuid || text.KeyId == 0)
                .Select(text => $"{text} (events {text.LocalizeEvents}, table {text.TableGuid})")
                .ToArray();

            Assert.That(offenders, Is.Empty, "Hint texts not localized through the Hints table:\n" + string.Join("\n", offenders));
        }

        [Test]
        public void EveryHintText_UpdatesItsOwnText()
        {
            string[] offenders = _texts.Where(text => !text.UpdatesItsOwnText).Select(text => text.ToString()).ToArray();

            Assert.That(offenders, Is.Empty, "LocalizeStringEvent must write into the hint's own TMP text:\n" + string.Join("\n", offenders));
        }

        [Test]
        public void EveryHintText_RussianEntryIsTheSceneText()
        {
            StringTable russian = Table("ru");

            string[] offenders = _texts
                .Where(text => russian.GetEntry(text.KeyId)?.Value != text.Text)
                .Select(text => $"{text}: table '{russian.GetEntry(text.KeyId)?.Value}' vs scene '{text.Text}'")
                .ToArray();

            Assert.That(offenders, Is.Empty, "Russian hint entries differ from the scene text:\n" + string.Join("\n", offenders));
        }

        [Test]
        public void HintsTable_EveryKeyHasEnglishText()
        {
            StringTable english = Table("en");

            string[] missing = Collection().SharedData.Entries
                .Where(entry => string.IsNullOrEmpty(english.GetEntry(entry.Id)?.Value))
                .Select(entry => entry.Key)
                .ToArray();

            Assert.That(missing, Is.Empty, "Hints without English text (they would fall back to Russian):\n" + string.Join("\n", missing));
        }

        [Test]
        public void HintsTable_EveryKeyIsUsedByExactlyOneHint()
        {
            var used = _texts.GroupBy(text => text.KeyId).ToDictionary(group => group.Key, group => group.Count());

            string[] offenders = Collection().SharedData.Entries
                .Where(entry => !used.TryGetValue(entry.Id, out int count) || count != 1)
                .Select(entry => entry.Key)
                .ToArray();

            Assert.That(offenders, Is.Empty, "Keys unused or shared by several hints:\n" + string.Join("\n", offenders));
        }

        [Test]
        public void HintsTable_HasNoEmptyOrSmartEntries()
        {
            var offenders = new List<string>();
            foreach (StringTable table in Collection().StringTables)
            {
                foreach (StringTableEntry entry in table.Values)
                {
                    if (string.IsNullOrEmpty(entry.Value) || entry.IsSmart)
                        offenders.Add($"{table.LocaleIdentifier.Code}: {entry.Key}");
                }
            }

            Assert.That(offenders, Is.Empty, "Empty or Smart hint entries:\n" + string.Join("\n", offenders));
        }

        [Test]
        public void HintsTable_IsPreloadedForEveryLocale()
        {
            string[] notPreloaded = Collection().StringTables
                .Where(table => !LocalizationEditorSettings.GetPreloadTableFlag(table))
                .Select(table => table.LocaleIdentifier.Code)
                .ToArray();

            Assert.That(notPreloaded, Is.Empty, "Hint tables must be preloaded, or a hint may show its text a frame late.");
        }
    }
}
