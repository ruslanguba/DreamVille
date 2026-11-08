using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace DreamVille.Tests.EditMode.Architecture
{
    /// <summary>
    /// Pins roadmap 6.1: code, comments and logs under <c>Assets/_Project/Scripts</c> are in English, and every public
    /// type carries an XML summary, and a concrete class nobody derives from is <c>sealed</c>. Player-facing text lives in the Localization tables, never in scripts.
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

        [Test]
        public void ConcreteClasses_WithoutSubclasses_AreSealed()
        {
            // Subclasses are looked up in every DreamVille assembly, tests included: a base class a test fake derives from stays open.
            Type[] allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => assembly.GetName().Name.StartsWith("DreamVille.", StringComparison.Ordinal))
                .SelectMany(LoadableTypes)
                .ToArray();
            var baseTypes = new HashSet<Type>(allTypes
                .Where(type => type.BaseType != null)
                .Select(type => type.BaseType.IsGenericType ? type.BaseType.GetGenericTypeDefinition() : type.BaseType));

            string[] offenders = NamingConventionsTests.RuntimeAssemblies()
                .SelectMany(LoadableTypes)
                .Where(type => type.IsClass && !type.IsAbstract && !type.IsSealed && !IsCompilerGenerated(type))
                .Where(type => !baseTypes.Contains(type.IsGenericType ? type.GetGenericTypeDefinition() : type))
                .Select(type => type.FullName)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

            Assert.That(offenders, Is.Empty, "Concrete classes with no subclass must be sealed:\n" + string.Join("\n", offenders));
        }

        private static IEnumerable<Type> LoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return exception.Types.Where(type => type != null);
            }
        }

        private static bool IsCompilerGenerated(Type type)
        {
            for (Type current = type; current != null; current = current.DeclaringType)
            {
                if (current.IsDefined(typeof(CompilerGeneratedAttribute), false))
                    return true;
            }

            return false;
        }
    }
}
