using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace DreamVille.Tests.EditMode.Architecture
{
    /// <summary>
    /// Pins roadmap 6.1: code, comments and logs under <c>Assets/_Project/Scripts</c> are in English, and every public
    /// type carries an XML summary. Player-facing text lives in the Localization tables, never in scripts.
    /// </summary>
    public class CodeConventionsTests
    {
        private static readonly Regex Cyrillic = new Regex("[Ѐ-ӿ]", RegexOptions.Compiled);

        private static readonly Regex PublicTypeDeclaration = new Regex(
            @"^\s*public\s+(?:sealed\s+|static\s+|abstract\s+|partial\s+|readonly\s+)*(?:class|struct|interface|enum|record)\s+\w+",
            RegexOptions.Compiled);

        private static string ScriptsRoot => Path.Combine(Application.dataPath, "_Project", "Scripts");

        private static IEnumerable<string> ScriptPaths()
            => Directory.EnumerateFiles(ScriptsRoot, "*.cs", SearchOption.AllDirectories);

        private static string RelativePath(string fullPath)
            => fullPath.Substring(ScriptsRoot.Length + 1).Replace('\\', '/');

        [Test]
        public void Scripts_ContainNoCyrillic()
        {
            string[] offenders = ScriptPaths()
                .SelectMany(path => File.ReadAllLines(path)
                    .Select((line, index) => (line, index))
                    .Where(entry => Cyrillic.IsMatch(entry.line))
                    .Select(entry => $"{RelativePath(path)}:{entry.index + 1}"))
                .ToArray();

            Assert.That(offenders, Is.Empty, "Russian text in scripts (comments and logs are English; player text goes to a Localization table):\n" + string.Join("\n", offenders));
        }

        [Test]
        public void PublicTypes_HaveXmlSummaries()
        {
            var offenders = new List<string>();
            foreach (string path in ScriptPaths())
            {
                string[] lines = File.ReadAllLines(path);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (!PublicTypeDeclaration.IsMatch(lines[i]))
                        continue;

                    int previous = i - 1;
                    while (previous >= 0 && lines[previous].TrimStart().StartsWith("["))
                        previous--;

                    if (previous < 0 || !lines[previous].TrimStart().StartsWith("///"))
                        offenders.Add($"{RelativePath(path)}:{i + 1}");
                }
            }

            Assert.That(offenders, Is.Empty, "Public types without an XML summary:\n" + string.Join("\n", offenders));
        }
    }
}
