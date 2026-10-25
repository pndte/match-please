using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Abstractions.Hands
{
    public sealed class HandsStateTests
    {
        private static readonly HandsSlot Fists = HandsSlot.Of(new WeaponSlot { Kinds = new[] { WeaponKind.Fists } });
        private static readonly HandsSlot Gun = HandsSlot.Of(new WeaponSlot { Kinds = new[] { WeaponKind.Gun } });

        [Test]
        public void EmptyHandsHoldNothingRememberNothingAndAreNotGuarded()
        {
            var empty = HandsState.Empty;

            Assert.That(empty.InHands, Is.EqualTo(HandsSlot.Empty));
            Assert.That(empty.Previous, Is.EqualTo(HandsSlot.Empty));
            Assert.That(empty.TakenByGuard, Is.False);
        }

        [Test]
        public void StatesOfEqualPartsAreEqual()
        {
            var first = new HandsState(Gun, Fists, true);
            var second = new HandsState(Gun, Fists, true);

            Assert.That(first.Equals(second), Is.True);
            Assert.That(first.Equals((object)second), Is.True);
            Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void StatesDifferingInOnePartDiffer(int part)
        {
            var state = new HandsState(Gun, Fists, false);
            var other = part switch
            {
                0 => new HandsState(Fists, Fists, false),
                1 => new HandsState(Gun, HandsSlot.Empty, false),
                _ => new HandsState(Gun, Fists, true)
            };

            Assert.That(state.Equals(other), Is.False);
        }
    }
}
