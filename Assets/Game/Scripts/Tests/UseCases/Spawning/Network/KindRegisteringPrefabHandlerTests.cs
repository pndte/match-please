using System;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Spawning.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.Tests.UseCases.Spawning.Network
{
    public sealed class KindRegisteringPrefabHandlerTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private FakeWeapon _weapon;
        private WeaponKindRegistry _kinds;
        private Instances _instances;
        private KindRegisteringPrefabHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _weapon = new FakeWeapon();
            _kinds = new WeaponKindRegistry();
            _instances = new Instances(WeaponObject(_weapon));
            _handler = new KindRegisteringPrefabHandler(_lifetime.Lifetime, _instances, WeaponKind.Gun, _kinds);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void CreatedInstanceHasItsKind()
        {
            _handler.Instantiate(0, Vector3.zero, Quaternion.identity);

            Assert.That(_kinds.KindOf(_weapon), Is.EqualTo(WeaponKind.Gun));
        }

        [Test]
        public void DestroyedInstanceLosesItsKindAndIsDestroyed()
        {
            var instance = _handler.Instantiate(0, Vector3.zero, Quaternion.identity);

            _handler.Destroy(instance);

            Assert.Throws<InvalidOperationException>(() => _kinds.KindOf(_weapon));
            Assert.That(_instances.Destroyed, Is.SameAs(instance));
        }

        [Test]
        public void InstanceWithoutWeaponThrows()
        {
            var characters = new Instances(_objects.Create("Character").AddComponent<NetworkObject>());
            var handler = new KindRegisteringPrefabHandler(_lifetime.Lifetime, characters, WeaponKind.Gun, _kinds);

            Assert.Throws<InvalidOperationException>(() => handler.Instantiate(0, Vector3.zero, Quaternion.identity));
        }

        private NetworkObject WeaponObject(IWeapon weapon)
        {
            var container = new DiContainer();
            container.Bind<IWeapon>().FromInstance(weapon);
            var weaponObject = _objects.Create("Gun");
            var networkObject = weaponObject.AddComponent<NetworkObject>();
            container.InstantiateComponent<WeaponHolder>(weaponObject);
            return networkObject;
        }

        private sealed class Instances : INetworkPrefabInstanceHandler
        {
            public NetworkObject Destroyed { get; private set; }

            private readonly NetworkObject _instance;

            public Instances(NetworkObject instance)
            {
                _instance = instance;
            }

            public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation) =>
                _instance;

            public void Destroy(NetworkObject networkObject) =>
                Destroyed = networkObject;
        }
    }
}
