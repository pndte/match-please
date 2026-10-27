using System;
using Bw.CoreAdapters.Weapons;
using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.Entities.Pool;
using Bw.Entities.Pool.GameObjects;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Spawning.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;
using Setup;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Bw.Tests.UseCases.Spawning.Network
{
    public sealed class NetworkPrefabHandlerFactoryTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private WeaponCatalog _catalog;
        private WeaponKindRegistry _kinds;
        private FakeWeapon _weapon;
        private NetworkObject _prefab;
        private NetworkObject _instance;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _catalog = ScriptableObject.CreateInstance<WeaponCatalog>();
            _kinds = new WeaponKindRegistry();
            _weapon = new FakeWeapon();
            _prefab = PooledObject("Crate");
            _instance = PooledObject("Crate(Clone)");
            var container = new DiContainer();
            container.Bind<IWeapon>().FromInstance(_weapon);
            container.InstantiateComponent<WeaponHolder>(_instance.gameObject);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
            Object.DestroyImmediate(_catalog);
        }

        [Test]
        public void ServerLetsGoOfAPropItSpawnedFromItsPool()
        {
            var handler = Factory(PeerType.Server).Create(_prefab.gameObject);

            Assert.DoesNotThrow(() => handler.Destroy(_instance), "the server's creators register a prop's kind; NGO hands the despawn to a handler that never instantiated it");
        }

        [Test]
        public void ClientInstanceOfAPropHasItsKind()
        {
            var handler = Factory(PeerType.Client).Create(_prefab.gameObject);

            handler.Instantiate(0, Vector3.zero, Quaternion.identity);

            Assert.That(_kinds.KindOf(_weapon), Is.EqualTo(WeaponKind.Prop));
        }

        [Test]
        public void ClientDestroyedPropLosesItsKind()
        {
            var handler = Factory(PeerType.Client).Create(_prefab.gameObject);
            var instance = handler.Instantiate(0, Vector3.zero, Quaternion.identity);

            handler.Destroy(instance);

            Assert.Throws<InvalidOperationException>(() => _kinds.KindOf(_weapon));
        }

        private NetworkPrefabHandlerFactory Factory(PeerType peer) =>
            new(_lifetime.Lifetime, new DiContainer(), new Pools(_instance), new RuntimeSettings(peer), WeaponPrefabKinds.Of(_catalog, new[] { _prefab }), _kinds);

        private NetworkObject PooledObject(string name)
        {
            var gameObject = _objects.Create(name);
            var networkObject = gameObject.AddComponent<NetworkObject>();
            gameObject.AddComponent<PooledNetworkObject>();
            return networkObject;
        }

        private sealed class Pools : IPrefabPools, IPool<NetworkObject>
        {
            private readonly NetworkObject _instance;

            public Pools(NetworkObject instance)
            {
                _instance = instance;
            }

            public IPool<T> For<T>(GameObject prefab) where T : class =>
                (IPool<T>)(object)this;

            public NetworkObject Resource(Lifetime lifetime) =>
                _instance;
        }
    }
}
