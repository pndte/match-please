using System;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon.Network
{
    public sealed class WeaponKindRegistrarTests
    {
        private LifetimeDefinition _lifetime;
        private FakeNetworkObject _networkObject;
        private FakeWeapon _weapon;
        private WeaponKindRegistry _registry;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _networkObject = new FakeNetworkObject();
            _weapon = new FakeWeapon();
            _registry = new WeaponKindRegistry();
            new WeaponKindRegistrar(_lifetime.Lifetime, _networkObject, _weapon, _registry, WeaponKind.Gun);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void WeaponNotSpawnedHasNoKind()
        {
            Assert.Throws<InvalidOperationException>(() => _registry.KindOf(_weapon));
        }

        [Test]
        public void SpawnedWeaponHasItsKind()
        {
            _networkObject.Spawn();

            Assert.That(_registry.KindOf(_weapon), Is.EqualTo(WeaponKind.Gun));
        }

        [Test]
        public void DespawnedWeaponLosesItsKind()
        {
            _networkObject.Spawn();

            _networkObject.Despawn();

            Assert.Throws<InvalidOperationException>(() => _registry.KindOf(_weapon));
        }

        [Test]
        public void RespawnedWeaponHasItsKindAgain()
        {
            _networkObject.Spawn();
            _networkObject.Despawn();

            _networkObject.Spawn();

            Assert.That(_registry.KindOf(_weapon), Is.EqualTo(WeaponKind.Gun));
        }
    }
}
