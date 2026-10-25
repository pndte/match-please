using Bw.Tests.UseCases.Combat.Hands.Doubles;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Hands;
using Bw.UseCases.Combat.Weapon;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Hands
{
    public sealed class HandsSimulatorTests
    {
        private const int None = -1;
        private const int Fists = 0;
        private const int Gun = 1;
        private const int Prop = 2;

        private LifetimeDefinition _lifetime;
        private InventoryRig _inventories;
        private CharacterInventory _inventory;
        private HandsSimulator _simulator;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _inventories = new InventoryRig(_lifetime.Lifetime);
            _inventory = _inventories.InventoryOf(new FakeMortal());
            _simulator = new HandsSimulator(_inventory);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _inventories.Dispose();
        }

        [Test]
        public void EmptyHandsSettleOnTheFirstSlotHoldingAWeapon()
        {
            Carry(Gun);

            var next = _simulator.Step(HandsState.Empty, Input(DrawChoice.Keep));

            Assert.That(next, Is.EqualTo(State(Gun, None)));
        }

        [Test]
        public void EmptyHandsSettleOnTheFistsBeforeTheGun()
        {
            Carry(Fists, Gun);

            var next = _simulator.Step(HandsState.Empty, Input(DrawChoice.Keep));

            Assert.That(next, Is.EqualTo(State(Fists, None)));
        }

        [Test]
        public void HandsStayEmptyWhileNothingIsCarried()
        {
            var next = _simulator.Step(HandsState.Empty, Input(DrawChoice.Keep));

            Assert.That(next, Is.EqualTo(HandsState.Empty));
        }

        [Test]
        public void HandsWhoseItemLeftGoBackToTheRememberedSlot()
        {
            Carry(Fists, Gun);

            var next = _simulator.Step(State(Prop, Gun), Input(DrawChoice.Keep));

            Assert.That(next, Is.EqualTo(State(Gun, None)));
        }

        [Test]
        public void HandsWhoseItemLeftWithNothingRememberedSettleOnTheFirstHeldSlot()
        {
            Carry(Fists);

            var next = _simulator.Step(State(Gun, None), Input(DrawChoice.Keep));

            Assert.That(next.InHands, Is.EqualTo(Slot(Fists)));
        }

        [Test]
        public void SlotChoiceDrawsTheSlotAndRemembersTheOneLeft()
        {
            Carry(Fists, Gun);

            var next = _simulator.Step(State(Gun, Fists), Input(ToSlot(Fists)));

            Assert.That(next, Is.EqualTo(State(Fists, Gun)));
        }

        [TestCase(Gun)]
        [TestCase(Prop)]
        public void ChoiceOfTheSlotInHandsOrOfAnEmptySlotChangesNothing(int chosen)
        {
            Carry(Fists, Gun);

            var next = _simulator.Step(State(Gun, Fists), Input(ToSlot(chosen)));

            Assert.That(next, Is.EqualTo(State(Gun, Fists)));
        }

        [Test]
        public void KeepChangesNothing()
        {
            Carry(Fists, Gun);

            var next = _simulator.Step(State(Gun, Fists), Input(DrawChoice.Keep));

            Assert.That(next, Is.EqualTo(State(Gun, Fists)));
        }

        [Test]
        public void LastGoesBackToTheRememberedSlot()
        {
            Carry(Fists, Gun);

            var next = _simulator.Step(State(Fists, Gun), Input(DrawChoice.Last));

            Assert.That(next, Is.EqualTo(State(Gun, Fists)));
        }

        [Test]
        public void LastWithNothingRememberedChangesNothing()
        {
            Carry(Fists, Gun);

            var next = _simulator.Step(State(Fists, None), Input(DrawChoice.Last));

            Assert.That(next, Is.EqualTo(State(Fists, None)));
        }

        [Test]
        public void ScrollForwardSkipsAnEmptySlotAndWrapsAround()
        {
            Carry(Fists, Gun);

            var next = _simulator.Step(State(Gun, Fists), Input(DrawChoice.Scrolled(1)));

            Assert.That(next, Is.EqualTo(State(Fists, Gun)));
        }

        [Test]
        public void ScrollBackSkipsAnEmptySlotAndWrapsAround()
        {
            Carry(Fists, Gun);

            var next = _simulator.Step(State(Fists, Gun), Input(DrawChoice.Scrolled(-1)));

            Assert.That(next, Is.EqualTo(State(Gun, Fists)));
        }

        [Test]
        public void ScrollStopsAtTheNextSlotHoldingAWeapon()
        {
            Carry(Fists, Gun, Prop);

            var next = _simulator.Step(State(Fists, None), Input(DrawChoice.Scrolled(1)));

            Assert.That(next, Is.EqualTo(State(Gun, Fists)));
        }

        [Test]
        public void ScrollWithASingleWeaponChangesNothing()
        {
            Carry(Fists);

            var next = _simulator.Step(State(Fists, None), Input(DrawChoice.Scrolled(1)));

            Assert.That(next, Is.EqualTo(State(Fists, None)));
        }

        [Test]
        public void ChoiceAwayFromAHandsOnlyItemRemembersTheWeaponBeforeIt()
        {
            Carry(Fists, Gun, Prop);

            var next = _simulator.Step(State(Prop, Gun), Input(ToSlot(Fists)));

            Assert.That(next, Is.EqualTo(State(Fists, Gun)));
        }

        [Test]
        public void RememberedSlotThatIsChosenIsForgotten()
        {
            Carry(Fists, Gun, Prop);

            var next = _simulator.Step(State(Prop, Fists), Input(ToSlot(Fists)));

            Assert.That(next, Is.EqualTo(State(Fists, None)));
        }

        [Test]
        public void RaisedGuardTakesTheHandsForTheFistsAndRemembersTheWeapon()
        {
            Carry(Fists, Gun);

            var next = _simulator.Step(State(Gun, Fists), Input(DrawChoice.Keep, guarded: true));

            Assert.That(next, Is.EqualTo(State(Fists, Gun, takenByGuard: true)));
        }

        [Test]
        public void GuardOverTheFistsTakesNothing()
        {
            Carry(Fists, Gun);

            var next = _simulator.Step(State(Fists, Gun), Input(DrawChoice.Keep, guarded: true));

            Assert.That(next, Is.EqualTo(State(Fists, Gun)));
        }

        [Test]
        public void GuardKeepsTheHandsWhileItStaysRaised()
        {
            Carry(Fists, Gun);

            var next = _simulator.Step(State(Fists, Gun, takenByGuard: true), Input(DrawChoice.Keep, guarded: true));

            Assert.That(next, Is.EqualTo(State(Fists, Gun, takenByGuard: true)));
        }

        [Test]
        public void LoweredGuardGivesTheRememberedWeaponBack()
        {
            Carry(Fists, Gun);

            var next = _simulator.Step(State(Fists, Gun, takenByGuard: true), Input(DrawChoice.Keep));

            Assert.That(next, Is.EqualTo(State(Gun, Fists)));
        }

        [Test]
        public void LoweredGuardWithNothingRememberedKeepsTheFists()
        {
            Carry(Fists);

            var next = _simulator.Step(State(Fists, None, takenByGuard: true), Input(DrawChoice.Keep));

            Assert.That(next, Is.EqualTo(State(Fists, None)));
        }

        [Test]
        public void ChoiceDuringTheGuardIsRememberedForItsEnd()
        {
            Carry(Fists, Gun, Prop);

            var guarded = _simulator.Step(State(Fists, Gun, takenByGuard: true), Input(ToSlot(Prop), guarded: true));
            var lowered = _simulator.Step(guarded, Input(DrawChoice.Keep));

            Assert.That(guarded, Is.EqualTo(State(Fists, Prop, takenByGuard: true)));
            Assert.That(lowered, Is.EqualTo(State(Prop, Fists)));
        }

        [Test]
        public void GuardRaisedTogetherWithAChoiceStillTakesTheHandsForTheFists()
        {
            Carry(Fists, Gun, Prop);

            var next = _simulator.Step(State(Gun, Fists), Input(ToSlot(Prop), guarded: true));

            Assert.That(next.InHands, Is.EqualTo(Slot(Fists)));
            Assert.That(next.TakenByGuard, Is.True);
        }

        [Test]
        public void GuardWithoutFistsTakesNothing()
        {
            Carry(Gun);

            var next = _simulator.Step(State(Gun, None), Input(DrawChoice.Keep, guarded: true));

            Assert.That(next, Is.EqualTo(State(Gun, None)));
        }

        [Test]
        public void GuardOverAHandsOnlyItemRemembersTheWeaponBeforeIt()
        {
            Carry(Fists, Gun, Prop);

            var next = _simulator.Step(State(Prop, Gun), Input(DrawChoice.Keep, guarded: true));

            Assert.That(next, Is.EqualTo(State(Fists, Gun, takenByGuard: true)));
        }

        [Test]
        public void ItemThatLeftIsSettledBeforeTheGuardTakesTheHands()
        {
            Carry(Fists, Gun);

            var next = _simulator.Step(State(Prop, Gun), Input(DrawChoice.Keep, guarded: true));

            Assert.That(next, Is.EqualTo(State(Fists, Gun, takenByGuard: true)));
        }

        [Test]
        public void GuardOverFistsCoveredByAWeaponInTheirSlotTakesNothing()
        {
            var shared = new WeaponSlot { Kinds = new[] { WeaponKind.Fists, WeaponKind.Gun } };
            _inventories.Config.Slots = new[] { shared };
            var inventory = _inventories.InventoryOf(new FakeMortal());
            inventory.Give(shared, _inventories.Weapon(WeaponKind.Fists));
            inventory.Give(shared, _inventories.Weapon(WeaponKind.Gun));
            var simulator = new HandsSimulator(inventory);
            var state = new HandsState(HandsSlot.Of(shared), HandsSlot.Empty, false);

            var next = simulator.Step(state, Input(DrawChoice.Keep, guarded: true));

            Assert.That(next, Is.EqualTo(state));
        }

        [Test]
        public void TakenSlotComesIntoTheHandsAndRemembersTheOneLeft()
        {
            Carry(Fists, Gun, Prop);

            var next = _simulator.Taken(State(Gun, Fists), _inventories.PropSlot);

            Assert.That(next, Is.EqualTo(State(Prop, Gun)));
        }

        [Test]
        public void TakenSlotWaitsBehindTheGuard()
        {
            Carry(Fists, Gun);

            var next = _simulator.Taken(State(Fists, None, takenByGuard: true), _inventories.GunSlot);

            Assert.That(next, Is.EqualTo(State(Fists, Gun, takenByGuard: true)));
        }

        [Test]
        public void TakenAwayFromAHandsOnlyItemRemembersTheWeaponBeforeIt()
        {
            Carry(Fists, Gun, Prop);

            var next = _simulator.Taken(State(Prop, Fists), _inventories.GunSlot);

            Assert.That(next, Is.EqualTo(State(Gun, Fists)));
        }

        private void Carry(params int[] slots)
        {
            foreach (var slot in slots)
                _inventory.Give(_inventories.Config.Slots[slot], _inventories.Weapon(KindOf(slot)));
        }

        private HandsSlot Slot(int index) =>
            index == None ? HandsSlot.Empty : HandsSlot.Of(_inventories.Config.Slots[index]);

        private HandsState State(int inHands, int previous, bool takenByGuard = false) =>
            new(Slot(inHands), Slot(previous), takenByGuard);

        private DrawChoice ToSlot(int index) =>
            DrawChoice.ToSlot(_inventories.Config.Slots[index]);

        private static HandsInput Input(DrawChoice choice, bool guarded = false) =>
            new(new WeaponInput(0f, false, false, guarded), choice);

        private static WeaponKind KindOf(int slot) =>
            slot switch
            {
                Fists => WeaponKind.Fists,
                Gun => WeaponKind.Gun,
                _ => WeaponKind.Prop
            };
    }
}
