using System;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon
{
    public sealed class WeaponKindRegistryTests
    {
        private LifetimeDefinition _lifetime;
        private WeaponKindRegistry _registry;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _registry = new WeaponKindRegistry();
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [TestCase(WeaponKind.Fists)]
        [TestCase(WeaponKind.Gun)]
        [TestCase(WeaponKind.Prop)]
        public void RegisteredWeaponHasItsKind(WeaponKind kind)
        {
            var weapon = new FakeWeapon();

            _registry.AddLifetimed(_lifetime.Lifetime, weapon, kind);

            Assert.That(_registry.KindOf(weapon), Is.EqualTo(kind));
        }

        [Test]
        public void WeaponNeverRegisteredHasNoKind()
        {
            Assert.Throws<InvalidOperationException>(() => _registry.KindOf(new FakeWeapon()));
        }

        [Test]
        public void WeaponIsRegisteredOnceAtATime()
        {
            var weapon = new FakeWeapon();
            _registry.AddLifetimed(_lifetime.Lifetime, weapon, WeaponKind.Gun);

            Assert.Throws<InvalidOperationException>(() => _registry.AddLifetimed(_lifetime.Lifetime, weapon, WeaponKind.Prop));
            Assert.That(_registry.KindOf(weapon), Is.EqualTo(WeaponKind.Gun));
        }

        [Test]
        public void KindIsForgottenWithItsRegistration()
        {
            var registration = new LifetimeDefinition();
            var weapon = new FakeWeapon();
            _registry.AddLifetimed(registration.Lifetime, weapon, WeaponKind.Gun);

            registration.Terminate();

            Assert.Throws<InvalidOperationException>(() => _registry.KindOf(weapon));
        }

        [Test]
        public void WeaponIsRegisteredAgainAfterItsLastRegistrationEnded()
        {
            var registration = new LifetimeDefinition();
            var weapon = new FakeWeapon();
            _registry.AddLifetimed(registration.Lifetime, weapon, WeaponKind.Gun);
            registration.Terminate();

            _registry.AddLifetimed(_lifetime.Lifetime, weapon, WeaponKind.Prop);

            Assert.That(_registry.KindOf(weapon), Is.EqualTo(WeaponKind.Prop));
        }

        [Test]
        public void WeaponsAreRegisteredApart()
        {
            var gun = new FakeWeapon();
            var fists = new FakeWeapon();

            _registry.AddLifetimed(_lifetime.Lifetime, gun, WeaponKind.Gun);
            _registry.AddLifetimed(_lifetime.Lifetime, fists, WeaponKind.Fists);

            Assert.That(_registry.KindOf(gun), Is.EqualTo(WeaponKind.Gun));
            Assert.That(_registry.KindOf(fists), Is.EqualTo(WeaponKind.Fists));
        }
    }
}
