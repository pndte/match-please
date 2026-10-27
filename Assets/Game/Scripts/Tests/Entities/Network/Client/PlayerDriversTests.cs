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
            _client = new Client(new ConnectionRequest("Саня", 3, System.Guid.NewGuid(), System.Guid.Empty));
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

            Assert.That(DriverOf(_human), Is.EqualTo("client Саня"));
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
            var stranger = new Client(new ConnectionRequest("Чужой", 2, System.Guid.NewGuid(), System.Guid.Empty));

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

            Assert.That(DriverOf(_human), Is.EqualTo("client Саня"));
        }

        [Test]
        public void PlayerWhoseClientLeftIsDrivenByTheServerAndFreesTheClient()
        {
            _drivers.AddClientDriven(_lifetime.Lifetime, _human, _client);

            _drivers.ChangeDriver(_human, PlayerDriver.Server);

            Assert.That(DriverOf(_human), Is.EqualTo("server"));
            Assert.That(_drivers.ByClient.ContainsKey(_client), Is.False);
        }

        [Test]
        public void ReturningClientDrivesItsPlayerAgain()
        {
            var returned = new Client(new ConnectionRequest("Саня", 3, System.Guid.NewGuid(), System.Guid.Empty));
            _drivers.AddClientDriven(_lifetime.Lifetime, _human, _client);
            _drivers.ChangeDriver(_human, PlayerDriver.Server);

            _drivers.ChangeDriver(_human, PlayerDriver.Client(returned));

            Assert.That(_drivers.PlayerOf(returned), Is.SameAs(_human));
            Assert.That(_drivers.Of(_human).Value, Is.Not.SameAs(PlayerDriver.Server));
        }

        [Test]
        public void ChangeToAClientThatDrivesAnotherPlayerThrows()
        {
            _drivers.AddClientDriven(_lifetime.Lifetime, _human, _client);
            _drivers.AddServerDriven(_lifetime.Lifetime, _bot);

            Assert.Throws<InvalidOperationException>(() => _drivers.ChangeDriver(_bot, PlayerDriver.Client(_client)));
        }

        [Test]
        public void PlayerLeavingTheSessionFreesItsCurrentClient()
        {
            var session = new LifetimeDefinition();
            var returned = new Client(new ConnectionRequest("Саня", 3, System.Guid.NewGuid(), System.Guid.Empty));
            _drivers.AddClientDriven(session.Lifetime, _human, _client);
            _drivers.ChangeDriver(_human, PlayerDriver.Client(returned));

            session.Terminate();

            Assert.That(_drivers.ByClient.ContainsKey(returned), Is.False);
            Assert.That(_drivers.ByClient.ContainsKey(_client), Is.False);
        }

        private string DriverOf(IPlayer player)
        {
            var named = "";
            _drivers.Of(player).Value.Switch(client => named = "client " + client.Request.Nickname, () => named = "server");
            return named;
        }
    }
}
