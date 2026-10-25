using System;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Hands.Network;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Combat.Weapon;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bw.Tests.UseCases.Combat.Hands.Network
{
    public sealed class HandsSlotsFormTests
    {
        private WeaponInventoryConfig _config;
        private HandsSlotsForm _form;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<WeaponInventoryConfig>();
            _form = new HandsSlotsForm(_config);
        }

        [TearDown]
        public void TearDown() =>
            Object.DestroyImmediate(_config);

        [Test]
        public void SlotsGoToTheWireAsTheirIndicesInTheLayout()
        {
            var wire = _form.WireOf(new HandsState(Slot(2), Slot(0), true));

            Assert.That(wire.InHands, Is.EqualTo(2));
            Assert.That(wire.Previous, Is.EqualTo(0));
            Assert.That(wire.TakenByGuard, Is.True);
        }

        [Test]
        public void EmptyHandsGoToTheWireAsTheEmptyIndex()
        {
            var wire = _form.WireOf(HandsState.Empty);

            Assert.That(wire.InHands, Is.EqualTo(HandsSlots.Empty));
            Assert.That(wire.Previous, Is.EqualTo(HandsSlots.Empty));
            Assert.That(wire.TakenByGuard, Is.False);
        }

        [TestCase(0, 1, false)]
        [TestCase(1, 0, true)]
        [TestCase(2, -1, false)]
        [TestCase(-1, -1, true)]
        public void StateComesBackFromTheWireUnchanged(int inHands, int previous, bool takenByGuard)
        {
            var state = new HandsState(Slot(inHands), Slot(previous), takenByGuard);

            Assert.That(_form.ValueOf(_form.WireOf(state)), Is.EqualTo(state));
        }

        [Test]
        public void SlotOutsideTheLayoutNeverGoesToTheWire()
        {
            var stranger = HandsSlot.Of(new WeaponSlot { Kinds = new[] { WeaponKind.Gun } });

            Assert.Throws<InvalidOperationException>(() => _form.WireOf(new HandsState(stranger, HandsSlot.Empty, false)));
        }

        [TestCase(3)]
        [TestCase(-2)]
        [TestCase(sbyte.MaxValue)]
        public void IndexOutsideTheLayoutIsRefused(int index)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _form.ValueOf(new HandsSlots((sbyte)index, HandsSlots.Empty, false)));
            Assert.Throws<ArgumentOutOfRangeException>(() => _form.ValueOf(new HandsSlots(HandsSlots.Empty, (sbyte)index, false)));
        }

        private HandsSlot Slot(int index) =>
            index < 0 ? HandsSlot.Empty : HandsSlot.Of(_config.Slots[index]);
    }
}
