using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using DreamVille.Tests.EditMode.Composition;

namespace DreamVille.Tests.EditMode.Enemies
{
    /// <summary>
    /// Guards spec 14 §5 items 1-3 and spec 15 §5 item 5: every enemy in every scene and prefab keeps the tuning recorded in
    /// <c>EnemyTuningSnapshot.tsv</c> (move speed, chase and attack ranges, attack cooldown, rotation speed, attack spawn,
    /// patrol route, archetype config). Values are read as the asset resolves them, through the enemy's config and with
    /// prefab-instance overrides included, so a serialized rename that loses designer data turns this red. The snapshot
    /// changes only on purpose: retuning an archetype or swapping an enemy's config lists every enemy it reaches.
    /// All assets are scanned once in the fixture setup; tests then compare plain rows.
    /// </summary>
    public class EnemyTuningSnapshotTests
    {
        private const string ProjectFolder = "Assets/_Project";
        private const string ScenesFolder = "Assets/_Project/Scenes";

        private string _originalScenePath;
        private bool _scenesOpened;
        private List<EnemyTuningRow> _scanned;

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

            _scanned = new List<EnemyTuningRow>();

            // Prefabs before scenes: opening a scene Single unloads assets that nothing references any more.
            foreach (string prefabPath in SceneContentScan.AssetPaths("t:Prefab", ProjectFolder))
                _scanned.AddRange(EnemyTuningReader.Read(prefabPath));

            _scenesOpened = true;

            foreach (string scenePath in SceneContentScan.AssetPaths("t:Scene", ScenesFolder))
                _scanned.AddRange(EnemyTuningReader.Read(scenePath));
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

        private static IEnumerable<string> SnapshotAssets() => EnemyTuningSnapshot.Assets();

        [TestCaseSource(nameof(SnapshotAssets))]
        public void Snapshot_AssetMatches(string asset)
        {
            Dictionary<string, EnemyTuningRow> expected = EnemyTuningSnapshot.RowsOf(asset).ToDictionary(row => row.Path, StringComparer.Ordinal);
            Dictionary<string, EnemyTuningRow> actual = _scanned.Where(row => row.Asset == asset).ToDictionary(row => row.Path, StringComparer.Ordinal);
            var differences = new List<string>();

            foreach (string path in expected.Keys.Except(actual.Keys).OrderBy(path => path, StringComparer.Ordinal))
                differences.Add($"{path}: in the snapshot, not in the asset");

            foreach (string path in actual.Keys.Except(expected.Keys).OrderBy(path => path, StringComparer.Ordinal))
                differences.Add($"{path}: in the asset, not in the snapshot");

            foreach (string path in expected.Keys.Intersect(actual.Keys).OrderBy(path => path, StringComparer.Ordinal))
                AddCellDifferences(differences, path, expected[path], actual[path]);

            Assert.That(differences, Is.Empty,
                $"{asset}: enemy tuning differs from {EnemyTuningSnapshot.FileName} " +
                "(designer data was lost, or the change is deliberate and the snapshot must be updated in the same commit):\n" +
                string.Join("\n", differences));
        }

        [Test]
        public void EveryAssetWithEnemies_IsInTheSnapshot()
        {
            var snapshotAssets = new HashSet<string>(EnemyTuningSnapshot.Assets(), StringComparer.Ordinal);

            string[] unlisted = _scanned
                .Select(row => row.Asset)
                .Distinct()
                .Where(asset => !snapshotAssets.Contains(asset))
                .OrderBy(asset => asset, StringComparer.Ordinal)
                .ToArray();

            Assert.That(unlisted, Is.Empty,
                $"Assets with enemies missing from {EnemyTuningSnapshot.FileName} (a new enemy asset is added consciously):\n" +
                string.Join("\n", unlisted));
        }

        [Test]
        public void SnapshotKeys_AreUnique()
        {
            string[] duplicatedInSnapshot = DuplicateKeys(EnemyTuningSnapshot.Load());
            string[] duplicatedInAssets = DuplicateKeys(_scanned);

            Assert.That(duplicatedInSnapshot, Is.Empty, $"Duplicate keys in {EnemyTuningSnapshot.FileName}:\n" + string.Join("\n", duplicatedInSnapshot));
            Assert.That(duplicatedInAssets, Is.Empty,
                "Two enemies resolve to the same key; the [n] sibling rule no longer tells them apart:\n" + string.Join("\n", duplicatedInAssets));
        }

        [Test]
        public void StationaryEnemies_HaveNoPatrolRoute()
        {
            string[] offenders = _scanned
                .Where(row => row.MoveSpeed <= 0f && row.PatrolPoints.Length > 0)
                .Select(row => $"{row.Asset}: {row.Path} ({row.Config}, patrolPoints '{row.PatrolPoints}')")
                .OrderBy(offender => offender, StringComparer.Ordinal)
                .ToArray();

            Assert.That(offenders, Is.Empty,
                "A move speed of 0 means the enemy stands where it is placed, so a patrol route would never be walked " +
                "(give the enemy a moving archetype, or remove the route):\n" + string.Join("\n", offenders));
        }

        private static string[] DuplicateKeys(IEnumerable<EnemyTuningRow> rows)
            => rows
                .GroupBy(row => row.Asset + ": " + row.Path, StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .Select(group => $"{group.Key} (x{group.Count()})")
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();

        private static void AddCellDifferences(List<string> differences, string path, EnemyTuningRow expected, EnemyTuningRow actual)
        {
            string[] expectedCells = expected.ToCells();
            string[] actualCells = actual.ToCells();

            // Columns 0 and 1 are the key the rows were matched on.
            for (int column = 2; column < expectedCells.Length; column++)
            {
                if (!string.Equals(expectedCells[column], actualCells[column], StringComparison.Ordinal))
                    differences.Add($"{path}: {EnemyTuningRow.Columns[column]} expected '{expectedCells[column]}', found '{actualCells[column]}'");
            }
        }
    }
}
