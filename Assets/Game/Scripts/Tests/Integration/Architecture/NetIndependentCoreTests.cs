using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor.Compilation;

namespace Bw.Tests.Integration.Architecture
{
    public sealed class NetIndependentCoreTests
    {
        [Test]
        public void EntitiesReferenceNoNetworkAssembly()
        {
            var entities = CompilationPipeline.GetAssemblies(AssembliesType.Player).Single(assembly => assembly.name == "Bw.Entities");

            var networked = entities.assemblyReferences
                .Select(reference => reference.name)
                .Concat(entities.compiledAssemblyReferences.Select(Path.GetFileNameWithoutExtension))
                .Where(name => name.StartsWith("Unity.Netcode") || name.StartsWith("Unity.Networking") || name == "Bw.Entities.Network")
                .ToList();

            Assert.That(networked, Is.Empty);
        }
    }
}
