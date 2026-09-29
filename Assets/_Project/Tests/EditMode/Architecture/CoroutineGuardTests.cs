using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace DreamVille.Tests.EditMode.Architecture
{
    /// <summary>
    /// Guards spec 08 §5 group F: timed logic under <c>Assets/_Project/Scripts</c> waits through UniTask with an explicit
    /// token, never through coroutines or string Invoke. The UI classes below are the only exceptions; roadmap 5.4
    /// rewrites them, and the second test forces the list to shrink as it does.
    /// </summary>
    public class CoroutineGuardTests
    {
        private static readonly Regex CoroutineApi = new Regex(
            @"\bStartCoroutine\s*\(|\bStopCoroutine\s*\(|\bIEnumerator\b|\bWaitForSeconds\b|\bInvoke\s*\(\s*""|\bInvokeRepeating\s*\(",
            RegexOptions.Compiled);

        private static readonly Regex BlockComment = new Regex(@"/\*.*?\*/", RegexOptions.Compiled | RegexOptions.Singleline);

        // Paths relative to Assets/_Project/Scripts. May only shrink.
        private static readonly string[] AllowList =
        {
            "UI/OnText.cs",                  // 5.4 ProximityHint
            "UI/Hints/HintDestroyOnTime.cs", // 5.4 ProximityHint
            "UI/FadeCanvasText.cs",          // 5.4 UiFader (host prefab unused)
        };

        private static string ScriptsRoot => Path.Combine(Application.dataPath, "_Project", "Scripts");

        [Test]
        public void Scripts_OutsideAllowList_UseNoCoroutineApis()
        {
            string[] offenders = Directory.EnumerateFiles(ScriptsRoot, "*.cs", SearchOption.AllDirectories)
                .Select(RelativePath)
                .Where(path => !AllowList.Contains(path))
                .SelectMany(path => FindCoroutineApis(path).Select(line => $"{path}:{line}"))
                .ToArray();

            Assert.That(offenders, Is.Empty,
                "Coroutine or string-Invoke use outside the allow-list; wait through DreamVille.Core.Async.GameTime instead.");
        }

        [Test]
        public void AllowList_EveryEntryStillUsesCoroutines()
        {
            string[] stale = AllowList
                .Where(path => !File.Exists(Path.Combine(ScriptsRoot, path)) || !FindCoroutineApis(path).Any())
                .ToArray();

            Assert.That(stale, Is.Empty, "These files were converted or removed; drop them from the allow-list.");
        }

        private static string RelativePath(string fullPath)
            => fullPath.Substring(ScriptsRoot.Length + 1).Replace('\\', '/');

        // Line numbers (1-based) of coroutine API use in code; comments are ignored so XML docs may name the old APIs.
        private static IEnumerable<int> FindCoroutineApis(string relativePath)
        {
            string source = File.ReadAllText(Path.Combine(ScriptsRoot, relativePath));
            string withoutBlockComments = BlockComment.Replace(source, match => new string('\n', match.Value.Count(c => c == '\n')));
            string[] lines = withoutBlockComments.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                int comment = line.IndexOf("//", StringComparison.Ordinal);
                string code = comment >= 0 ? line.Substring(0, comment) : line;

                if (CoroutineApi.IsMatch(code))
                    yield return i + 1;
            }
        }
    }
}
