using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

using DreamVille.Configs.Enemies;
using DreamVille.Gameplay.Enemies;

namespace DreamVille.Tests.EditMode.Enemies
{
    /// <summary>
    /// The serialized field names the tuning snapshot reads from <see cref="Enemy"/>. The five tuning numbers are not among
    /// them: they come from the <see cref="EnemyConfig"/> the enemy references. Renaming one of these fields means changing
    /// exactly these constants: the TSV columns keep their names and values.
    /// </summary>
    internal static class EnemyTuningFields
    {
        public const string Config = "_config";
        public const string PatrolPoints = "_patrolPoints";
        public const string AttackSpawn = "_attackSpawn";
    }

    /// <summary>
    /// One enemy of one scene or prefab: its key (asset and hierarchy path) and the tuning it resolves to through its config.
    /// The five tuning numbers are the <see cref="EnemyConfig"/> values, or NaN when the enemy has no config.
    /// </summary>
    internal readonly struct EnemyTuningRow
    {
        /// <summary>The TSV header and the column order of <see cref="ToCells"/>.</summary>
        public static readonly IReadOnlyList<string> Columns = new[]
        {
            "asset", "path", "moveSpeed", "chaseRange", "attackRange", "attackCooldown", "rotationSpeed", "attackSpawn", "patrolPoints", "config"
        };

        public EnemyTuningRow(string asset, string path, float moveSpeed, float chaseRange, float attackRange,
            float attackCooldown, float rotationSpeed, string attackSpawn, string patrolPoints, string config)
        {
            Asset = asset;
            Path = path;
            MoveSpeed = moveSpeed;
            ChaseRange = chaseRange;
            AttackRange = attackRange;
            AttackCooldown = attackCooldown;
            RotationSpeed = rotationSpeed;
            AttackSpawn = attackSpawn;
            PatrolPoints = patrolPoints;
            Config = config;
        }

        /// <summary>Path under <c>Assets/_Project/</c> without the extension, for example <c>Scenes/Max/Level_2_Max</c>.</summary>
        public string Asset { get; }

        /// <summary>Hierarchy path from the scene or prefab root; every member of a same-name sibling group carries a zero-based <c>[n]</c> suffix.</summary>
        public string Path { get; }

        public float MoveSpeed { get; }

        public float ChaseRange { get; }

        public float AttackRange { get; }

        public float AttackCooldown { get; }

        public float RotationSpeed { get; }

        /// <summary>Path relative to the enemy root: <c>.</c> is the enemy itself, <c>-</c> means unassigned.</summary>
        public string AttackSpawn { get; }

        /// <summary>Hierarchy paths in the same asset joined with <c>;</c>, <c>null</c> for an empty slot, an empty string for no points.</summary>
        public string PatrolPoints { get; }

        /// <summary>File name without extension of the <see cref="EnemyConfig"/> asset the enemy references, <c>-</c> when unassigned.</summary>
        public string Config { get; }

        public static EnemyTuningRow Parse(string tsvLine, int lineNumber)
        {
            string[] cells = tsvLine.Split('\t');
            if (cells.Length != Columns.Count)
                throw new FormatException($"{EnemyTuningSnapshot.FileName} line {lineNumber}: expected {Columns.Count} columns, found {cells.Length}.");

            float Number(int column) => ParseFloat(cells[column], Columns[column], lineNumber);

            return new EnemyTuningRow(
                asset: cells[0],
                path: cells[1],
                moveSpeed: Number(2),
                chaseRange: Number(3),
                attackRange: Number(4),
                attackCooldown: Number(5),
                rotationSpeed: Number(6),
                attackSpawn: cells[7],
                patrolPoints: cells[8],
                config: cells[9]);
        }

        /// <summary>The columns in <see cref="Columns"/> order, floats as <c>R</c> in the invariant culture.</summary>
        public string[] ToCells() => new[]
        {
            Asset, Path, Format(MoveSpeed), Format(ChaseRange), Format(AttackRange), Format(AttackCooldown), Format(RotationSpeed),
            AttackSpawn, PatrolPoints, Config
        };

        public string ToTsv() => string.Join("\t", ToCells());

        private static string Format(float value) => value.ToString("R", CultureInfo.InvariantCulture);

        private static float ParseFloat(string text, string column, int lineNumber)
        {
            if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                throw new FormatException($"{EnemyTuningSnapshot.FileName} line {lineNumber}: column '{column}' is not a number: '{text}'.");

            return value;
        }
    }

    /// <summary>
    /// Reads <c>EnemyTuningSnapshot.tsv</c> (UTF-8 without BOM, a header row, then one <see cref="EnemyTuningRow"/> per line):
    /// the designer values of every enemy in every scene and prefab, as they were when the file was last changed on purpose.
    /// </summary>
    internal static class EnemyTuningSnapshot
    {
        public const string FileName = "EnemyTuningSnapshot.tsv";

        private static string FilePath
            => Path.Combine(Application.dataPath, "_Project", "Tests", "EditMode", "Enemies", FileName);

        public static IReadOnlyList<EnemyTuningRow> Load()
        {
            string[] lines = File.ReadAllLines(FilePath, new UTF8Encoding(false));

            string header = string.Join("\t", EnemyTuningRow.Columns);
            if (lines.Length == 0 || lines[0] != header)
                throw new FormatException($"{FileName} line 1: expected the header '{header}'.");

            var rows = new List<EnemyTuningRow>(lines.Length - 1);

            // Blank lines can only be a trailing newline: no cell contains a line break.
            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].Length == 0)
                    continue;

                rows.Add(EnemyTuningRow.Parse(lines[i], i + 1));
            }

            return rows;
        }

        /// <summary>Every asset that has rows in the snapshot, in file order.</summary>
        public static IEnumerable<string> Assets() => Load().Select(row => row.Asset).Distinct().ToArray();

        public static EnemyTuningRow[] RowsOf(string asset) => Load().Where(row => row.Asset == asset).ToArray();
    }

    /// <summary>
    /// Builds <see cref="EnemyTuningRow"/>s from the real assets. Placement and the config reference are read through
    /// <see cref="SerializedObject"/> with the names in <see cref="EnemyTuningFields"/>, so the result is what the scene or prefab
    /// resolves to, overrides included; the five tuning numbers are then read from the referenced <see cref="EnemyConfig"/>.
    /// Scenes are opened Single: the caller guards against a dirty scene and restores the user's scene afterwards.
    /// </summary>
    internal static class EnemyTuningReader
    {
        private const string ProjectFolderPrefix = "Assets/_Project/";
        private const string SelfPath = ".";
        private const string Unassigned = "-";
        private const string EmptySlot = "null";

        /// <summary>The snapshot key of an asset: its path under <c>Assets/_Project/</c> without the extension.</summary>
        public static string AssetKeyOf(string assetPath)
        {
            if (!assetPath.StartsWith(ProjectFolderPrefix, StringComparison.Ordinal))
                throw new ArgumentException($"'{assetPath}' is not under {ProjectFolderPrefix}.", nameof(assetPath));

            return Path.ChangeExtension(assetPath.Substring(ProjectFolderPrefix.Length), null);
        }

        /// <summary>Every <see cref="Enemy"/> in the scene or prefab, inactive objects and nested prefab instances included, sorted by path.</summary>
        public static EnemyTuningRow[] Read(string assetPath)
        {
            string asset = AssetKeyOf(assetPath);
            GameObject[] roots = LoadRoots(assetPath);
            var paths = new HierarchyPaths(assetPath, roots);

            return roots
                .SelectMany(root => root.GetComponentsInChildren<Enemy>(true))
                .Select(enemy => ReadRow(asset, paths, enemy))
                .OrderBy(row => row.Path, StringComparer.Ordinal)
                .ToArray();
        }

        private static GameObject[] LoadRoots(string assetPath)
        {
            if (assetPath.EndsWith(".unity", StringComparison.Ordinal))
                return EditorSceneManager.OpenScene(assetPath, OpenSceneMode.Single).GetRootGameObjects();

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
                throw new InvalidOperationException($"{assetPath}: the prefab cannot be loaded.");

            return new[] { prefab };
        }

        private static EnemyTuningRow ReadRow(string asset, HierarchyPaths paths, Enemy enemy)
        {
            string path = paths.PathOf(enemy.transform);
            string where = $"{asset}: {path}";

            using var enemyObject = new SerializedObject(enemy);

            var config = Property(enemyObject, EnemyTuningFields.Config, where).objectReferenceValue as EnemyConfig;

            return new EnemyTuningRow(
                asset: asset,
                path: path,
                moveSpeed: TuningOf(config, tuned => tuned.MoveSpeed),
                chaseRange: TuningOf(config, tuned => tuned.ChaseRange),
                attackRange: TuningOf(config, tuned => tuned.AttackRange),
                attackCooldown: TuningOf(config, tuned => tuned.AttackCooldown),
                rotationSpeed: TuningOf(config, tuned => tuned.RotationSpeed),
                attackSpawn: AttackSpawnOf(Property(enemyObject, EnemyTuningFields.AttackSpawn, where), enemy.transform, paths),
                patrolPoints: PatrolPointsOf(Property(enemyObject, EnemyTuningFields.PatrolPoints, where), paths),
                config: config == null ? Unassigned : Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(config)));
        }

        private static float TuningOf(EnemyConfig config, Func<EnemyConfig, float> read) => config == null ? float.NaN : read(config);

        private static SerializedProperty Property(SerializedObject owner, string name, string where)
            => owner.FindProperty(name)
               ?? throw new InvalidOperationException(
                   $"{where}: {owner.targetObject.GetType().Name} has no serialized field '{name}'. Was it renamed without updating {nameof(EnemyTuningFields)}?");

        private static string AttackSpawnOf(SerializedProperty property, Transform enemyRoot, HierarchyPaths paths)
        {
            var spawn = property.objectReferenceValue as Transform;

            return spawn == null ? Unassigned : paths.RelativePath(enemyRoot, spawn);
        }

        private static string PatrolPointsOf(SerializedProperty property, HierarchyPaths paths)
        {
            var points = new string[property.arraySize];

            for (int i = 0; i < points.Length; i++)
            {
                var point = property.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
                points[i] = point == null ? EmptySlot : paths.PatrolPointOf(point);
            }

            return string.Join(";", points);
        }

        /// <summary>
        /// Hierarchy paths inside one scene or prefab asset. Every member of a same-name sibling group
        /// (children of one parent, or the scene roots) gets a zero-based <c>[n]</c> suffix.
        /// </summary>
        private sealed class HierarchyPaths
        {
            private readonly string _assetPath;
            private readonly IReadOnlyList<GameObject> _roots;

            public HierarchyPaths(string assetPath, IReadOnlyList<GameObject> roots)
            {
                _assetPath = assetPath;
                _roots = roots;
            }

            public string PathOf(Transform target)
            {
                string segment = SegmentOf(target);

                return target.parent == null ? segment : PathOf(target.parent) + "/" + segment;
            }

            /// <summary>The path below <paramref name="root"/>; <c>.</c> for the root itself, <c>abs:</c> plus a reference when the target is not below it.</summary>
            public string RelativePath(Transform root, Transform target)
            {
                if (target == root)
                    return SelfPath;

                var segments = new List<string>();
                Transform current = target;

                for (; current != null && current != root; current = current.parent)
                    segments.Add(SegmentOf(current));

                if (current != root)
                    return "abs:" + ReferenceTo(target);

                segments.Reverse();

                return string.Join("/", segments);
            }

            /// <summary>The plain path when the point lives in this asset, a qualified reference when it points into another asset.</summary>
            public string PatrolPointOf(Transform point)
                => IsInThisAsset(AssetDatabase.GetAssetPath(point.gameObject)) ? PathOf(point) : ReferenceTo(point);

            private string ReferenceTo(Transform target)
            {
                string owner = AssetDatabase.GetAssetPath(target.gameObject);
                if (IsInThisAsset(owner))
                    return "scene:" + PathOf(target) + "|Transform";

                return "asset:" + owner + "#" + new HierarchyPaths(owner, Array.Empty<GameObject>()).PathOf(target) + "|Transform";
            }

            // Scene objects have no asset path; objects of a prefab asset report that prefab's path.
            private bool IsInThisAsset(string ownerAssetPath) => string.IsNullOrEmpty(ownerAssetPath) || ownerAssetPath == _assetPath;

            private string SegmentOf(Transform target)
            {
                IEnumerable<Transform> siblings = target.parent != null
                    ? target.parent.Cast<Transform>()
                    : _roots.Select(root => root.transform);

                int sameName = 0;
                int index = 0;

                foreach (Transform sibling in siblings)
                {
                    if (sibling.name != target.name)
                        continue;

                    if (sibling == target)
                        index = sameName;

                    sameName++;
                }

                return sameName > 1 ? $"{target.name}[{index}]" : target.name;
            }
        }
    }
}
