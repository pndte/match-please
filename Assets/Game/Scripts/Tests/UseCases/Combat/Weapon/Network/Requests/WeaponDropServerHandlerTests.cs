using Bw.Entities.Extensions;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Hands;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using JetBrains.Core;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon.Network.Requests
{
    public sealed class WeaponDropServerHandlerTests
    {
        private LifetimeDefinition _lifetime;
        private FakeWeapon _weapon;
        private WeaponsInHands _inHands;
        private WeaponCarriers _carriers;
        private ManualResults<Unit> _requests;
        private RecordingInventory _inventory;
        private FakeCharacter _carrier;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _weapon = new FakeWeapon();
            _inHands = new WeaponsInHands();
            _carriers = new WeaponCarriers();
            _requests = new ManualResults<Unit>();
            _inventory = new RecordingInventory(new WeaponSlot());
            _carrier = new FakeCharacter(new FakeMortal(), _inventory);
            _inventory.Give(_inventory.Slots[0], _weapon);
            new WeaponDropServerHandler(_lifetime.Lifetime, _weapon, _inHands, _requests, _carriers);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void RequestDropsTheWeaponInHandsFromItsCarriersInventory()
        {
            _carriers.ByWeapon.AddLifetimed(_lifetime.Lifetime, _weapon, _carrier);
            _inHands.ByWeapon.AddLifetimed(_lifetime.Lifetime, _weapon, _carrier);

            _requests.Deliver(Unit.Instance);

            Assert.That(_inventory.Dropped, Is.EqualTo(new[] { _weapon }));
        }

        [Test]
        public void StowedWeaponIsNotDropped()
        {
            _carriers.ByWeapon.AddLifetimed(_lifetime.Lifetime, _weapon, _carrier);

            _requests.Deliver(Unit.Instance);

            Assert.That(_inventory.Dropped, Is.Empty);
        }

        [Test]
        public void WeaponNobodyCarriesIsNotDropped()
        {
            _inHands.ByWeapon.AddLifetimed(_lifetime.Lifetime, _weapon, _carrier);

            _requests.Deliver(Unit.Instance);

            Assert.That(_inventory.Dropped, Is.Empty);
        }

        [Test]
        public void RequestsAfterTheHandlersLifetimeAreIgnored()
        {
            _carriers.ByWeapon.AddLifetimed(_lifetime.Lifetime, _weapon, _carrier);
            _inHands.ByWeapon.AddLifetimed(_lifetime.Lifetime, _weapon, _carrier);
            var handler = new LifetimeDefinition();
            var requests = new ManualResults<Unit>();
            new WeaponDropServerHandler(handler.Lifetime, _weapon, _inHands, requests, _carriers);
            handler.Terminate();

            requests.Deliver(Unit.Instance);

            Assert.That(_inventory.Dropped, Is.Empty);
        }
    }
}
