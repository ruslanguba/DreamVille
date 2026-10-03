using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using DreamVille.Tests.EditMode.Composition;
using DreamVille.UI.Hints;

namespace DreamVille.Tests.EditMode.UI
{
    /// <summary>
    /// Guards spec 12 §5 items 4-10: every <see cref="ProximityHint"/> in the scenes keeps the target, timings and
    /// text recorded in <see cref="ProximityHintSnapshot"/>, points at a hidden target that has something to show,
    /// owns an enabled trigger collider and shares its target with no other hint; the host prefab writes its
    /// defaults into the asset. The scan of all scenes runs once in the fixture setup; tests then work on plain
    /// data, because opening the next scene unloads the previous one.
    /// </summary>
    public class ProximityHintContentTests
    {
        private const string HintPrefabPath = "Assets/_Project/Prefabs/ProximityHint.prefab";

        // Serialized names are frozen (they are overridden by property path in the scenes), so reading them as strings is safe.
        private const string TargetField = "_object";
        private const string FadeField = "_fadeDuration";
        private const string ShowField = "_showDuration";
        private const string ShowOnceField = "_showOnce";

        private const string TextSeparator = " | ";

        private sealed class ScannedHint
        {
            public string Scene { get; set; }

            public string TriggerPath { get; set; }

            /// <summary>Null when the hint has no target assigned.</summary>
            public string TargetPath { get; set; }

            /// <summary>Identifies the target within <see cref="Scene"/>; meaningful only when <see cref="TargetPath"/> is not null.</summary>
            public int TargetInstanceId { get; set; }

            public bool TargetActiveSelf { get; set; }

            public int GraphicCount { get; set; }

            public string Text { get; set; }

            public float ShowSeconds { get; set; }

            public float FadeSeconds { get; set; }

            public bool ShowOnce { get; set; }

            public bool HasEnabledTriggerCollider { get; set; }

            public override string ToString() => $"{Scene}: {TriggerPath}";
        }

        private string _originalScenePath;
        private bool _scenesOpened;
        private List<ScannedHint> _hints;

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

            _hints = new List<ScannedHint>();
            _scenesOpened = true;

            SceneContentScan.ForEach<ProximityHint>((scene, triggerPath, hint) => _hints.Add(Scan(scene, triggerPath, hint)));
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

        private static IEnumerable<object[]> SnapshotRows() => ProximityHintSnapshot.Cases();

        [TestCaseSource(nameof(SnapshotRows))]
        public void Snapshot_HintMatches(string scene, string triggerPath)
        {
            HintRow row = ProximityHintSnapshot.RowOf(scene, triggerPath);
            ScannedHint hint = FindHint(scene, triggerPath);
            var differences = new List<string>();

            AddIfDifferent(differences, "target", row.TargetPath, hint.TargetPath);
            AddIfDifferent(differences, "show seconds", row.ShowSeconds, hint.ShowSeconds);
            AddIfDifferent(differences, "fade seconds", row.FadeSeconds, hint.FadeSeconds);
            AddIfDifferent(differences, "show once", row.ShowOnce, hint.ShowOnce);
            AddIfDifferent(differences, "text", row.Text, hint.Text);

            Assert.That(differences, Is.Empty, $"{hint} differs from the snapshot:\n" + string.Join("\n", differences));
        }

        [Test]
        public void EveryHintInScenes_IsInTheSnapshot()
        {
            string[] unlisted = _hints
                .Where(hint => !ProximityHintSnapshot.Rows.Any(row => row.Scene == hint.Scene && row.TriggerPath == hint.TriggerPath))
                .Select(hint => hint.ToString())
                .ToArray();

            Assert.That(unlisted, Is.Empty,
                "Hints missing from ProximityHintSnapshot (a new or renamed trigger must be added consciously):\n" + string.Join("\n", unlisted));
        }

        [Test]
        public void EveryHint_TargetStartsInactive()
        {
            string[] shownTooEarly = _hints
                .Where(hint => hint.TargetPath != null && hint.TargetActiveSelf)
                .Select(hint => $"{hint} -> {hint.TargetPath}")
                .ToArray();

            Assert.That(shownTooEarly, Is.Empty,
                "Hint targets that are active in the scene, so they show before the player arrives:\n" + string.Join("\n", shownTooEarly));
        }

        [Test]
        public void EveryHint_TargetHasAGraphic()
        {
            string[] showingNothing = _hints
                .Where(hint => hint.TargetPath == null || hint.GraphicCount == 0)
                .Select(hint => hint.TargetPath == null ? $"{hint} has no target" : $"{hint} -> {hint.TargetPath} has no Graphic")
                .ToArray();

            Assert.That(showingNothing, Is.Empty,
                "Hints that would show nothing:\n" + string.Join("\n", showingNothing));
        }

        [Test]
        public void EveryHint_HasAnEnabledTriggerCollider()
        {
            string[] unreachable = _hints
                .Where(hint => !hint.HasEnabledTriggerCollider)
                .Select(hint => hint.ToString())
                .ToArray();

            Assert.That(unreachable, Is.Empty,
                "Hints without an enabled trigger collider on their own GameObject (the player could never enter them):\n" + string.Join("\n", unreachable));
        }

        [Test]
        public void NoTwoHints_ShareATarget()
        {
            string[] shared = _hints
                .Where(hint => hint.TargetPath != null)
                .GroupBy(hint => (hint.Scene, hint.TargetInstanceId))
                .Where(group => group.Count() > 1)
                .Select(group => $"{group.First().Scene}: {group.First().TargetPath} is the target of {string.Join(", ", group.Select(hint => hint.TriggerPath))}")
                .ToArray();

            Assert.That(shared, Is.Empty,
                "Two hints on one target would fight over one alpha with two timers:\n" + string.Join("\n", shared));
        }

        [Test]
        public void HintPrefab_SerializesItsDefaults()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HintPrefabPath);
            Assert.That(prefab, Is.Not.Null, $"{HintPrefabPath} is missing.");

            BoxCollider[] colliders = prefab.GetComponents<BoxCollider>();
            ProximityHint[] hints = prefab.GetComponents<ProximityHint>();
            Assert.That(colliders, Has.Length.EqualTo(1), $"{HintPrefabPath} must carry exactly one BoxCollider.");
            Assert.That(hints, Has.Length.EqualTo(1), $"{HintPrefabPath} must carry exactly one {nameof(ProximityHint)}.");

            var serialized = new SerializedObject(hints[0]);
            string yaml = File.ReadAllText(Path.GetFullPath(Path.Combine(Application.dataPath, "..", HintPrefabPath)));

            Assert.That(colliders[0].isTrigger, Is.True, "The prefab's collider must be a trigger.");
            Assert.That(serialized.FindProperty(TargetField).objectReferenceValue, Is.Null, "The prefab has no target by design; every instance sets its own.");
            Assert.That(serialized.FindProperty(FadeField).floatValue, Is.EqualTo(4f), "Prefab fade seconds.");
            Assert.That(serialized.FindProperty(ShowField).floatValue, Is.EqualTo(2f), "Prefab show seconds.");
            Assert.That(serialized.FindProperty(ShowOnceField).boolValue, Is.False, "Prefab show-once flag.");
            Assert.That(yaml, Does.Contain(ShowField + ":"),
                "The show duration must be written into the asset, or changing the C# initializer would silently change every hint that does not override it.");
        }

        private ScannedHint FindHint(string scene, string triggerPath)
        {
            ScannedHint[] matches = _hints
                .Where(hint => hint.Scene == scene && hint.TriggerPath == triggerPath)
                .ToArray();

            Assert.That(matches, Has.Length.EqualTo(1),
                $"Expected exactly one {nameof(ProximityHint)} at '{triggerPath}' in '{scene}', found {matches.Length}.");

            return matches[0];
        }

        private static void AddIfDifferent<T>(List<string> differences, string what, T expected, T actual)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                differences.Add($"{what}: expected '{expected}', found '{actual}'");
        }

        private static ScannedHint Scan(string scene, string triggerPath, ProximityHint hint)
        {
            var serialized = new SerializedObject(hint);
            var target = serialized.FindProperty(TargetField).objectReferenceValue as GameObject;

            var scanned = new ScannedHint
            {
                Scene = scene,
                TriggerPath = triggerPath,
                ShowSeconds = serialized.FindProperty(ShowField).floatValue,
                FadeSeconds = serialized.FindProperty(FadeField).floatValue,
                ShowOnce = serialized.FindProperty(ShowOnceField).boolValue,
                HasEnabledTriggerCollider = hint.GetComponents<Collider>().Any(collider => collider.enabled && collider.isTrigger),
            };

            if (target == null)
                return scanned;

            scanned.TargetPath = SceneContentScan.HierarchyPathOf(target.transform);
            scanned.TargetInstanceId = target.GetInstanceID();
            scanned.TargetActiveSelf = target.activeSelf;
            scanned.GraphicCount = target.GetComponentsInChildren<Graphic>(true).Length;
            scanned.Text = string.Join(TextSeparator, target.GetComponentsInChildren<TMP_Text>(true).Select(text => text.text));

            return scanned;
        }
    }
}
