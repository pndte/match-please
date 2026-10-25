using System;
using System.Collections.Generic;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Hands;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Hands
{
    public sealed class DrawChoiceTests
    {
        private static readonly WeaponSlot Fists = new() { Kinds = new[] { WeaponKind.Fists } };
        private static readonly WeaponSlot Gun = new() { Kinds = new[] { WeaponKind.Gun } };

        private static IEnumerable<TestCaseData> EqualChoices()
        {
            yield return new TestCaseData(DrawChoice.Keep, DrawChoice.Keep).SetName("KeepEqualsKeep");
            yield return new TestCaseData(DrawChoice.Last, DrawChoice.Last).SetName("LastEqualsLast");
            yield return new TestCaseData(DrawChoice.ToSlot(Gun), DrawChoice.ToSlot(Gun)).SetName("ChoicesOfOneSlotAreEqual");
            yield return new TestCaseData(DrawChoice.Scrolled(1), DrawChoice.Scrolled(1)).SetName("ForwardScrollsAreEqual");
            yield return new TestCaseData(DrawChoice.Scrolled(-1), DrawChoice.Scrolled(-1)).SetName("BackScrollsAreEqual");
        }

        private static IEnumerable<TestCaseData> DifferentChoices()
        {
            yield return new TestCaseData(DrawChoice.Keep, DrawChoice.Last).SetName("KeepIsNotLast");
            yield return new TestCaseData(DrawChoice.ToSlot(Gun), DrawChoice.ToSlot(Fists)).SetName("ChoicesOfTwoSlotsDiffer");
            yield return new TestCaseData(DrawChoice.Scrolled(1), DrawChoice.Scrolled(-1)).SetName("ScrollsOfTwoDirectionsDiffer");
            yield return new TestCaseData(DrawChoice.Keep, DrawChoice.Scrolled(1)).SetName("KeepIsNotAScroll");
            yield return new TestCaseData(DrawChoice.Last, DrawChoice.ToSlot(Gun)).SetName("LastIsNotASlot");
        }

        [TestCaseSource(nameof(EqualChoices))]
        public void EqualChoicesAreEqualValues(DrawChoice first, DrawChoice second)
        {
            Assert.That(first.Equals(second), Is.True);
            Assert.That(first.Equals((object)second), Is.True);
            Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
        }

        [TestCaseSource(nameof(DifferentChoices))]
        public void DifferentChoicesAreDifferentValues(DrawChoice first, DrawChoice second)
        {
            Assert.That(first.Equals(second), Is.False);
            Assert.That(second.Equals(first), Is.False);
        }

        [TestCase(0)]
        [TestCase(2)]
        [TestCase(-2)]
        public void ScrollOfMoreOrLessThanOneSlotThrows(int direction)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DrawChoice.Scrolled(direction));
        }

        [Test]
        public void ChoiceOfNoSlotThrows()
        {
            Assert.Throws<ArgumentNullException>(() => DrawChoice.ToSlot(null));
        }

        [Test]
        public void ChoiceIsNeverNull()
        {
            Assert.That(DrawChoice.Keep.Equals(null), Is.False);
            Assert.That(DrawChoice.Keep.Equals((object)"keep"), Is.False);
        }

        [Test]
        public void SwitchTakesTheCaseOfEachChoice()
        {
            Assert.That(Case(DrawChoice.Keep), Is.EqualTo("keep"));
            Assert.That(Case(DrawChoice.ToSlot(Gun)), Is.EqualTo("slot gun"));
            Assert.That(Case(DrawChoice.Last), Is.EqualTo("last"));
            Assert.That(Case(DrawChoice.Scrolled(-1)), Is.EqualTo("scrolled -1"));
        }

        private static string Case(DrawChoice choice) =>
            choice.Switch(
                Gun,
                _ => "keep",
                (gun, slot) => slot == gun ? "slot gun" : "slot other",
                _ => "last",
                (_, direction) => $"scrolled {direction}");
    }
}
