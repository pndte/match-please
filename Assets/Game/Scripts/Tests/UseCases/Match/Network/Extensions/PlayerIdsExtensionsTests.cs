using System;
using Bw.Entities.Network;
using Bw.Entities.Players;
using Bw.UseCases.Match.Network.Extensions;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Match.Network.Extensions
{
    public sealed class PlayerIdsExtensionsTests
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
        public void TagOfAPlayerIsItsIdAndNickname()
        {
            var first = new Player("Сева");
            var second = new Player("Гошак");
            _ids.AddLifetimed(_lifetime.Lifetime, first);
            _ids.AddLifetimed(_lifetime.Lifetime, second);

            var tag = _ids.TagOf(second);

            Assert.That(tag.Id, Is.EqualTo(2));
            Assert.That(tag.Nickname, Is.EqualTo("Гошак"));
        }

        [Test]
        public void TagOfAPlayerWithoutAnIdThrows()
        {
            Assert.Throws<InvalidOperationException>(() => _ids.TagOf(new Player("Чужой")));
        }
    }
}
