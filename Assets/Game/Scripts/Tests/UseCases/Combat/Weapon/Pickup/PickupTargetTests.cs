using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Weapon.Pickup;
using Bw.UseCases.Combat.Weapon.Pickup.Abstractions;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.Weapon.Pickup
{
    public sealed class PickupTargetTests
    {
        private FakeFloorWeapon _weapon;
        private FakeFloorWeapon _other;

        [SetUp]
        public void SetUp()
        {
            _weapon = new FakeFloorWeapon(Vector2.zero);
            _other = new FakeFloorWeapon(Vector2.one);
        }

        [Test]
        public void NothingIsNoWeapon()
        {
            Assert.That(PickupTarget.Nothing.IsWeapon(_weapon), Is.False);
        }

        [Test]
        public void WeaponTargetIsItsWeaponOnly()
        {
            var target = PickupTarget.Of(_weapon);

            Assert.That(target.IsWeapon(_weapon), Is.True);
            Assert.That(target.IsWeapon(_other), Is.False);
        }

        [Test]
        public void NothingSwitchesToTheNothingBranch()
        {
            var branch = "";

            PickupTarget.Nothing.Switch(() => branch = "nothing", _ => branch = "weapon");

            Assert.That(branch, Is.EqualTo("nothing"));
        }

        [Test]
        public void WeaponTargetSwitchesToTheWeaponBranchWithItsWeapon()
        {
            IFloorWeapon switched = null;

            PickupTarget.Of(_weapon).Switch(() => Assert.Fail("A weapon target is not nothing."), weapon => switched = weapon);

            Assert.That(switched, Is.SameAs(_weapon));
        }

        [Test]
        public void TargetsOfTheSameWeaponAreEqual()
        {
            var first = PickupTarget.Of(_weapon);
            var second = PickupTarget.Of(_weapon);

            Assert.That(first.Equals(second), Is.True);
            Assert.That(first.Equals((object)second), Is.True);
            Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
        }

        [Test]
        public void TargetsOfOtherWeaponsDiffer()
        {
            Assert.That(PickupTarget.Of(_weapon).Equals(PickupTarget.Of(_other)), Is.False);
        }

        [Test]
        public void NothingIsEqualOnlyToNothing()
        {
            Assert.That(PickupTarget.Nothing.Equals(PickupTarget.Nothing), Is.True);
            Assert.That(PickupTarget.Nothing.Equals(PickupTarget.Of(_weapon)), Is.False);
            Assert.That(PickupTarget.Of(_weapon).Equals(PickupTarget.Nothing), Is.False);
        }

        [Test]
        public void TargetIsNeverEqualToSomethingElse()
        {
            Assert.That(PickupTarget.Of(_weapon).Equals((object)_weapon), Is.False);
        }
    }
}
