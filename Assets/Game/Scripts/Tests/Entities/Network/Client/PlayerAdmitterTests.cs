using System;
using System.Collections.Generic;
using Bw.Entities.Network;
using Bw.Entities.Players;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network
{
    public sealed class PlayerAdmitterTests
    {
        private LifetimeDefinition _lifetime;
        private PlayerIds _ids;
        private PlayerDrivers _drivers;
        private PlayerCharacters _characters;
        private PlayerCollection _players;
        private PlayerAdmitter _admitter;
        private List<string> _announced;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _ids = new PlayerIds();
            _drivers = new PlayerDrivers();
            _characters = new PlayerCharacters();
            _players = new PlayerCollection();
            _admitter = new PlayerAdmitter(_ids, _drivers, _characters, _players);
            _announced = new List<string>();
            _players.Advise(_lifetime.Lifetime, change =>
            {
                if (change.Kind == AddRemove.Add)
                    _announced.Add($"{change.Value.Nickname}: {DriverOf(change.Value)}, character {_characters.KeyOf(change.Value).ToString()}, id {_ids.IdOf(change.Value).ToString()}");
            });
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void ClientPlayerIsAnnouncedWithItsIdDriverAndCharacterKnown()
        {
            var client = new Client(new ConnectionRequest("Саня", 3));

            _admitter.Admit(_lifetime.Lifetime, new Player("Саня"), 3, PlayerDriver.Client(client));

            Assert.That(_announced, Is.EqualTo(new[] { "Саня: client Саня, character 3, id 1" }));
        }

        [Test]
        public void BotIsAnnouncedAsDrivenByTheServer()
        {
            _admitter.Admit(_lifetime.Lifetime, new Player("Гошак"), 1, PlayerDriver.Server);

            Assert.That(_announced, Is.EqualTo(new[] { "Гошак: server, character 1, id 1" }));
        }

        [Test]
        public void ClientPlayerIsListedByItsClient()
        {
            var client = new Client(new ConnectionRequest("Саня", 3));
            var player = new Player("Саня");

            _admitter.Admit(_lifetime.Lifetime, player, 3, PlayerDriver.Client(client));

            Assert.That(_drivers.PlayerOf(client), Is.SameAs(player));
        }

        [Test]
        public void BotIsNotListedByAnyClient()
        {
            _admitter.Admit(_lifetime.Lifetime, new Player("Гошак"), 1, PlayerDriver.Server);

            Assert.That(_drivers.ByClient.Count, Is.EqualTo(0));
        }

        [Test]
        public void AdmissionEndsWithItsLifetime()
        {
            var stay = new LifetimeDefinition();
            var client = new Client(new ConnectionRequest("Саня", 3));
            var player = new Player("Саня");
            _admitter.Admit(stay.Lifetime, player, 3, PlayerDriver.Client(client));

            stay.Terminate();

            Assert.That(_players.Count, Is.EqualTo(0));
            Assert.Throws<InvalidOperationException>(() => _ids.IdOf(player));
            Assert.Throws<InvalidOperationException>(() => _drivers.Of(player));
            Assert.Throws<InvalidOperationException>(() => _drivers.PlayerOf(client));
            Assert.Throws<InvalidOperationException>(() => _characters.KeyOf(player));
        }

        [Test]
        public void SecondAdmissionOfThePlayerThrowsAndChangesNothing()
        {
            var bot = new Player("Гошак");
            _admitter.Admit(_lifetime.Lifetime, bot, 1, PlayerDriver.Server);

            Assert.Throws<InvalidOperationException>(() => _admitter.Admit(_lifetime.Lifetime, bot, 2, PlayerDriver.Server));

            Assert.That(_ids.IdOf(bot), Is.EqualTo(1));
            Assert.That(_characters.KeyOf(bot), Is.EqualTo(1));
            Assert.That(_players.Count, Is.EqualTo(1));
            Assert.That(_announced, Has.Count.EqualTo(1));
        }

        [Test]
        public void IdsOfLeftPlayersAreNotGivenToNewOnes()
        {
            var first = new LifetimeDefinition();
            _admitter.Admit(first.Lifetime, new Player("Сева"), 4, PlayerDriver.Server);
            first.Terminate();

            _admitter.Admit(_lifetime.Lifetime, new Player("Матвей"), 2, PlayerDriver.Server);

            Assert.That(_announced[^1], Is.EqualTo("Матвей: server, character 2, id 2"));
        }

        [Test]
        public void AdmissionRefusedForABusyClientLeavesNoTrace()
        {
            var client = new Client(new ConnectionRequest("Саня", 3));
            _admitter.Admit(_lifetime.Lifetime, new Player("Саня"), 3, PlayerDriver.Client(client));
            var second = new Player("Сева");

            Assert.Catch(() => _admitter.Admit(_lifetime.Lifetime, second, 4, PlayerDriver.Client(client)));

            Assert.Throws<InvalidOperationException>(() => _ids.IdOf(second));
            Assert.Throws<InvalidOperationException>(() => _drivers.Of(second));
        }

        [Test]
        public void RefusedAdmissionSpendsNoId()
        {
            var client = new Client(new ConnectionRequest("Саня", 3));
            _admitter.Admit(_lifetime.Lifetime, new Player("Саня"), 3, PlayerDriver.Client(client));
            Assert.Catch(() => _admitter.Admit(_lifetime.Lifetime, new Player("Сева"), 4, PlayerDriver.Client(client)));

            _admitter.Admit(_lifetime.Lifetime, new Player("Матвей"), 2, PlayerDriver.Server);

            Assert.That(_announced[^1], Is.EqualTo("Матвей: server, character 2, id 2"));
        }

        [Test]
        public void PlayerHasItsIdWhenItsClientIsListed()
        {
            var listedIds = new List<int>();
            _drivers.ByClient.View(_lifetime.Lifetime, (_, _, player) => listedIds.Add(_ids.IdOf(player)));

            _admitter.Admit(_lifetime.Lifetime, new Player("Саня"), 3, PlayerDriver.Client(new Client(new ConnectionRequest("Саня", 3))));

            Assert.That(listedIds, Is.EqualTo(new[] { 1 }));
        }

        private string DriverOf(IPlayer player)
        {
            var named = "";
            _drivers.Of(player).Value.Switch(client => named = "client " + client.Request.Nickname, () => named = "server");
            return named;
        }
    }
}
