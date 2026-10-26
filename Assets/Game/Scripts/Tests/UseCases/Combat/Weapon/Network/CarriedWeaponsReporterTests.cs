using System;
using Bw.Entities.Extensions;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon.Network
{
    public sealed class CarriedWeaponsReporterTests
    {
        private LifetimeDefinition _lifetime;
        private WeaponSlot _fists;
        private WeaponSlot _gun;
        private RecordingInventory _inventory;
        private WeaponCollections _weapons;
        private CarriedWeaponsReporter _reporter;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _fists = new WeaponSlot();
            _gun = new WeaponSlot();
            _inventory = new RecordingInventory(_fists, _gun);
            _weapons = new WeaponCollections(_lifetime.Lifetime);
            _reporter = new CarriedWeaponsReporter(_inventory, _weapons);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void EmptyInventoryReportsNoWeapons()
        {
            Assert.That(_reporter.Current.Weapons, Is.Empty);
        }

        [Test]
        public void EveryCarriedWeaponIsReportedByItsNetworkIdAndSlotIndex()
        {
            var fists = Spawned(7);
            var gun = Spawned(12);
            _inventory.Give(_fists, fists);
            _inventory.Give(_gun, gun);

            var weapons = _reporter.Current.Weapons;

            Assert.That(weapons, Is.EquivalentTo(new[] { new CarriedWeapon(7, 0), new CarriedWeapon(12, 1) }));
        }

        [Test]
        public void ReportIsTakenAtTheMomentItIsRead()
        {
            var gun = Spawned(12);
            var before = _reporter.Current;

            _inventory.Give(_gun, gun);

            Assert.That(before.Weapons, Is.Empty);
            Assert.That(_reporter.Current.Weapons, Is.EqualTo(new[] { new CarriedWeapon(12, 1) }));
        }

        [Test]
        public void CarriedWeaponInASlotOutsideTheLayoutThrows()
        {
            var gun = Spawned(12);
            _inventory.Give(new WeaponSlot(), gun);

            Assert.Throws<InvalidOperationException>(() => _ = _reporter.Current);
        }

        private FakeWeapon Spawned(ulong id)
        {
            var weapon = new FakeWeapon();
            _weapons.ByIds.AddLifetimed(_lifetime.Lifetime, id, weapon);
            return weapon;
        }
    }
}
