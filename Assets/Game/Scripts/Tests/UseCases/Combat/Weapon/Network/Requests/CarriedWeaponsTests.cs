using Bw.UseCases.Combat.Weapon.Network.Requests;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon.Network.Requests
{
    public sealed class CarriedWeaponsTests
    {
        [Test]
        public void CarriedWeaponIsItsIdAndSlot()
        {
            Assert.That(new CarriedWeapon(7, 1), Is.EqualTo(new CarriedWeapon(7, 1)));
            Assert.That(new CarriedWeapon(7, 1), Is.Not.EqualTo(new CarriedWeapon(7, 2)));
            Assert.That(new CarriedWeapon(7, 1), Is.Not.EqualTo(new CarriedWeapon(8, 1)));
        }

        [Test]
        public void EqualCarriedWeaponsHashAlike()
        {
            Assert.That(new CarriedWeapon(7, 1).GetHashCode(), Is.EqualTo(new CarriedWeapon(7, 1).GetHashCode()));
        }

        [Test]
        public void SameWeaponsInAnyOrderAreEqual()
        {
            var first = new CarriedWeapons(new[] { new CarriedWeapon(7, 0), new CarriedWeapon(9, 1) });
            var second = new CarriedWeapons(new[] { new CarriedWeapon(9, 1), new CarriedWeapon(7, 0) });

            Assert.That(first.Equals(second), Is.True);
            Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
        }

        [Test]
        public void MoreWeaponsDiffer()
        {
            var first = new CarriedWeapons(new[] { new CarriedWeapon(7, 0) });
            var second = new CarriedWeapons(new[] { new CarriedWeapon(7, 0), new CarriedWeapon(9, 1) });

            Assert.That(first.Equals(second), Is.False);
            Assert.That(second.Equals(first), Is.False);
        }

        [Test]
        public void WeaponInAnotherSlotDiffers()
        {
            var first = new CarriedWeapons(new[] { new CarriedWeapon(7, 0) });
            var second = new CarriedWeapons(new[] { new CarriedWeapon(7, 1) });

            Assert.That(first.Equals(second), Is.False);
        }

        [Test]
        public void EmptyHandsAreEqual()
        {
            Assert.That(new CarriedWeapons(new CarriedWeapon[0]).Equals(new CarriedWeapons(new CarriedWeapon[0])), Is.True);
        }

        [Test]
        public void BoxedWeaponsCompareByValue()
        {
            object boxed = new CarriedWeapons(new[] { new CarriedWeapon(7, 0) });

            Assert.That(new CarriedWeapons(new[] { new CarriedWeapon(7, 0) }).Equals(boxed), Is.True);
            Assert.That(new CarriedWeapon(7, 0).Equals((object)new CarriedWeapon(7, 0)), Is.True);
        }
    }
}
