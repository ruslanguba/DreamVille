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
    /// Timed logic under <c>Assets/_Project/Scripts</c> waits through UniTask with an explicit token
    /// (<c>DreamVille.Core.Async</c>, ADR 0006), never through coroutines or string Invoke. Roadmap 5.4 retired the last
    /// coroutine files, so the allow-list is empty; it stays as the place for a deliberate exception, each entry with
    /// its reason.
    /// </summary>
    public class CoroutineGuardTests
    {
        // Coroutine APIs and their helpers, plus Invoke by method name. Member calls such as "Changed?.Invoke(...)"
        // are delegate invocations and stay allowed, as do generic IEnumerator<T> iterators and UniTask.WaitUntil.
        internal static readonly Regex CoroutineApi = new Regex(
            @"\b(?:StartCoroutine|StopCoroutine|StopAllCoroutines|InvokeRepeating|CancelInvoke)\s*\(" +
            @"|\bIEnumerator\b(?!\s*<)" +
            @"|\bWaitForSeconds(?:Realtime)?\b|\bWaitForEndOfFrame\b|\bWaitForFixedUpdate\b" +
            @"|\bnew\s+WaitUntil\b|\bnew\s+WaitWhile\b" +
            @"|(?<![.\w])Invoke\s*\(\s*(?:""|nameof\s*\()" +
            @"|\bCoroutine\b",
            RegexOptions.Compiled);

        private static readonly Regex BlockComment = new Regex(@"/\*.*?\*/", RegexOptions.Compiled | RegexOptions.Singleline);

        // Paths relative to Assets/_Project/Scripts. Deliberate exceptions only, each with a reason comment.
        private static readonly string[] AllowList = Array.Empty<string>();

        private static string ScriptsRoot => Path.Combine(Application.dataPath, "_Project", "Scripts");

        [Test]
        public void Scripts_OutsideAllowList_UseNoCoroutineApis()
        {
            string[] offenders = Directory.EnumerateFiles(ScriptsRoot, "*.cs", SearchOption.AllDirectories)
                .Select(RelativePath)
                .Where(path => !AllowList.Contains(path))
                .SelectMany(path => FindCoroutineApiLines(ReadScript(path)).Select(line => $"{path}:{line}"))
                .ToArray();

            Assert.That(offenders, Is.Empty,
                "Coroutine or string-Invoke use outside the allow-list; wait through DreamVille.Core.Async.GameTime instead.");
        }

        [Test]
        public void AllowList_EveryEntryStillUsesCoroutines()
        {
            string[] stale = AllowList
                .Where(path => !File.Exists(Path.Combine(ScriptsRoot, path)) || !FindCoroutineApiLines(ReadScript(path)).Any())
                .ToArray();

            Assert.That(stale, Is.Empty, "These files were converted or removed; drop them from the allow-list.");
        }

        [TestCase("StartCoroutine(Fade());")]
        [TestCase("StopCoroutine(_routine);")]
        [TestCase("StopAllCoroutines();")]
        [TestCase("private IEnumerator Fade()")]
        [TestCase("yield return new WaitForSeconds(1f);")]
        [TestCase("yield return new WaitForSecondsRealtime(1f);")]
        [TestCase("yield return new WaitForEndOfFrame();")]
        [TestCase("yield return new WaitForFixedUpdate();")]
        [TestCase("yield return new WaitUntil(() => ready);")]
        [TestCase("yield return new WaitWhile(() => busy);")]
        [TestCase("Invoke(\"Hide\", 2f);")]
        [TestCase("Invoke(nameof(Hide), 2f);")]
        [TestCase("InvokeRepeating(nameof(Tick), 0f, 1f);")]
        [TestCase("CancelInvoke();")]
        [TestCase("private Coroutine _routine;")]
        public void CoroutineApi_FlagsForbiddenCode(string line)
        {
            Assert.That(FindCoroutineApiLines(line), Is.EqualTo(new[] { 1 }));
        }

        [TestCase("Changed?.Invoke(value);")]
        [TestCase("_onDone.Invoke(nameof(Hide));")]
        [TestCase("IEnumerator<int> numbers = list.GetEnumerator();")]
        [TestCase("await UniTask.WaitUntil(() => handle.IsDone, cancellationToken: token);")]
        [TestCase("await GameTime.DelayAsync(_showDuration, token);")]
        [TestCase("// StartCoroutine(Fade()); the old way")]
        [TestCase("/* yield return new WaitForSeconds(1f); */")]
        [TestCase("public class CoroutineGuardTests")]
        public void CoroutineApi_IgnoresAllowedCode(string line)
        {
            Assert.That(FindCoroutineApiLines(line), Is.Empty);
        }

        private static string RelativePath(string fullPath)
            => fullPath.Substring(ScriptsRoot.Length + 1).Replace('\\', '/');

        private static string ReadScript(string relativePath)
            => File.ReadAllText(Path.Combine(ScriptsRoot, relativePath));

        // Line numbers (1-based) of coroutine API use in code; comments are ignored so XML docs may name the old APIs.
        internal static IEnumerable<int> FindCoroutineApiLines(string source)
        {
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
