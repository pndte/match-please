using Bw.Tests.UseCases.Combat.Hands.Doubles;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Hands.Extensions;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Hands
{
    public sealed class HandsInventoryExtensionsTests
    {
        private LifetimeDefinition _lifetime;
        private InventoryRig _inventories;
        private FakeMortal _mortal;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _inventories = new InventoryRig(_lifetime.Lifetime);
            _mortal = new FakeMortal();
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _inventories.Dispose();
        }

        [Test]
        public void EmptyHandsHaveNoTop()
        {
            var inventory = _inventories.InventoryOf(_mortal);
            var fists = _inventories.Weapon(WeaponKind.Fists);
            inventory.Give(_inventories.FistsSlot, fists);

            Assert.That(inventory.TryFindTop(HandsSlot.Empty, out var top), Is.False);
            Assert.That(top, Is.Null);
        }

        [Test]
        public void SlotWithAWeaponHasItOnTop()
        {
            var inventory = _inventories.InventoryOf(_mortal);
            var gun = _inventories.Weapon(WeaponKind.Gun);
            inventory.Give(_inventories.GunSlot, gun);

            Assert.That(inventory.TryFindTop(HandsSlot.Of(_inventories.GunSlot), out var top), Is.True);
            Assert.That(top, Is.SameAs(gun));
        }

        [Test]
        public void EmptySlotHasNoTop()
        {
            var inventory = _inventories.InventoryOf(_mortal);

            Assert.That(inventory.TryFindTop(HandsSlot.Of(_inventories.PropSlot), out _), Is.False);
        }

        [Test]
        public void DroppableWeaponLiesOverTheFistsInASharedSlot()
        {
            var shared = new WeaponSlot { Kinds = new[] { WeaponKind.Fists, WeaponKind.Gun } };
            _inventories.Config.Slots = new[] { shared };
            var inventory = _inventories.InventoryOf(_mortal);
            var fists = _inventories.Weapon(WeaponKind.Fists);
            var gun = _inventories.Weapon(WeaponKind.Gun);
            inventory.Give(shared, fists);
            inventory.Give(shared, gun);

            Assert.That(inventory.TryFindTop(HandsSlot.Of(shared), out var top), Is.True);
            Assert.That(top, Is.SameAs(gun));
        }
    }
}
