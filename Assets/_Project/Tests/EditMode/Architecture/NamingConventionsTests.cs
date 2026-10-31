using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;

namespace DreamVille.Tests.EditMode.Architecture
{
    /// <summary>
    /// Pins the naming rules of conventions §2 and §4 on the compiled runtime assemblies (spec 14 §5 items 4-13 and 16):
    /// no public instance fields, <c>_camelCase</c> private fields, PascalCase constants, <c>[SerializeField]</c> only on
    /// private fields and no leftover <c>[FormerlySerializedAs]</c>, events without an <c>On</c> prefix, ScriptableObjects named after their role with a
    /// <c>DreamVille/</c> menu, and type names without underscores, long capitalised acronyms or a <c>Manager</c> suffix. Reflection, not source text: modifiers and attributes are facts of the compiled types,
    /// and compiler-generated members (closures, async state machines, backing fields) are skipped.
    /// </summary>
    public class NamingConventionsTests
    {
        private const int RuntimeAssemblyCount = 14;

        private const BindingFlags DeclaredMembers =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        private static readonly Regex PascalCase = new Regex("^[A-Z][A-Za-z0-9]*$", RegexOptions.Compiled);
        private static readonly Regex UnderscoreCamelCase = new Regex("^_[a-z][A-Za-z0-9]*$", RegexOptions.Compiled);
        private static readonly Regex OnPrefix = new Regex("^On[A-Z]", RegexOptions.Compiled);

        // Two-letter acronyms stay upper case (UI), longer ones are PascalCase (Npc, Vfx, Hud): four capitals in a row
        // mean an acronym of three or more followed by the next word, three at the end mean a trailing one.
        private static readonly Regex LongAcronym = new Regex("[A-Z]{4}|[A-Z]{3}$", RegexOptions.Compiled);

        private static readonly string[] ScriptableObjectRoles = { "Config", "Definition", "Rules", "Catalog" };

        // Misspellings the code audits found in identifiers (the classes that carried most of them are gone);
        // the list records them and keeps them from coming back.
        private static readonly string[] KnownTypos =
        {
            "rigitbody", "interect", "interract", "hiden", "compleat", "damagable", "mumbel", "recive", "distannce",
            "baloon", "tovards", "rutine", "upwords", "movig", "foott", "volium", "commics",
        };

        private const string DreamVilleMenuPrefix = "DreamVille/";

        /// <summary>Every loaded DreamVille runtime assembly (the test assemblies excluded), ordered by name.</summary>
        internal static IReadOnlyList<Assembly> RuntimeAssemblies()
            => AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => IsRuntimeAssemblyName(assembly.GetName().Name))
                .OrderBy(assembly => assembly.GetName().Name, StringComparer.Ordinal)
                .ToArray();

        [Test]
        public void RuntimeAssemblies_AreAllLoaded()
        {
            string[] names = RuntimeAssemblies().Select(assembly => assembly.GetName().Name).ToArray();

            Assert.That(names, Has.Length.EqualTo(RuntimeAssemblyCount),
                "The reflection scans below would pass on an empty set; loaded runtime assemblies:\n" + string.Join("\n", names));
        }

        [Test]
        public void Types_DeclareNoPublicInstanceFields()
        {
            string[] offenders = UserFields()
                .Where(field => field.IsPublic && !field.IsStatic)
                .Select(KeyOf)
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();

            Assert.That(offenders, Is.Empty,
                "Public instance fields (expose a property; serialized data is [SerializeField] private):\n" + string.Join("\n", offenders));
        }

        [Test]
        public void Fields_FollowNamingRules()
        {
            var offenders = new List<string>();

            foreach (FieldInfo field in UserFields())
            {
                string key = KeyOf(field);

                if (IsConstant(field))
                {
                    if (!PascalCase.IsMatch(field.Name))
                        offenders.Add($"{key} (constants and static readonly fields are PascalCase)");
                }
                else if (!field.IsPublic && !UnderscoreCamelCase.IsMatch(field.Name))
                {
                    offenders.Add($"{key} (private and protected fields are _camelCase)");
                }
            }

            offenders.Sort(StringComparer.Ordinal);

            Assert.That(offenders, Is.Empty, "Fields that break conventions §2:\n" + string.Join("\n", offenders));
        }

        [Test]
        public void SerializeField_IsOnlyOnPrivateFields()
        {
            string[] offenders = UserFields()
                .Where(field => field.IsDefined(typeof(SerializeField), false) && !field.IsPrivate)
                .Select(KeyOf)
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();

            Assert.That(offenders, Is.Empty, "[SerializeField] on a non-private field (it is either redundant or exposes data):\n" + string.Join("\n", offenders));
        }

        // The attribute is a migration tool (ADR 0010): it lives only inside a rename batch's editor session. Backing
        // fields count too, since [field: FormerlySerializedAs] would hide there.
        [Test]
        public void Fields_CarryNoFormerlySerializedAs()
        {
            string[] offenders = UserTypes()
                .SelectMany(type => type.GetFields(DeclaredMembers))
                .Where(field => field.IsDefined(typeof(FormerlySerializedAsAttribute), false))
                .Select(KeyOf)
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();

            Assert.That(offenders, Is.Empty,
                "[FormerlySerializedAs] outside a rename batch (re-serialize the data, record the old name in RetiredSerializedFields, drop the attribute):\n" +
                string.Join("\n", offenders));
        }

        [Test]
        public void Events_HaveNoOnPrefix()
        {
            string[] offenders = UserTypes()
                .SelectMany(type => type.GetEvents(DeclaredMembers))
                .Where(evt => OnPrefix.IsMatch(evt.Name))
                .Select(evt => $"{evt.DeclaringType.FullName}.{evt.Name}")
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();

            Assert.That(offenders, Is.Empty, "Events named like handlers (an event is what happened; On<Event> is its handler):\n" + string.Join("\n", offenders));
        }

        [Test]
        public void ScriptableObjects_HaveRoleSuffixAndDreamVilleMenu()
        {
            var offenders = new List<string>();

            foreach (Type type in UserTypes().Where(type => typeof(ScriptableObject).IsAssignableFrom(type) && !type.IsAbstract))
            {
                if (!ScriptableObjectRoles.Any(role => type.Name.EndsWith(role, StringComparison.Ordinal)))
                    offenders.Add($"{type.FullName} (name must end with {string.Join(", ", ScriptableObjectRoles)})");

                var menu = type.GetCustomAttribute<CreateAssetMenuAttribute>(false);
                if (menu == null || menu.menuName == null || !menu.menuName.StartsWith(DreamVilleMenuPrefix, StringComparison.Ordinal))
                    offenders.Add($"{type.FullName} (needs [CreateAssetMenu] with a menuName under '{DreamVilleMenuPrefix}')");
            }

            offenders.Sort(StringComparer.Ordinal);

            Assert.That(offenders, Is.Empty, "ScriptableObject types that break conventions §7:\n" + string.Join("\n", offenders));
        }

        [Test]
        public void TypeNames_HaveNoUnderscoreOrLongAcronym()
        {
            string[] offenders = UserTypes()
                .Where(type => type.Name.Contains("_") || LongAcronym.IsMatch(NameForAcronymCheck(type)))
                .Select(type => type.FullName)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

            Assert.That(offenders, Is.Empty,
                "Type names with an underscore or an acronym longer than two letters in capitals (UI stays, Npc and Vfx are PascalCase):\n" +
                string.Join("\n", offenders));
        }

        [Test]
        public void TypeNames_DoNotEndWithManager()
        {
            string[] offenders = UserTypes()
                .Where(type => StripGenericArity(type.Name).EndsWith("Manager", StringComparison.Ordinal))
                .Select(type => type.FullName)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

            Assert.That(offenders, Is.Empty, "Types named 'Manager' (name the one job the type does):\n" + string.Join("\n", offenders));
        }

        [Test]
        public void Identifiers_AvoidKnownTypos()
        {
            var offenders = new List<string>();

            foreach (Type type in UserTypes())
            {
                AddIfMisspelled(offenders, type.FullName, type.Name);

                foreach (MemberInfo member in type.GetMembers(DeclaredMembers))
                {
                    if (member.Name.Contains("<"))
                        continue;

                    AddIfMisspelled(offenders, $"{type.FullName}.{member.Name}", member.Name);

                    if (member is MethodBase method)
                    {
                        foreach (ParameterInfo parameter in method.GetParameters())
                            AddIfMisspelled(offenders, $"{type.FullName}.{member.Name}({parameter.Name})", parameter.Name);
                    }
                }
            }

            Assert.That(offenders.Distinct().OrderBy(entry => entry, StringComparer.Ordinal), Is.Empty,
                "Identifiers with a known misspelling:\n" + string.Join("\n", offenders.Distinct()));
        }

        private static void AddIfMisspelled(List<string> offenders, string where, string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                return;

            foreach (string typo in KnownTypos)
            {
                if (identifier.IndexOf(typo, StringComparison.OrdinalIgnoreCase) >= 0)
                    offenders.Add($"{where} ('{typo}')");
            }
        }

        private static string NameForAcronymCheck(Type type)
        {
            string name = StripGenericArity(type.Name);

            // The leading I of an interface is a prefix, not part of an acronym: IUIView reads as I + UIView.
            return type.IsInterface && name.Length > 1 && name[0] == 'I' && char.IsUpper(name[1]) ? name.Substring(1) : name;
        }

        private static string StripGenericArity(string name)
        {
            int tick = name.IndexOf('`');
            return tick < 0 ? name : name.Substring(0, tick);
        }

        private static bool IsRuntimeAssemblyName(string name)
            => name.StartsWith("DreamVille.", StringComparison.Ordinal) && !name.StartsWith("DreamVille.Tests", StringComparison.Ordinal);

        private static IEnumerable<Type> UserTypes()
            => RuntimeAssemblies()
                .SelectMany(LoadableTypes)
                .Where(type => !IsCompilerGenerated(type));

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

        // Enum members are values, not fields in the naming sense; compiler-generated members have '<' in their names.
        private static IEnumerable<FieldInfo> UserFields()
            => UserTypes()
                .Where(type => !type.IsEnum)
                .SelectMany(type => type.GetFields(DeclaredMembers))
                .Where(field => !field.IsDefined(typeof(CompilerGeneratedAttribute), false) && !field.Name.Contains("<"));

        private static bool IsCompilerGenerated(Type type)
        {
            for (Type current = type; current != null; current = current.DeclaringType)
            {
                if (current.IsDefined(typeof(CompilerGeneratedAttribute), false) || current.Name.Contains("<"))
                    return true;
            }

            return false;
        }

        private static bool IsConstant(FieldInfo field) => field.IsLiteral || (field.IsStatic && field.IsInitOnly);

        private static string KeyOf(FieldInfo field) => $"{field.DeclaringType.FullName}.{field.Name}";
    }
}
