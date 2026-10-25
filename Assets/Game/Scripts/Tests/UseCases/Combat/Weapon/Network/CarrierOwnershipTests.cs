using Bw.Entities.Extensions;
using Bw.Entities.Players;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon.Network
{
    public sealed class CarrierOwnershipTests
    {
        private LifetimeDefinition _lifetime;
        private FakeWeapon _weapon;
        private WeaponCarriers _carriers;
        private CharacterRegistry _characters;
        private OwnerRegistry _owners;
        private FakeCharacter _carrier;
        private Player _player;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _weapon = new FakeWeapon();
            _carriers = new WeaponCarriers();
            _characters = new CharacterRegistry();
            _owners = new OwnerRegistry();
            _carrier = new FakeCharacter();
            _player = new Player("Сева");
            _characters.PlayerByCharacter.AddLifetimed(_lifetime.Lifetime, _carrier, _player);
            new CarrierOwnership(_lifetime.Lifetime, _weapon, _carriers, _characters, _owners);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void WeaponOnTheFloorHasNoOwner()
        {
            Assert.That(_owners.Owners, Is.Empty);
        }

        [Test]
        public void CarriedWeaponIsOwnedByItsCarriersPlayer()
        {
            _carriers.ByWeapon.AddLifetimed(_lifetime.Lifetime, _weapon, _carrier);

            Assert.That(_owners.Owners, Is.EqualTo(new[] { _player }));
        }

        [Test]
        public void DroppedWeaponLosesItsOwner()
        {
            var carried = new LifetimeDefinition();
            _carriers.ByWeapon.AddLifetimed(carried.Lifetime, _weapon, _carrier);

            carried.Terminate();

            Assert.That(_owners.Owners, Is.Empty);
        }

        [Test]
        public void WeaponPassedOnIsOwnedByTheNextCarriersPlayer()
        {
            var next = new FakeCharacter();
            var nextPlayer = new Player("Матвей");
            _characters.PlayerByCharacter.AddLifetimed(_lifetime.Lifetime, next, nextPlayer);
            var carried = new LifetimeDefinition();
            _carriers.ByWeapon.AddLifetimed(carried.Lifetime, _weapon, _carrier);
            carried.Terminate();

            _carriers.ByWeapon.AddLifetimed(_lifetime.Lifetime, _weapon, next);

            Assert.That(_owners.Owners, Is.EqualTo(new[] { nextPlayer }));
        }

        [Test]
        public void AnotherCarriedWeaponGivesThisOneNoOwner()
        {
            _carriers.ByWeapon.AddLifetimed(_lifetime.Lifetime, new FakeWeapon(), _carrier);

            Assert.That(_owners.Owners, Is.Empty);
        }
    }
}
