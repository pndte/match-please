using Bw.CoreAdapters.Weapons;
using Bw.Tests.Doubles;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Spawning.Network;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

namespace Bw.Tests.UseCases.Spawning.Network
{
    public sealed class WeaponPrefabKindsTests
    {
        private HiddenObjects _objects;
        private WeaponCatalog _catalog;
        private NetworkObject _fists;
        private NetworkObject _pistol;

        [SetUp]
        public void SetUp()
        {
            _objects = new HiddenObjects();
            _fists = _objects.Create("Fists").AddComponent<NetworkObject>();
            _pistol = _objects.Create("Pistol").AddComponent<NetworkObject>();
            _catalog = ScriptableObject.CreateInstance<WeaponCatalog>();
            _catalog.Weapons = new[]
            {
                new WeaponEntry { Prefab = _fists, Kind = WeaponKind.Fists },
                new WeaponEntry { Prefab = _pistol, Kind = WeaponKind.Gun }
            };
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_catalog);
            _objects.Dispose();
        }

        [Test]
        public void CatalogRowsGiveTheirPrefabsTheirKinds()
        {
            var kinds = WeaponPrefabKinds.Of(_catalog, new NetworkObject[0]);

            Assert.That(kinds.ByPrefab[_fists.gameObject], Is.EqualTo(WeaponKind.Fists));
            Assert.That(kinds.ByPrefab[_pistol.gameObject], Is.EqualTo(WeaponKind.Gun));
            Assert.That(kinds.ByPrefab.Count, Is.EqualTo(2));
        }

        [Test]
        public void PropPrefabsArePropKind()
        {
            var crate = _objects.Create("Crate").AddComponent<NetworkObject>();

            var kinds = WeaponPrefabKinds.Of(_catalog, new[] { crate });

            Assert.That(kinds.ByPrefab[crate.gameObject], Is.EqualTo(WeaponKind.Prop));
        }

        [Test]
        public void OtherPrefabsHaveNoKind()
        {
            var character = _objects.Create("Character");

            var kinds = WeaponPrefabKinds.Of(_catalog, new NetworkObject[0]);

            Assert.That(kinds.ByPrefab.ContainsKey(character), Is.False);
        }
    }
}
