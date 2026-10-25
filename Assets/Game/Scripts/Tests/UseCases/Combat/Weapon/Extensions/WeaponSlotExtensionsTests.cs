using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Extensions;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon.Extensions
{
    public sealed class WeaponSlotExtensionsTests
    {
        [TestCase(WeaponKind.Gun, true)]
        [TestCase(WeaponKind.Prop, true)]
        [TestCase(WeaponKind.Fists, false)]
        public void SlotTakesTheKindsItLists(WeaponKind kind, bool taken)
        {
            var slot = new WeaponSlot { Kinds = new[] { WeaponKind.Gun, WeaponKind.Prop } };

            Assert.That(slot.IsFor(kind), Is.EqualTo(taken));
        }

        [TestCase(WeaponKind.Fists)]
        [TestCase(WeaponKind.Gun)]
        [TestCase(WeaponKind.Prop)]
        public void SlotWithoutKindsTakesNothing(WeaponKind kind)
        {
            Assert.That(new WeaponSlot().IsFor(kind), Is.False);
        }
    }
}
