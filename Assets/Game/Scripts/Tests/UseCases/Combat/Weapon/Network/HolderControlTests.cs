using Bw.Entities.Extensions;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Characters;
using Bw.UseCases.Combat.Hands;
using Bw.UseCases.Combat.Weapon.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon.Network
{
    public sealed class HolderControlTests
    {
        private LifetimeDefinition _lifetime;
        private FakeWeapon _weapon;
        private WeaponsInHands _inHands;
        private ControlledCharacters _controlled;
        private FakeCharacter _holder;
        private HolderControl _control;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _weapon = new FakeWeapon();
            _inHands = new WeaponsInHands();
            _controlled = new ControlledCharacters();
            _holder = new FakeCharacter();
            _control = new HolderControl(_lifetime.Lifetime, _weapon, _inHands, _controlled);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void WeaponInNobodysHandsIsNotControlledHere()
        {
            _controlled.Add(_lifetime.Lifetime, _holder);

            Assert.That(_control.Me.Value, Is.False);
        }

        [Test]
        public void WeaponInTheHandsOfACharacterControlledHereIsControlledHere()
        {
            _controlled.Add(_lifetime.Lifetime, _holder);

            _inHands.ByWeapon.AddLifetimed(_lifetime.Lifetime, _weapon, _holder);

            Assert.That(_control.Me.Value, Is.True);
        }

        [Test]
        public void WeaponInTheHandsOfAnotherCharacterIsNotControlledHere()
        {
            _controlled.Add(_lifetime.Lifetime, new FakeCharacter());

            _inHands.ByWeapon.AddLifetimed(_lifetime.Lifetime, _weapon, _holder);

            Assert.That(_control.Me.Value, Is.False);
        }

        [Test]
        public void CharacterControlledAfterTheDrawTakesTheWeaponsControl()
        {
            _inHands.ByWeapon.AddLifetimed(_lifetime.Lifetime, _weapon, _holder);

            _controlled.Add(_lifetime.Lifetime, _holder);

            Assert.That(_control.Me.Value, Is.True);
        }

        [Test]
        public void StowedWeaponLosesControl()
        {
            _controlled.Add(_lifetime.Lifetime, _holder);
            var drawn = new LifetimeDefinition();
            _inHands.ByWeapon.AddLifetimed(drawn.Lifetime, _weapon, _holder);

            drawn.Terminate();

            Assert.That(_control.Me.Value, Is.False);
        }

        [Test]
        public void HolderLosingControlTakesItFromTheWeapon()
        {
            var controlled = new LifetimeDefinition();
            _controlled.Add(controlled.Lifetime, _holder);
            _inHands.ByWeapon.AddLifetimed(_lifetime.Lifetime, _weapon, _holder);

            controlled.Terminate();

            Assert.That(_control.Me.Value, Is.False);
        }

        [Test]
        public void AnotherWeaponInTheHoldersHandsDoesNotGiveControl()
        {
            _controlled.Add(_lifetime.Lifetime, _holder);

            _inHands.ByWeapon.AddLifetimed(_lifetime.Lifetime, new FakeWeapon(), _holder);

            Assert.That(_control.Me.Value, Is.False);
        }
    }
}
