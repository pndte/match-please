using System;
using Bw.Entities.Network;
using Bw.Entities.Players;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network
{
    public sealed class PlayerDriversTests
    {
        private LifetimeDefinition _lifetime;
        private PlayerDrivers _drivers;
        private Client _client;
        private Player _human;
        private Player _bot;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _drivers = new PlayerDrivers();
            _client = new Client(7, new ConnectionRequest("Саня", 3));
            _human = new Player("Саня");
            _bot = new Player("Гошак");
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void ClientDrivenPlayerIsDrivenByItsClient()
        {
            _drivers.AddClientDriven(_lifetime.Lifetime, _human, _client);

            Assert.That(DriverOf(_human), Is.EqualTo("client 7"));
        }

        [Test]
        public void ServerDrivenPlayerIsDrivenByTheServer()
        {
            _drivers.AddServerDriven(_lifetime.Lifetime, _bot);

            Assert.That(DriverOf(_bot), Is.EqualTo("server"));
        }

        [Test]
        public void ClientFindsItsPlayer()
        {
            _drivers.AddClientDriven(_lifetime.Lifetime, _human, _client);

            Assert.That(_drivers.PlayerOf(_client), Is.SameAs(_human));
        }

        [Test]
        public void OnlyClientDrivenPlayersAreListedByClient()
        {
            _drivers.AddClientDriven(_lifetime.Lifetime, _human, _client);
            _drivers.AddServerDriven(_lifetime.Lifetime, _bot);

            Assert.That(_drivers.ByClient.Count, Is.EqualTo(1));
            Assert.That(_drivers.ByClient[_client], Is.SameAs(_human));
        }

        [Test]
        public void PlayerWithoutADriverThrows()
        {
            Assert.Throws<InvalidOperationException>(() => _drivers.Of(new Player("Никто")));
        }

        [Test]
        public void ClientWithoutAPlayerThrows()
        {
            _drivers.AddClientDriven(_lifetime.Lifetime, _human, _client);
            var stranger = new Client(9, new ConnectionRequest("Чужой", 2));

            Assert.Throws<InvalidOperationException>(() => _drivers.PlayerOf(stranger));
        }

        [Test]
        public void SecondDriverForTheSamePlayerThrows()
        {
            _drivers.AddServerDriven(_lifetime.Lifetime, _bot);

            Assert.Throws<InvalidOperationException>(() => _drivers.AddServerDriven(_lifetime.Lifetime, _bot));
            Assert.Throws<InvalidOperationException>(() => _drivers.AddClientDriven(_lifetime.Lifetime, _bot, _client));
            Assert.That(DriverOf(_bot), Is.EqualTo("server"));
        }

        [Test]
        public void ClientDrivingAPlayerCannotDriveASecondOne()
        {
            _drivers.AddClientDriven(_lifetime.Lifetime, _human, _client);

            Assert.Catch(() => _drivers.AddClientDriven(_lifetime.Lifetime, _bot, _client));
            Assert.That(_drivers.PlayerOf(_client), Is.SameAs(_human));
        }

        [Test]
        public void RefusedClientDriverLeavesThePlayerWithoutADriver()
        {
            _drivers.AddClientDriven(_lifetime.Lifetime, _human, _client);

            Assert.Catch(() => _drivers.AddClientDriven(_lifetime.Lifetime, _bot, _client));

            Assert.Throws<InvalidOperationException>(() => _drivers.Of(_bot));
        }

        [Test]
        public void DriverLeavesWithItsLifetime()
        {
            var session = new LifetimeDefinition();
            _drivers.AddClientDriven(session.Lifetime, _human, _client);

            session.Terminate();

            Assert.Throws<InvalidOperationException>(() => _drivers.Of(_human));
            Assert.Throws<InvalidOperationException>(() => _drivers.PlayerOf(_client));
            Assert.That(_drivers.ByClient.Count, Is.EqualTo(0));
        }

        [Test]
        public void PlayerCanBeDrivenAgainAfterItsDriverLeft()
        {
            var bot = new LifetimeDefinition();
            _drivers.AddServerDriven(bot.Lifetime, _human);
            bot.Terminate();

            _drivers.AddClientDriven(_lifetime.Lifetime, _human, _client);

            Assert.That(DriverOf(_human), Is.EqualTo("client 7"));
        }

        private string DriverOf(IPlayer player)
        {
            var named = "";
            _drivers.Of(player).Value.Switch(client => named = "client " + client.Id.ToString(), () => named = "server");
            return named;
        }
    }
}
