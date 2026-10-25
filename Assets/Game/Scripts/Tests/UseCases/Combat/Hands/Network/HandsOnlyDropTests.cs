using Bw.Tests.Doubles;
using Bw.Tests.UseCases.Combat.Hands.Doubles;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Hands;
using Bw.UseCases.Combat.Hands.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Hands.Network
{
    public sealed class HandsOnlyDropTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private InventoryRig _inventories;
        private HandsRig _rig;
        private FakeWeapon _gun;
        private FakeWeapon _crate;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _inventories = new InventoryRig(_lifetime.Lifetime);
            _rig = new HandsRig(_lifetime.Lifetime, _inventories, _objects.Create("holder").AddComponent<HoldOval>());
            new HandsOnlyDrop(_lifetime.Lifetime, _rig.Character);
            _rig.Inventory.Give(_inventories.FistsSlot, _inventories.Weapon(WeaponKind.Fists));
            _gun = _inventories.Weapon(WeaponKind.Gun);
            _crate = _inventories.Weapon(WeaponKind.Prop);
            _rig.Hands.Take(_gun);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _inventories.Dispose();
            _objects.Dispose();
        }

        [Test]
        public void HandsLeavingAHandsOnlyItemDropIt()
        {
            _rig.Hands.Take(_crate);

            _rig.Step(DrawChoice.ToSlot(_inventories.FistsSlot));

            Assert.That(_rig.Inventory.Carried.ContainsKey(_crate), Is.False);
        }

        [Test]
        public void GuardTakingTheHandsDropsAHandsOnlyItem()
        {
            _rig.Hands.Take(_crate);

            _rig.Step(DrawChoice.Keep, guarded: true);

            Assert.That(_rig.Inventory.Carried.ContainsKey(_crate), Is.False);
        }

        [Test]
        public void HandsLeavingAWeaponKeepIt()
        {
            _rig.Step(DrawChoice.ToSlot(_inventories.FistsSlot));

            Assert.That(_rig.Inventory.Carried.ContainsKey(_gun), Is.True);
        }

        [Test]
        public void HandsStayingOnTheItemKeepIt()
        {
            _rig.Hands.Take(_crate);

            _rig.Step(DrawChoice.Keep);

            Assert.That(_rig.Inventory.Carried.ContainsKey(_crate), Is.True);
        }

        [Test]
        public void ItemThatLeftTheInventoryFirstIsNotDroppedAgain()
        {
            _rig.Hands.Take(_crate);
            _rig.Inventory.Drop(_crate);

            Assert.DoesNotThrow(() => _rig.Step(DrawChoice.Keep));
            Assert.That(_rig.Hands.TryFindInHands(out var inHands) && inHands == _gun, Is.True);
        }

        [Test]
        public void RevivedCharacterDropsAgain()
        {
            _rig.Mortal.Die();
            _rig.Mortal.Revive();
            _rig.Inventory.Give(_inventories.FistsSlot, _inventories.Weapon(WeaponKind.Fists));
            _rig.Step(DrawChoice.Keep);
            var crate = _inventories.Weapon(WeaponKind.Prop);
            _rig.Hands.Take(crate);

            _rig.Step(DrawChoice.ToSlot(_inventories.FistsSlot));

            Assert.That(_rig.Inventory.Carried.ContainsKey(crate), Is.False);
        }
    }
}
