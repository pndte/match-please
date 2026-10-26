using System;
using System.Collections.Generic;
using Bw.Entities.Extensions;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.Combat.Hands.Doubles;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Hands;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Hands
{
    public sealed class DrawnWeaponsTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private InventoryRig _inventories;
        private HandsRig _rig;
        private FakeWeapon _fists;
        private FakeWeapon _gun;
        private List<string> _changes;
        private int _mostDrawn;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _inventories = new InventoryRig(_lifetime.Lifetime);
            _rig = new HandsRig(_lifetime.Lifetime, _inventories, _objects.Create("holder").AddComponent<HoldOval>());
            _fists = _inventories.Weapon(WeaponKind.Fists);
            _gun = _inventories.Weapon(WeaponKind.Gun);
            _changes = new List<string>();
            _rig.Draw.Drawn.Change.Advise(_lifetime.Lifetime, change =>
            {
                _changes.Add($"{change.Kind} {NameOf(change.Kind == AddUpdateRemove.Remove ? change.OldValue : change.NewValue)}");
                _mostDrawn = Math.Max(_mostDrawn, _rig.Draw.Drawn.Count);
            });
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _inventories.Dispose();
            _objects.Dispose();
        }

        [Test]
        public void WeaponGivenToTheInventoryIsNotDrawnByItself()
        {
            _rig.Inventory.Give(_inventories.FistsSlot, _fists);

            Assert.That(_rig.Draw.Drawn, Is.Empty);
        }

        [Test]
        public void WeaponInHandsIsDrawn()
        {
            _rig.Inventory.Give(_inventories.FistsSlot, _fists);

            _rig.Step(DrawChoice.Keep);

            Assert.That(_rig.Draw.Drawn, Is.EqualTo(new[] { _fists }));
        }

        [Test]
        public void SwitchStowsTheOldWeaponBeforeItDrawsTheNewOne()
        {
            _rig.Inventory.Give(_inventories.FistsSlot, _fists);
            _rig.Inventory.Give(_inventories.GunSlot, _gun);
            _rig.Step(DrawChoice.Keep);
            _changes.Clear();

            _rig.Step(DrawChoice.ToSlot(_inventories.GunSlot));

            Assert.That(_changes, Is.EqualTo(new[] { "Remove fists", "Add gun" }));
            Assert.That(_mostDrawn, Is.EqualTo(1));
        }

        [Test]
        public void TakenItemIsDrawnInsteadOfTheWeaponItReplacesInTheHands()
        {
            _rig.Inventory.Give(_inventories.FistsSlot, _fists);
            _rig.Step(DrawChoice.Keep);
            _changes.Clear();

            _rig.Hands.Take(_gun);

            Assert.That(_changes, Is.EqualTo(new[] { "Remove fists", "Add gun" }));
            Assert.That(_mostDrawn, Is.EqualTo(1));
        }

        [Test]
        public void DroppedWeaponIsStowedAtOnce()
        {
            _rig.Inventory.Give(_inventories.FistsSlot, _fists);
            _rig.Hands.Take(_gun);

            _rig.Inventory.Drop(_gun);

            Assert.That(_rig.Draw.Drawn, Is.Empty);
        }

        [Test]
        public void DrawEndsWhenTheWeaponIsStowed()
        {
            var draws = new List<Lifetime>();
            _rig.Draw.Drawn.View(_lifetime.Lifetime, (drawnLifetime, _) => draws.Add(drawnLifetime));
            _rig.Inventory.Give(_inventories.FistsSlot, _fists);
            _rig.Inventory.Give(_inventories.GunSlot, _gun);
            _rig.Step(DrawChoice.Keep);

            _rig.Step(DrawChoice.ToSlot(_inventories.GunSlot));

            Assert.That(draws, Has.Count.EqualTo(2));
            Assert.That(draws[0].IsAlive, Is.False);
            Assert.That(draws[1].IsAlive, Is.True);
        }

        [Test]
        public void DeathStowsEverything()
        {
            _rig.Inventory.Give(_inventories.FistsSlot, _fists);
            _rig.Step(DrawChoice.Keep);

            _rig.Mortal.Die();

            Assert.That(_rig.Draw.Drawn, Is.Empty);
        }

        [Test]
        public void WeaponGivenOverTheFistsInTheirSlotIsDrawnAndDroppingItDrawsTheFistsAgain()
        {
            var shared = new WeaponSlot { Kinds = new[] { WeaponKind.Fists, WeaponKind.Gun } };
            _inventories.Config.Slots = new[] { shared };
            _rig.Inventory.Give(shared, _fists);
            _rig.Step(DrawChoice.Keep);

            _rig.Inventory.Give(shared, _gun);
            var covered = new List<IWeapon>(_rig.Draw.Drawn);
            _rig.Inventory.Drop(_gun);

            Assert.That(covered, Is.EqualTo(new[] { _gun }));
            Assert.That(_rig.Draw.Drawn, Is.EqualTo(new[] { _fists }));
            Assert.That(_mostDrawn, Is.EqualTo(1));
        }

        private string NameOf(IWeapon weapon) =>
            weapon == _fists ? "fists" : weapon == _gun ? "gun" : "other";
    }
}
