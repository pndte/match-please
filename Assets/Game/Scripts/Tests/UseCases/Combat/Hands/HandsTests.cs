using System;
using System.Collections.Generic;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.Combat.Hands.Doubles;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Hands;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.Hands
{
    public sealed class HandsTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private InventoryRig _inventories;
        private HoldOval _oval;
        private HandsRig _rig;
        private FakeWeapon _fists;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _inventories = new InventoryRig(_lifetime.Lifetime);
            _oval = _objects.Create("holder", new Vector2(2f, 3f)).AddComponent<HoldOval>();
            _rig = new HandsRig(_lifetime.Lifetime, _inventories, _oval);
            _fists = _inventories.Weapon(WeaponKind.Fists);
            _rig.Inventory.Give(_inventories.FistsSlot, _fists);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _inventories.Dispose();
            _objects.Dispose();
        }

        [Test]
        public void NewHandsHoldNothingUntilTheirFirstStep()
        {
            Assert.That(_rig.Hands.State.Value, Is.EqualTo(HandsState.Empty));
            Assert.That(_rig.Hands.Previous, Is.EqualTo(HandsState.Empty));
            Assert.That(_rig.Hands.TryFindInHands(out _), Is.False);
            Assert.That(_rig.Hands.Busy, Is.False);
        }

        [Test]
        public void FirstStepSettlesTheHandsOnTheFists()
        {
            _rig.Step(DrawChoice.Keep);

            Assert.That(_rig.Hands.TryFindInHands(out var inHands), Is.True);
            Assert.That(inHands, Is.SameAs(_fists));
        }

        [Test]
        public void StepRemembersThePreviousStateAndAnnouncesTheNewOne()
        {
            var stepped = new List<HandsState>();
            _rig.Hands.Stepped.Advise(_lifetime.Lifetime, stepped.Add);

            _rig.Step(DrawChoice.Keep);

            Assert.That(_rig.Hands.Previous, Is.EqualTo(HandsState.Empty));
            Assert.That(stepped, Is.EqualTo(new[] { _rig.Hands.State.Value }));
        }

        [Test]
        public void StepStepsOnlyTheWeaponInHandsWithTheWeaponInput()
        {
            var gun = _inventories.Weapon(WeaponKind.Gun);
            _rig.Inventory.Give(_inventories.GunSlot, gun);
            _rig.Step(DrawChoice.Keep);

            _rig.Step(DrawChoice.Keep, pulled: true);

            Assert.That(_fists.Steps, Has.Count.EqualTo(2));
            Assert.That(_fists.Steps[1].Pulled, Is.True);
            Assert.That(gun.Steps, Is.Empty);
        }

        [Test]
        public void ChosenWeaponIsSteppedInTheStepThatDrawsIt()
        {
            var gun = _inventories.Weapon(WeaponKind.Gun);
            _rig.Inventory.Give(_inventories.GunSlot, gun);
            _rig.Step(DrawChoice.Keep);

            _rig.Step(DrawChoice.ToSlot(_inventories.GunSlot));

            Assert.That(gun.Steps, Has.Count.EqualTo(1));
            Assert.That(_fists.Steps, Has.Count.EqualTo(1));
        }

        [Test]
        public void EmptyHandsStepNoWeapon()
        {
            var lifetime = new LifetimeDefinition(_lifetime.Lifetime);
            var empty = new HandsRig(lifetime.Lifetime, _inventories, _oval);

            empty.Step(DrawChoice.Keep);

            Assert.That(empty.Hands.State.Value, Is.EqualTo(HandsState.Empty));
        }

        [Test]
        public void HandsAreBusyWhileTheirWeaponCannotBeStowed()
        {
            _rig.Step(DrawChoice.Keep);

            _fists.Stowable = false;

            Assert.That(_rig.Hands.Busy, Is.True);
        }

        [Test]
        public void TakeBringsTheItemIntoTheHandsAtOnce()
        {
            _rig.Step(DrawChoice.Keep);
            var gun = _inventories.Weapon(WeaponKind.Gun);

            _rig.Hands.Take(gun);

            Assert.That(_rig.Inventory.Carried[gun], Is.SameAs(_inventories.GunSlot));
            Assert.That(_rig.Hands.State.Value, Is.EqualTo(new HandsState(HandsSlot.Of(_inventories.GunSlot), HandsSlot.Of(_inventories.FistsSlot), false)));
            Assert.That(_rig.Hands.TryFindInHands(out var inHands) && inHands == gun, Is.True);
        }

        [Test]
        public void TakeIntoAFullSlotSwapsTheWeaponThere()
        {
            var gun = _inventories.Weapon(WeaponKind.Gun);
            var pistol = _inventories.Weapon(WeaponKind.Gun);
            _rig.Step(DrawChoice.Keep);
            _rig.Hands.Take(gun);

            _rig.Hands.Take(pistol);

            Assert.That(_rig.Inventory.Carried.ContainsKey(gun), Is.False);
            Assert.That(_rig.Hands.TryFindInHands(out var inHands) && inHands == pistol, Is.True);
        }

        [Test]
        public void TakeOfACarriedItemThrows()
        {
            var gun = _inventories.Weapon(WeaponKind.Gun);
            _rig.Hands.Take(gun);

            Assert.Throws<InvalidOperationException>(() => _rig.Hands.Take(gun));
        }

        [Test]
        public void TakeOfAnUndroppableItemThrows()
        {
            Assert.Throws<InvalidOperationException>(() => _rig.Hands.Take(_inventories.Weapon(WeaponKind.Fists)));
        }

        [Test]
        public void BusyHandsAreFreeForNothingAndTakeChangesNothing()
        {
            _rig.Step(DrawChoice.Keep);
            _fists.Stowable = false;
            var gun = _inventories.Weapon(WeaponKind.Gun);

            Assert.That(_rig.Hands.FreeFor(gun), Is.False);
            Assert.Throws<InvalidOperationException>(() => _rig.Hands.Take(gun));
            Assert.That(_rig.Inventory.Carried.ContainsKey(gun), Is.False);
        }

        [Test]
        public void GuardLeavesTheHandsFreeForAGunButNotForAHandsOnlyItem()
        {
            _rig.Inventory.Give(_inventories.GunSlot, _inventories.Weapon(WeaponKind.Gun));
            _rig.Step(DrawChoice.ToSlot(_inventories.GunSlot));
            _rig.Step(DrawChoice.Keep, guarded: true);

            Assert.That(_rig.Hands.FreeFor(_inventories.Weapon(WeaponKind.Gun)), Is.True);
            Assert.That(_rig.Hands.FreeFor(_inventories.Weapon(WeaponKind.Prop)), Is.False);
        }

        [Test]
        public void GunTakenDuringTheGuardWaitsBehindTheFists()
        {
            var gun = _inventories.Weapon(WeaponKind.Gun);
            var pistol = _inventories.Weapon(WeaponKind.Gun);
            _rig.Hands.Take(gun);
            _rig.Step(DrawChoice.Keep, guarded: true);

            _rig.Hands.Take(pistol);
            var guarded = _rig.Hands.TryFindInHands(out var duringTheGuard) && duringTheGuard == _fists;
            _rig.Step(DrawChoice.Keep);

            Assert.That(guarded, Is.True);
            Assert.That(_rig.Inventory.Carried.ContainsKey(gun), Is.False);
            Assert.That(_rig.Hands.TryFindInHands(out var afterTheGuard) && afterTheGuard == pistol, Is.True);
        }

        [Test]
        public void ApplySetsTheStateWithoutAStep()
        {
            var stepped = 0;
            _rig.Hands.Stepped.Advise(_lifetime.Lifetime, _ => stepped++);
            var state = new HandsState(HandsSlot.Of(_inventories.FistsSlot), HandsSlot.Empty, true);

            _rig.Hands.Apply(state);

            Assert.That(_rig.Hands.State.Value, Is.EqualTo(state));
            Assert.That(stepped, Is.Zero);
            Assert.That(_fists.Steps, Is.Empty);
        }

        [Test]
        public void PoseIsTheHoldOvalAtTheHoldersBody()
        {
            _rig.MoveTo(new Vector2(5f, -1f));

            var pose = _rig.Hands.Pose;

            Assert.That(pose.Position, Is.EqualTo(new Vector2(5f, -1f)));
            Assert.That(pose.Hold.center, Is.EqualTo(new Vector2(5f, -1f) + _oval.Center));
        }

        [Test]
        public void PoseIgnoresWhereTheHolderIsDrawn()
        {
            _rig.MoveTo(new Vector2(5f, -1f));

            _oval.transform.position = new Vector3(40f, 7f, 0f);

            Assert.That(_rig.Hands.Pose.Position, Is.EqualTo(new Vector2(5f, -1f)));
        }

        [Test]
        public void GripIsThePoseOfTheHandsGrip()
        {
            Assert.That(_rig.Hands.Grip, Is.EqualTo(_rig.Grip.Pose));
        }
    }
}
