using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor.Compilation;

namespace Bw.Tests.Integration.Architecture
{
    public sealed class ViewGateTests
    {
        private const string Infrastructure = "Bw.Entities";
        private const string CoreAdapters = "Bw.CoreAdapters";

        private static readonly Regex View = new(@"^Bw\.UseCases\.\w+\.View$");
        private static readonly Regex Adapters = new(@"^Bw\.UseCases\.\w+\.View\.Adapters$");
        private static readonly Regex ViewNamespace = new(@"^Bw\.UseCases\.\w+\.View(\.\w+)*$");
        private static readonly Regex AdapterNamespace = new(@"^Bw\.UseCases\.\w+\.View\.Adapters(\.\w+)*$");
        private static readonly Regex Network = new(@"^Bw\.UseCases\.\w+\.Network$");
        private static readonly Regex NamespaceLine = new(@"^\s*namespace\s+([\w.]+)", RegexOptions.Multiline);
        private static readonly Regex TypeDeclaration = new(@"\b(?:class|interface|struct|enum|record)\s+(\w+)");
        private static readonly Regex Qualified = new(@"\bBw(?:\.\w+)+");
        private static readonly Regex Using = new(@"^\s*using\s+(?:static\s+)?(?:\w+\s*=\s*)?(Bw(?:\.\w+)+)\s*;", RegexOptions.Multiline);
        private static readonly Regex Identifier = new(@"\b[A-Za-z_]\w*\b");
        private static readonly Regex BlockComment = new(@"/\*.*?\*/", RegexOptions.Singleline);
        private static readonly Regex LineComment = new(@"//[^\n]*");
        private static readonly Regex VerbatimString = new(@"@""(?:[^""]|"""")*""");
        private static readonly Regex PlainString = new(@"\$?""(?:\\.|[^""\\\n])*""");

        private static readonly HashSet<string> AdapterUsers = new() { "Bw.Injection" };
        private static readonly HashSet<string> CoreAdapterUsers = new() { "Bw.Injection", "Bw.Setup" };
        private static readonly HashSet<string> NetworkAssembliesWithoutSuffix = new() { "Bw.UseCases", "Bw.UseCases.Spawning" };
        private static readonly HashSet<string> Keywords = new() { "where", "new", "class", "struct", "unmanaged", "notnull", "default" };

        private static readonly HashSet<string> OpenNamespaces = new()
        {
            "Bw.Entities.Extensions",
            "Bw.Entities.Infrastructure",
            "Bw.Entities.Pool",
            "Bw.Entities.Pool.GameObjects",
            "Bw.Entities.Pool.GameObjects.Modes",
            "Bw.Entities.Utilities",
        };

        private static readonly Dictionary<string, HashSet<string>> OpenTypes = new()
        {
            ["Bw.Entities"] = new HashSet<string> { "IHolder", "LifetimeHolder", "IReadonlyViewableList", "BwViewableList" },
            ["Bw.Entities.Simulation"] = new HashSet<string> { "IStateView", "CompositeStateView" },
            ["Bw.Entities.Physics"] = new HashSet<string> { "BodyBox" },
        };

        [Test]
        public void GateSeesTheViewAndAdapterAssemblies()
        {
            var names = ProductionAssemblies().Select(assembly => assembly.name).ToList();

            Assert.That(names.Count(View.IsMatch), Is.GreaterThan(0));
            Assert.That(names.Count(Adapters.IsMatch), Is.GreaterThan(0));
            Assert.That(names, Has.Member(Infrastructure));
        }

        [Test]
        public void AdapterAssembliesAreReferencedOnlyByInjection()
        {
            var findings = new List<string>();
            foreach (var assembly in ProductionAssemblies())
                foreach (var reference in ReferencesOf(assembly))
                    if (Adapters.IsMatch(reference) && !AdapterUsers.Contains(assembly.name))
                        findings.Add($"{assembly.name} references {reference}; only {string.Join(", ", AdapterUsers.OrderBy(user => user))} wires adapters");

            Assert.That(findings, Is.Empty, string.Join("\n", findings));
        }

        [Test]
        public void ViewAssembliesReferenceOnlyViewsAndTheInfrastructure()
        {
            var findings = new List<string>();
            foreach (var assembly in ProductionAssemblies().Where(assembly => View.IsMatch(assembly.name)))
                foreach (var reference in ReferencesOf(assembly))
                    if (reference.StartsWith("Bw.") && !View.IsMatch(reference) && reference != Infrastructure)
                        findings.Add($"view {assembly.name} references {reference}, which is not a view");

            Assert.That(findings, Is.Empty, string.Join("\n", findings));
        }

        [Test]
        public void CoreAdaptersAreReferencedOnlyByCompositionNetworkAndAdapterAssemblies()
        {
            var findings = new List<string>();
            foreach (var assembly in ProductionAssemblies())
                foreach (var reference in ReferencesOf(assembly))
                    if (IsCoreAdapters(reference) && !UsesCoreAdapters(assembly.name))
                        findings.Add($"{assembly.name} references {reference}; only feature adapters, {string.Join(", ", CoreAdapterUsers.OrderBy(user => user))} may");

            Assert.That(findings, Is.Empty, string.Join("\n", findings));
        }

        [Test]
        public void ViewSourcesNameOnlyNamespacesOpenToViews()
        {
            var findings = new List<string>();
            foreach (var (path, text) in ViewSources())
            {
                var lines = text.Split('\n');
                for (var index = 0; index < lines.Length; index++)
                {
                    if (NamespaceLine.IsMatch(lines[index]))
                        continue;

                    var imported = Using.IsMatch(lines[index]);
                    foreach (Match qualified in Qualified.Matches(lines[index]))
                        if (TryFindNamespaceProblem(qualified.Value, imported, out var problem))
                            findings.Add($"{path}:{index + 1} {problem}");
                }
            }

            Assert.That(findings, Is.Empty, string.Join("\n", findings));
        }

        [Test]
        public void ViewSourcesUseNoClosedTypeOfAPartlyOpenNamespace()
        {
            var closed = ClosedTypes();
            var findings = new List<string>();
            foreach (var (path, text) in ViewSources())
            {
                var imported = Using.Matches(text).Select(match => match.Groups[1].Value).Distinct().ToList();
                var lines = text.Split('\n');
                for (var index = 0; index < lines.Length; index++)
                {
                    if (NamespaceLine.IsMatch(lines[index]))
                        continue;

                    foreach (Match identifier in Identifier.Matches(lines[index]))
                        foreach (var namespaceName in imported)
                            if (closed.TryGetValue(namespaceName, out var types) && types.Contains(identifier.Value))
                                findings.Add($"{path}:{index + 1} {identifier.Value} from {namespaceName} is not open to views");
                }
            }

            Assert.That(findings, Is.Empty, string.Join("\n", findings));
        }

        private static bool TryFindNamespaceProblem(string name, bool imported, out string problem)
        {
            problem = null;
            if (AdapterNamespace.IsMatch(name))
            {
                problem = $"{name} is an adapter, views never see adapters";
                return true;
            }

            if (ViewNamespace.IsMatch(name) || OpenNamespaces.Contains(name) || OpenTypes.ContainsKey(name))
                return false;

            if (!imported)
            {
                var split = name.LastIndexOf('.');
                var prefix = name.Substring(0, split);
                var last = name.Substring(split + 1);
                if (ViewNamespace.IsMatch(prefix) || OpenNamespaces.Contains(prefix) || (OpenTypes.TryGetValue(prefix, out var open) && open.Contains(last)))
                    return false;
            }

            problem = $"{name} is not open to views";
            return true;
        }

        private static Dictionary<string, HashSet<string>> ClosedTypes()
        {
            var declared = new Dictionary<string, HashSet<string>>();
            foreach (var path in ProductionAssemblies().Single(assembly => assembly.name == Infrastructure).sourceFiles)
            {
                var text = Stripped(File.ReadAllText(Path.GetFullPath(path)));
                var namespaces = NamespaceLine.Match(text);
                if (!namespaces.Success)
                    continue;

                if (!declared.TryGetValue(namespaces.Groups[1].Value, out var types))
                    declared[namespaces.Groups[1].Value] = types = new HashSet<string>();

                foreach (Match type in TypeDeclaration.Matches(text))
                    if (!Keywords.Contains(type.Groups[1].Value))
                        types.Add(type.Groups[1].Value);
            }

            var closed = new Dictionary<string, HashSet<string>>();
            foreach (var (namespaceName, open) in OpenTypes)
                closed[namespaceName] = declared.TryGetValue(namespaceName, out var types) ? new HashSet<string>(types.Except(open)) : new HashSet<string>();

            return closed;
        }

        private static IEnumerable<(string Path, string Text)> ViewSources() =>
            ProductionAssemblies()
                .Where(assembly => View.IsMatch(assembly.name))
                .SelectMany(assembly => assembly.sourceFiles)
                .Select(path => (path, Stripped(File.ReadAllText(Path.GetFullPath(path)))));

        private static string Stripped(string text)
        {
            text = BlockComment.Replace(text, match => new string('\n', match.Value.Count(character => character == '\n')));
            text = LineComment.Replace(text, string.Empty);
            text = VerbatimString.Replace(text, match => "\"\"" + new string('\n', match.Value.Count(character => character == '\n')));
            return PlainString.Replace(text, "\"\"");
        }

        private static bool IsCoreAdapters(string name) =>
            name == CoreAdapters || name.StartsWith(CoreAdapters + ".");

        private static bool UsesCoreAdapters(string name) =>
            CoreAdapterUsers.Contains(name) || NetworkAssembliesWithoutSuffix.Contains(name) || Network.IsMatch(name) || Adapters.IsMatch(name);

        private static IEnumerable<string> ReferencesOf(Assembly assembly) =>
            assembly.assemblyReferences.Select(reference => reference.name);

        private static IEnumerable<Assembly> ProductionAssemblies() =>
            CompilationPipeline.GetAssemblies(AssembliesType.Player).Where(assembly => assembly.name.StartsWith("Bw."));
    }
}
