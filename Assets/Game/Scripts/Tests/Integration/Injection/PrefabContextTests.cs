using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bw.Entities.Network;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Bw.Tests.Integration.Injection
{
    [Category("Slow")]
    [TestFixture(PeerType.Server)]
    [TestFixture(PeerType.Client)]
    public sealed class PrefabContextTests
    {
        private const string Level = "Assets/Resources/Scenes/MeatPlant.unity";

        private readonly PeerType _peer;

        private ValidationContainers _containers;
        private DiContainer _level;

        public PrefabContextTests(PeerType peer)
        {
            _peer = peer;
        }

        private static IEnumerable<string> ContextPrefabPaths() =>
            AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Game/Prefabs" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => AssetDatabase.LoadAssetAtPath<GameObject>(path).TryGetComponent<GameObjectContext>(out _))
                .OrderBy(path => path, StringComparer.Ordinal);

        private static IEnumerable<TestCaseData> ContextPrefabs() =>
            ContextPrefabPaths().Select(path => new TestCaseData(path).SetName($"{{m}}({Path.GetFileNameWithoutExtension(path)})"));

        [OneTimeSetUp]
        public void BuildTheLevel()
        {
            _containers = new ValidationContainers();
            _containers.UseGameNetworkManager();
            var project = _containers.Project(ValidationContainers.SettingsOf(_peer));
            project.ResolveRoots();
            var network = _containers.Scene(ValidationContainers.NetworkScene, project);
            network.Resolve();
            var level = _containers.Scene(Level, network.Container);
            level.Resolve();
            _level = level.Container;
        }

        [OneTimeTearDown]
        public void CloseTheLevel() =>
            _containers.Dispose();

        [Test]
        public void ThereArePrefabsToValidate()
        {
            Assert.That(ContextPrefabPaths(), Is.Not.Empty);
        }

        [TestCaseSource(nameof(ContextPrefabs))]
        public void PrefabContextResolvesInTheLevel(string path)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var context = root.GetComponent<GameObjectContext>();

                context.Install(_level);

                Assert.DoesNotThrow(context.Container.ResolveRoots);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
