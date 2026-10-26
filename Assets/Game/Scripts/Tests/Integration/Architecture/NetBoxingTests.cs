using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;
using UnityEditor.Compilation;

namespace Bw.Tests.Integration.Architecture
{
    public sealed class NetBoxingTests
    {
        private const string NetFramework = "Bw.Entities.Network";

        [Test]
        public void ScanReadsTheCompiledNetFramework()
        {
            using var module = ReadNetFramework();

            Assert.That(module.GetTypes().Select(type => type.FullName), Has.Member("Bw.Entities.Network.Rollback.RollbackStepper"));
        }

        [Test]
        public void NetFrameworkNeverBoxesOrUnboxes()
        {
            using var module = ReadNetFramework();

            var found = new List<string>();
            foreach (var type in module.GetTypes())
                foreach (var method in type.Methods.Where(method => method.HasBody))
                    foreach (var instruction in method.Body.Instructions)
                        if (instruction.OpCode == OpCodes.Box || instruction.OpCode == OpCodes.Unbox || instruction.OpCode == OpCodes.Unbox_Any)
                            found.Add($"{method.FullName}  {instruction.OpCode} {instruction.Operand}");

            Assert.That(found, Is.Empty, string.Join("\n", found));
        }

        private static ModuleDefinition ReadNetFramework()
        {
            var assembly = CompilationPipeline.GetAssemblies(AssembliesType.Editor).Single(candidate => candidate.name == NetFramework);
            return ModuleDefinition.ReadModule(Path.GetFullPath(assembly.outputPath), new ReaderParameters { InMemory = true, ReadingMode = ReadingMode.Deferred });
        }
    }
}
