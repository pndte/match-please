using Bw.Tests.UseCases.Combat.Hands.Doubles;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Abstractions.Inventory
{
    public sealed class InventoryContentTests
    {
        private static readonly WeaponSlot Fists = new() { Kinds = new[] { WeaponKind.Fists } };
        private static readonly WeaponSlot Gun = new() { Kinds = new[] { WeaponKind.Gun } };

        private readonly FakeWeapon _fists = new();
        private readonly FakeWeapon _gun = new();

        [Test]
        public void EmptyContentListsNothing()
        {
            Assert.That(InventoryContent.Empty.Items, Is.Empty);
            Assert.That(InventoryContent.Empty.IsListed(_gun, Gun), Is.False);
        }

        [Test]
        public void WeaponIsListedOnlyInItsOwnSlot()
        {
            var content = new InventoryContent(new[] { new InventoryItem(_gun, Gun) });

            Assert.That(content.IsListed(_gun, Gun), Is.True);
            Assert.That(content.IsListed(_gun, Fists), Is.False);
            Assert.That(content.IsListed(_fists, Gun), Is.False);
        }

        [Test]
        public void ContentsOfTheSameItemsInAnotherOrderAreEqual()
        {
            var first = new InventoryContent(new[] { new InventoryItem(_fists, Fists), new InventoryItem(_gun, Gun) });
            var second = new InventoryContent(new[] { new InventoryItem(_gun, Gun), new InventoryItem(_fists, Fists) });

            Assert.That(first.Equals(second), Is.True);
            Assert.That(first.Equals((object)second), Is.True);
            Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
        }

        [Test]
        public void ContentWithAWeaponInAnotherSlotDiffers()
        {
            var first = new InventoryContent(new[] { new InventoryItem(_gun, Gun) });
            var second = new InventoryContent(new[] { new InventoryItem(_gun, Fists) });

            Assert.That(first.Equals(second), Is.False);
        }

        [Test]
        public void ContentWithAnItemMoreDiffers()
        {
            var first = new InventoryContent(new[] { new InventoryItem(_fists, Fists) });
            var second = new InventoryContent(new[] { new InventoryItem(_fists, Fists), new InventoryItem(_gun, Gun) });

            Assert.That(first.Equals(second), Is.False);
            Assert.That(second.Equals(first), Is.False);
            Assert.That(InventoryContent.Empty.Equals(first), Is.False);
        }
    }
}
