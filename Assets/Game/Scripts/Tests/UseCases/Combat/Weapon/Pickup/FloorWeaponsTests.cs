using System;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Weapon.Pickup;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.Weapon.Pickup
{
    public sealed class FloorWeaponsTests
    {
        private LifetimeDefinition _lifetime;
        private FloorWeapons _floor;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _floor = new FloorWeapons();
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void FloorStartsEmpty()
        {
            Assert.That(_floor.Lying, Is.Empty);
        }

        [Test]
        public void AddedWeaponsLieInTheOrderTheyCame()
        {
            var first = new FakeFloorWeapon(Vector2.zero);
            var second = new FakeFloorWeapon(Vector2.one);

            _floor.AddLifetimed(_lifetime.Lifetime, first);
            _floor.AddLifetimed(_lifetime.Lifetime, second);

            Assert.That(_floor.Lying, Is.EqualTo(new[] { first, second }));
        }

        [Test]
        public void WeaponLeavesTheFloorWithItsLifetime()
        {
            var stay = new LifetimeDefinition();
            var weapon = new FakeFloorWeapon(Vector2.zero);
            var other = new FakeFloorWeapon(Vector2.one);
            _floor.AddLifetimed(stay.Lifetime, weapon);
            _floor.AddLifetimed(_lifetime.Lifetime, other);

            stay.Terminate();

            Assert.That(_floor.Lying, Is.EqualTo(new[] { other }));
        }

        [Test]
        public void WeaponLiesOnTheFloorOnceAtATime()
        {
            var weapon = new FakeFloorWeapon(Vector2.zero);
            _floor.AddLifetimed(_lifetime.Lifetime, weapon);

            Assert.Throws<InvalidOperationException>(() => _floor.AddLifetimed(_lifetime.Lifetime, weapon));
            Assert.That(_floor.Lying, Has.Count.EqualTo(1));
        }

        [Test]
        public void WeaponLiesAgainAfterItsLastStayEnded()
        {
            var stay = new LifetimeDefinition();
            var weapon = new FakeFloorWeapon(Vector2.zero);
            _floor.AddLifetimed(stay.Lifetime, weapon);
            stay.Terminate();

            _floor.AddLifetimed(_lifetime.Lifetime, weapon);

            Assert.That(_floor.Lying, Is.EqualTo(new[] { weapon }));
        }
    }
}
