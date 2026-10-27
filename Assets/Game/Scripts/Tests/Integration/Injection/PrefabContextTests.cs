using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bw.CoreAdapters.Weapons;
using Bw.Entities.Network;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Spawning.Network;
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
        private const string Level = "Assets/Game/Scenes/MeatPlant.unity";

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
            var project = _containers.Project();
            project.ResolveRoots();
            var network = _containers.Scene(ValidationContainers.NetworkScene, project, ValidationContainers.SessionOf(_peer));
            network.Resolve();
            var level = _containers.Scene(Level, network.Container);
            level.Resolve();
            _level = level.Container;
        }

        [OneTimeTearDown]
        public void CloseTheLevel() =>
            _containers.Dispose();

        private static IEnumerable<TestCaseData> WeaponPrefabs()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<WeaponCatalog>("Assets/Game/Content/Weapons/WeaponCatalog.asset");
            var crates = AssetDatabase.LoadAssetAtPath<CrateSpawnConfig>("Assets/Game/Content/Crates/CrateSpawnConfig.asset");
            return catalog.Weapons
                .Select(entry => entry.Prefab)
                .Append(crates.Prefab)
                .Select(prefab => new TestCaseData(AssetDatabase.GetAssetPath(prefab)).SetName($"{{m}}({prefab.name})"));
        }

        [TestCaseSource(nameof(WeaponPrefabs))]
        public void WeaponPrefabHoldsItsWeaponAfterTheInstall(string path)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var context = root.GetComponent<GameObjectContext>();

                context.Install(_level);

                Assert.That(context.Container.HasBinding<WeaponHolder>(), Is.True, "Armory, the crate spawner and the client's kind registration find the weapon through its WeaponHolder");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

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
