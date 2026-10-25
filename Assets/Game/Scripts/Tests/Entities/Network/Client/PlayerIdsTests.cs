using System;
using Bw.Entities.Network;
using Bw.Entities.Players;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network
{
    public sealed class PlayerIdsTests
    {
        private LifetimeDefinition _lifetime;
        private PlayerIds _ids;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _ids = new PlayerIds();
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void IdsAreIssuedFromOneInAdmissionOrder()
        {
            var first = new Player("Саня");
            var second = new Player("Гошак");
            var third = new Player("Сева");

            _ids.AddLifetimed(_lifetime.Lifetime, first);
            _ids.AddLifetimed(_lifetime.Lifetime, second);
            _ids.AddLifetimed(_lifetime.Lifetime, third);

            Assert.That(_ids.IdOf(first), Is.EqualTo(1));
            Assert.That(_ids.IdOf(second), Is.EqualTo(2));
            Assert.That(_ids.IdOf(third), Is.EqualTo(3));
        }

        [Test]
        public void PlayerWithoutAnIdThrows()
        {
            Assert.Throws<InvalidOperationException>(() => _ids.IdOf(new Player("Никто")));
        }

        [Test]
        public void IdIsTakenBackWhenItsLifetimeEnds()
        {
            var player = new Player("Саня");
            var stay = new LifetimeDefinition();
            _ids.AddLifetimed(stay.Lifetime, player);

            stay.Terminate();

            Assert.Throws<InvalidOperationException>(() => _ids.IdOf(player));
        }

        [Test]
        public void IdIsNeverReusedAfterItsPlayerLeft()
        {
            var leaving = new Player("Сева");
            var stay = new LifetimeDefinition();
            _ids.AddLifetimed(stay.Lifetime, leaving);
            stay.Terminate();
            var next = new Player("Матвей");

            _ids.AddLifetimed(_lifetime.Lifetime, next);

            Assert.That(_ids.IdOf(next), Is.EqualTo(2));
        }

        [Test]
        public void ReturningPlayerGetsANewId()
        {
            var player = new Player("Сева");
            var first = new LifetimeDefinition();
            _ids.AddLifetimed(first.Lifetime, player);
            first.Terminate();

            _ids.AddLifetimed(_lifetime.Lifetime, player);

            Assert.That(_ids.IdOf(player), Is.EqualTo(2));
        }

        [Test]
        public void SecondIdForTheSamePlayerThrows()
        {
            var player = new Player("Саня");
            _ids.AddLifetimed(_lifetime.Lifetime, player);

            Assert.Throws<InvalidOperationException>(() => _ids.AddLifetimed(_lifetime.Lifetime, player));
            Assert.That(_ids.IdOf(player), Is.EqualTo(1));
        }

        [Test]
        public void RefusedSecondIdUsesUpNoNumber()
        {
            var player = new Player("Саня");
            _ids.AddLifetimed(_lifetime.Lifetime, player);
            Assert.Throws<InvalidOperationException>(() => _ids.AddLifetimed(_lifetime.Lifetime, player));
            var next = new Player("Гошак");

            _ids.AddLifetimed(_lifetime.Lifetime, next);

            Assert.That(_ids.IdOf(next), Is.EqualTo(2));
        }

        [Test]
        public void PlayersWithTheSameNicknameHaveTheirOwnIds()
        {
            var first = new Player("Player");
            var second = new Player("Player");

            _ids.AddLifetimed(_lifetime.Lifetime, first);
            _ids.AddLifetimed(_lifetime.Lifetime, second);

            Assert.That(_ids.IdOf(first), Is.Not.EqualTo(_ids.IdOf(second)));
        }
    }
}
