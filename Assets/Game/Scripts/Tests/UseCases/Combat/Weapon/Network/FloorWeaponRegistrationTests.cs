using Bw.Entities.Extensions;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Combat.Weapon.Pickup;
using Bw.UseCases.Combat.Weapon.Pickup.Abstractions;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.Weapon.Network
{
    public sealed class FloorWeaponRegistrationTests
    {
        private LifetimeDefinition _lifetime;
        private FakeNetworkObject _networkObject;
        private FakeWeapon _weapon;
        private WeaponCarriers _carriers;
        private LyingWeapon _lying;
        private FloorWeapons _floor;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _networkObject = new FakeNetworkObject();
            _weapon = new FakeWeapon();
            _carriers = new WeaponCarriers();
            _lying = new LyingWeapon();
            _floor = new FloorWeapons();
            new FloorWeaponRegistration(_lifetime.Lifetime, _networkObject, _weapon, _carriers, _lying, _floor);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void WeaponNotSpawnedIsNotOnTheFloor()
        {
            Assert.That(_floor.Lying, Is.Empty);
        }

        [Test]
        public void SpawnedWeaponNobodyCarriesLiesOnTheFloor()
        {
            _networkObject.Spawn();

            Assert.That(_floor.Lying, Is.EqualTo(new[] { _lying }));
        }

        [Test]
        public void PickedUpWeaponLeavesTheFloor()
        {
            _networkObject.Spawn();

            _carriers.ByWeapon.AddLifetimed(_lifetime.Lifetime, _weapon, new FakeCharacter());

            Assert.That(_floor.Lying, Is.Empty);
        }

        [Test]
        public void DroppedWeaponLiesOnTheFloorAgain()
        {
            _networkObject.Spawn();
            var carried = new LifetimeDefinition();
            _carriers.ByWeapon.AddLifetimed(carried.Lifetime, _weapon, new FakeCharacter());

            carried.Terminate();

            Assert.That(_floor.Lying, Is.EqualTo(new[] { _lying }));
        }

        [Test]
        public void WeaponSpawnedInHandsIsNotOnTheFloor()
        {
            _carriers.ByWeapon.AddLifetimed(_lifetime.Lifetime, _weapon, new FakeCharacter());

            _networkObject.Spawn();

            Assert.That(_floor.Lying, Is.Empty);
        }

        [Test]
        public void DespawnedWeaponLeavesTheFloor()
        {
            _networkObject.Spawn();

            _networkObject.Despawn();

            Assert.That(_floor.Lying, Is.Empty);
        }

        private sealed class LyingWeapon : IFloorWeapon
        {
            public Vector2 Position => Vector2.zero;

            public float DistanceTo(Vector2 point) =>
                point.magnitude;

            public void Highlight(Lifetime highlightLifetime)
            {
            }
        }
    }
}
