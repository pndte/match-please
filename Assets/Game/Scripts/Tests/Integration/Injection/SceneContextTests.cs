using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bw.Entities.Network;
using NUnit.Framework;
using Setup;

namespace Bw.Tests.Integration.Injection
{
    [Category("Slow")]
    public sealed class SceneContextTests
    {
        private ValidationContainers _containers;

        [SetUp]
        public void SetUp() =>
            _containers = new ValidationContainers();

        [TearDown]
        public void TearDown() =>
            _containers.Dispose();

        private static IEnumerable<TestCaseData> LevelsOnBothPeers() =>
            ValidationContainers.Levels().SelectMany(level => new[] { PeerType.Server, PeerType.Client }
                .Select(peer => new TestCaseData(level, peer).SetName($"{{m}}({Path.GetFileNameWithoutExtension(level)}, {peer})")));

        [Test]
        public void SetupSceneResolvesOverTheProjectContextBeforeThePeerIsChosen()
        {
            var project = _containers.Project();
            project.ResolveRoots();

            var setup = _containers.Scene(ValidationContainers.SetupScene, project);

            Assert.DoesNotThrow(setup.Resolve);
        }

        [TestCase(PeerType.Server)]
        [TestCase(PeerType.Client)]
        public void NetworkSceneResolvesOverTheProjectContext(PeerType peer)
        {
            var project = _containers.Project();
            project.ResolveRoots();

            var network = _containers.Scene(ValidationContainers.NetworkScene, project, ValidationContainers.SessionOf(peer));

            Assert.DoesNotThrow(network.Resolve);
        }

        [TestCaseSource(nameof(LevelsOnBothPeers))]
        public void LevelResolvesOverTheNetworkScene(string level, PeerType peer)
        {
            _containers.UseGameNetworkManager();
            var project = _containers.Project();
            project.ResolveRoots();
            var network = _containers.Scene(ValidationContainers.NetworkScene, project, ValidationContainers.SessionOf(peer));
            network.Resolve();

            var scene = _containers.Scene(level, network.Container);

            Assert.DoesNotThrow(scene.Resolve);
        }

        [Test]
        public void ThereAreLevelsToValidate()
        {
            Assert.That(ValidationContainers.Levels(), Is.Not.Empty);
        }
    }
}
