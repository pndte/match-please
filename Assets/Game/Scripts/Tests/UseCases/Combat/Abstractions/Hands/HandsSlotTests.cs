using System;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Abstractions.Hands
{
    public sealed class HandsSlotTests
    {
        private static readonly WeaponSlot Gun = new() { Kinds = new[] { WeaponKind.Gun } };
        private static readonly WeaponSlot SecondGun = new() { Kinds = new[] { WeaponKind.Gun } };

        [Test]
        public void EmptyEqualsOnlyEmpty()
        {
            Assert.That(HandsSlot.Empty.Equals(HandsSlot.Empty), Is.True);
            Assert.That(HandsSlot.Empty != HandsSlot.Of(Gun), Is.True);
            Assert.That(HandsSlot.Empty.Equals(HandsSlot.Of(Gun)), Is.False);
        }

        [Test]
        public void SlotsOfOneLayoutSlotAreEqual()
        {
            var first = HandsSlot.Of(Gun);
            var second = HandsSlot.Of(Gun);

            Assert.That(first == second, Is.True);
            Assert.That(first.Equals((object)second), Is.True);
            Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
        }

        [Test]
        public void TwoLayoutSlotsWithEqualContentStayDifferentSlots()
        {
            Assert.That(HandsSlot.Of(Gun) == HandsSlot.Of(SecondGun), Is.False);
        }

        [Test]
        public void SlotOfNoLayoutSlotThrows()
        {
            Assert.Throws<ArgumentNullException>(() => HandsSlot.Of(null));
        }

        [Test]
        public void NullIsNoSlot()
        {
            HandsSlot missing = null;

            Assert.That(missing == null, Is.True);
            Assert.That(HandsSlot.Empty == missing, Is.False);
            Assert.That(missing == HandsSlot.Empty, Is.False);
            Assert.That(HandsSlot.Of(Gun).Equals(missing), Is.False);
            Assert.That(HandsSlot.Empty.Equals((object)"empty"), Is.False);
        }

        [Test]
        public void SwitchOnEmptyTakesTheEmptyCase()
        {
            var taken = HandsSlot.Empty.Switch(7, state => $"empty {state}", (state, slot) => $"slot {state}");

            Assert.That(taken, Is.EqualTo("empty 7"));
        }

        [Test]
        public void SwitchOnASlotPassesTheLayoutSlot()
        {
            var taken = HandsSlot.Of(Gun).Switch(7, _ => null, (_, slot) => slot);

            Assert.That(taken, Is.SameAs(Gun));
        }
    }
}
