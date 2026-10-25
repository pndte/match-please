using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Weapon.Pickup;
using Bw.UseCases.Combat.Weapon.Pickup.Extensions;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.Weapon.Pickup.Extensions
{
    public sealed class FloorWeaponsExtensionsTests
    {
        private static readonly Vector2 Holder = Vector2.zero;

        private LifetimeDefinition _lifetime;
        private WeaponPickupConfig _config;
        private FloorWeapons _floor;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _config = ScriptableObject.CreateInstance<WeaponPickupConfig>();
            _config.Reach = 2.5f;
            _config.HoverRadius = 0.35f;
            _floor = new FloorWeapons();
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void EmptyFloorHasNothingToPickUp()
        {
            Assert.That(_floor.TryFindPickable(Vector2.zero, Holder, _config, out var pickable), Is.False);
            Assert.That(pickable, Is.Null);
        }

        [Test]
        public void WeaponUnderTheCursorWithinReachIsPickable()
        {
            var weapon = Lying(new Vector2(2f, 0f));

            Assert.That(_floor.TryFindPickable(new Vector2(2.1f, 0f), Holder, _config, out var pickable), Is.True);
            Assert.That(pickable, Is.SameAs(weapon));
        }

        [Test]
        public void WeaponAwayFromTheCursorIsNotPickable()
        {
            Lying(new Vector2(2f, 0f));

            Assert.That(_floor.TryFindPickable(new Vector2(2f, 1f), Holder, _config, out _), Is.False);
        }

        [Test]
        public void WeaponOutOfReachIsNotPickable()
        {
            Lying(new Vector2(4f, 0f));

            Assert.That(_floor.TryFindPickable(new Vector2(4f, 0f), Holder, _config, out _), Is.False);
        }

        [Test]
        public void WeaponNearestToTheCursorWins()
        {
            Lying(new Vector2(1.7f, 0f));
            var nearest = Lying(new Vector2(2f, 0f));
            Lying(new Vector2(2.2f, 0f));

            _floor.TryFindPickable(new Vector2(2.05f, 0f), Holder, _config, out var pickable);

            Assert.That(pickable, Is.SameAs(nearest));
        }

        [Test]
        public void NearerWeaponOutOfReachGivesWayToTheNextNearest()
        {
            var inReach = Lying(new Vector2(2.3f, 0f));
            Lying(new Vector2(2.6f, 0f));

            _floor.TryFindPickable(new Vector2(2.55f, 0f), Holder, _config, out var pickable);

            Assert.That(pickable, Is.SameAs(inReach));
        }

        [Test]
        public void FirstOfEquallyNearWeaponsWins()
        {
            var first = Lying(new Vector2(1f, 0.1f));
            Lying(new Vector2(1f, -0.1f));

            _floor.TryFindPickable(new Vector2(1f, 0f), Holder, _config, out var pickable);

            Assert.That(pickable, Is.SameAs(first));
        }

        private FakeFloorWeapon Lying(Vector2 position)
        {
            var weapon = new FakeFloorWeapon(position);
            _floor.AddLifetimed(_lifetime.Lifetime, weapon);
            return weapon;
        }
    }
}
