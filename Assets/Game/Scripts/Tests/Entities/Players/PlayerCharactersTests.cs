using System;
using Bw.Entities.Players;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Players
{
    public sealed class PlayerCharactersTests
    {
        private LifetimeDefinition _lifetime;
        private PlayerCharacters _characters;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _characters = new PlayerCharacters();
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void NamedCharacterIsTheKeyOfThePlayer()
        {
            var player = new Player("Сева");

            _characters.AddLifetimed(_lifetime.Lifetime, player, 3);

            Assert.That(_characters.KeyOf(player), Is.EqualTo(3));
        }

        [Test]
        public void PlayerWithoutACharacterThrows()
        {
            var player = new Player("Сева");

            Assert.Throws<InvalidOperationException>(() => _characters.KeyOf(player));
        }

        [Test]
        public void CharacterIsForgottenWhenItsNamingEnds()
        {
            var player = new Player("Сева");
            var naming = _lifetime.Lifetime.CreateNested();
            _characters.AddLifetimed(naming.Lifetime, player, 3);

            naming.Terminate();

            Assert.Throws<InvalidOperationException>(() => _characters.KeyOf(player));
        }

        [Test]
        public void PlayerCannotBeNamedTwiceAtOnce()
        {
            var player = new Player("Сева");
            _characters.AddLifetimed(_lifetime.Lifetime, player, 3);

            Assert.Throws<InvalidOperationException>(() => _characters.AddLifetimed(_lifetime.Lifetime, player, 4));
            Assert.That(_characters.KeyOf(player), Is.EqualTo(3));
        }

        [Test]
        public void PlayerIsNamedAgainAfterTheLastNamingEnded()
        {
            var player = new Player("Сева");
            var first = _lifetime.Lifetime.CreateNested();
            _characters.AddLifetimed(first.Lifetime, player, 3);
            first.Terminate();

            _characters.AddLifetimed(_lifetime.Lifetime, player, 4);

            Assert.That(_characters.KeyOf(player), Is.EqualTo(4));
        }

        [Test]
        public void RefusedSecondNamingDoesNotEndTheFirstWithItsLifetime()
        {
            var player = new Player("Сева");
            _characters.AddLifetimed(_lifetime.Lifetime, player, 3);
            var refused = _lifetime.Lifetime.CreateNested();
            Assert.Throws<InvalidOperationException>(() => _characters.AddLifetimed(refused.Lifetime, player, 4));

            refused.Terminate();

            Assert.That(_characters.KeyOf(player), Is.EqualTo(3));
        }

        [Test]
        public void PlayersAreToldApartByIdentityNotByNickname()
        {
            var first = new Player("Сева");
            var second = new Player("Сева");

            _characters.AddLifetimed(_lifetime.Lifetime, first, 1);
            _characters.AddLifetimed(_lifetime.Lifetime, second, 2);

            Assert.That(_characters.KeyOf(first), Is.EqualTo(1));
            Assert.That(_characters.KeyOf(second), Is.EqualTo(2));
        }

        [Test]
        public void NamingOnAnEndedLifetimeIsRefusedAndLeavesNoCharacter()
        {
            var player = new Player("Сева");

            Assert.Throws<InvalidOperationException>(() => _characters.AddLifetimed(Lifetime.Terminated, player, 3));
            Assert.Throws<InvalidOperationException>(() => _characters.KeyOf(player));
        }

        [Test]
        public void PlayerRefusedOnAnEndedLifetimeMayBeNamedAgain()
        {
            var player = new Player("Сева");
            Assert.Throws<InvalidOperationException>(() => _characters.AddLifetimed(Lifetime.Terminated, player, 3));

            _characters.AddLifetimed(_lifetime.Lifetime, player, 4);

            Assert.That(_characters.KeyOf(player), Is.EqualTo(4));
        }
    }
}
