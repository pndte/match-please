using System;
using System.Linq;
using Bw.Entities.Network;
using Bw.Entities.Players;
using Bw.Tests.Entities.Network.Doubles;
using Bw.UseCases.Players.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Players.Network
{
    public sealed class ConnectedClientAdmitterTests
    {
        private static readonly Guid Token = Guid.NewGuid();

        private LifetimeDefinition _network;
        private LifetimeDefinition _session;
        private ClientsById _clients;
        private PlayerDrivers _drivers;
        private PlayerCollection _players;
        private DepartedPlayers _departed;

        [SetUp]
        public void SetUp()
        {
            _network = new LifetimeDefinition();
            _session = new LifetimeDefinition();
            _clients = new ClientsById(_network.Lifetime);
            _drivers = new PlayerDrivers();
            _players = new PlayerCollection();
            _departed = new DepartedPlayers();
            var admitter = new PlayerAdmitter(new PlayerIds(), _drivers, new PlayerCharacters(), _players);
            new ConnectedClientAdmitter(_session.Lifetime, _clients, admitter, _drivers, _departed);
        }

        [TearDown]
        public void TearDown()
        {
            _session.Terminate();
            _network.Terminate();
        }

        [Test]
        public void ConnectedClientDrivesANewPlayer()
        {
            var client = _clients.Connect(1, Token);

            Assert.That(_players.Single().Nickname, Is.EqualTo("Саня"));
            Assert.That(_drivers.PlayerOf(client), Is.SameAs(_players.Single()));
        }

        [Test]
        public void PlayerOfALeftClientStaysDrivenByTheServerAndDeparted()
        {
            _clients.Connect(1, Token);
            var player = _players.Single();

            _clients.ByIds.Remove(1);

            Assert.That(_players.Single(), Is.SameAs(player));
            Assert.That(_drivers.Of(player).Value, Is.SameAs(PlayerDriver.Server));
            Assert.That(_departed.PlayerOf(Token), Is.SameAs(player));
        }

        [Test]
        public void ClientWithTheTokenOfADepartedPlayerReturnsToIt()
        {
            _clients.Connect(1, Token);
            var player = _players.Single();
            _clients.ByIds.Remove(1);

            var returned = _clients.Connect(2, Token);

            Assert.That(_players.Count, Is.EqualTo(1));
            Assert.That(_drivers.PlayerOf(returned), Is.SameAs(player));
            Assert.That(_departed.Contains(Token), Is.False);
        }

        [Test]
        public void ClientWithAnotherTokenJoinsAsANewPlayer()
        {
            _clients.Connect(1, Token);
            _clients.ByIds.Remove(1);

            var stranger = _clients.Connect(2, Guid.NewGuid(), "Сева");

            Assert.That(_players.Count, Is.EqualTo(2));
            Assert.That(_drivers.PlayerOf(stranger).Nickname, Is.EqualTo("Сева"));
            Assert.That(_departed.Contains(Token), Is.True);
        }

        [Test]
        public void ClientOfAConnectedPlayersTokenJoinsAsANewPlayer()
        {
            _clients.Connect(1, Token);

            _clients.Connect(2, Token, "Сева");

            Assert.That(_players.Count, Is.EqualTo(2));
        }

        [Test]
        public void EndOfTheSessionDepartsNobody()
        {
            _clients.Connect(1, Token);

            _session.Terminate();
            _network.Terminate();

            Assert.That(_departed.Contains(Token), Is.False);
        }
    }
}
