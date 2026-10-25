using System.Collections.Generic;
using Bw.Entities.Simulation;
using Bw.Tests.UseCases.Combat.Hands.Doubles;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Hands;
using Bw.UseCases.Combat.Hands.Network;
using Bw.UseCases.Combat.Weapon;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Hands.Network
{
    public sealed class BusyHandsGuardTests
    {
        private LifetimeDefinition _lifetime;
        private InventoryRig _inventories;
        private CharacterInventory _inventory;
        private RecordingSimulator _inner;
        private BusyHandsGuard _guard;
        private FakeWeapon _fists;
        private HandsState _fistsInHands;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _inventories = new InventoryRig(_lifetime.Lifetime);
            _inventory = _inventories.InventoryOf(new FakeMortal());
            _inner = new RecordingSimulator();
            _guard = new BusyHandsGuard(_inner, _inventory);
            _fists = _inventories.Weapon(WeaponKind.Fists);
            _inventory.Give(_inventories.FistsSlot, _fists);
            _inventory.Give(_inventories.GunSlot, _inventories.Weapon(WeaponKind.Gun));
            _fistsInHands = new HandsState(HandsSlot.Of(_inventories.FistsSlot), HandsSlot.Empty, false);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _inventories.Dispose();
        }

        [Test]
        public void FreeHandsPassTheChoiceOn()
        {
            var choice = DrawChoice.ToSlot(_inventories.GunSlot);

            _guard.Step(_fistsInHands, Input(choice));

            Assert.That(_inner.Inputs[0].Choice, Is.EqualTo(choice));
        }

        [Test]
        public void BusyHandsKeepWhatTheyHoldButPassTheWeaponInputOn()
        {
            _fists.Stowable = false;

            _guard.Step(_fistsInHands, Input(DrawChoice.ToSlot(_inventories.GunSlot)));

            Assert.That(_inner.Inputs[0].Choice, Is.EqualTo(DrawChoice.Keep));
            Assert.That(_inner.Inputs[0].Weapon.Pulled, Is.True);
            Assert.That(_inner.Inputs[0].Weapon.Guarded, Is.True);
            Assert.That(_inner.Inputs[0].Weapon.Aim, Is.EqualTo(33f));
        }

        [Test]
        public void EmptyHandsAreNeverBusy()
        {
            var choice = DrawChoice.Scrolled(1);

            _guard.Step(HandsState.Empty, Input(choice));

            Assert.That(_inner.Inputs[0].Choice, Is.EqualTo(choice));
        }

        [Test]
        public void StepIsTheInnerSimulatorsStep()
        {
            var next = _guard.Step(_fistsInHands, Input(DrawChoice.Keep));

            Assert.That(next, Is.EqualTo(_inner.Result));
            Assert.That(_inner.States, Is.EqualTo(new[] { _fistsInHands }));
        }

        private static HandsInput Input(DrawChoice choice) =>
            new(new WeaponInput(33f, true, false, true), choice);

        private sealed class RecordingSimulator : ISimulator<HandsInput, HandsState>
        {
            public List<HandsInput> Inputs { get; } = new();
            public List<HandsState> States { get; } = new();
            public HandsState Result { get; } = new(HandsSlot.Empty, HandsSlot.Empty, true);

            public HandsState Step(HandsState state, HandsInput input)
            {
                States.Add(state);
                Inputs.Add(input);
                return Result;
            }
        }
    }
}
