using Bw.Entities.Network;
using Bw.Entities.Players;
using Bw.Tests.UseCases.PropsMatchBots.Doubles;
using Bw.UseCases.Match.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Match.Network
{
    public sealed class LocalPlayerPublisherTests
    {
        private LifetimeDefinition _lifetime;
        private StubNetworkObject _match;
        private PlayerDrivers _drivers;
        private PlayerIds _ids;
        private RecordingResults<int> _sender;
        private Client _first;
        private Client _second;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _match = new StubNetworkObject(_lifetime.Lifetime);
            _drivers = new PlayerDrivers();
            _ids = new PlayerIds();
            _sender = new RecordingResults<int>();
            _first = new Client(10, default);
            _second = new Client(20, default);
            _ = new LocalPlayerPublisher(_lifetime.Lifetime, _match, _drivers, _ids, _sender);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void NobodyIsToldBeforeTheMatchSpawns()
        {
            Admit(new Player("Сева"), _first);

            Assert.That(_sender.Sent, Is.Empty);
        }

        [Test]
        public void SpawnTellsEveryConnectedClientItsOwnPlayer()
        {
            Admit(new Player("Сева"), _first);
            Admit(new Player("Гошак"), _second);

            _match.Spawn();

            Assert.That(_sender.Sent, Is.EquivalentTo(new[] { ((IClient)_first, 1), (_second, 2) }));
            Assert.That(_sender.Broadcasts, Is.Empty);
        }

        [Test]
        public void ClientConnectingLaterIsToldOnArrival()
        {
            _match.Spawn();

            Admit(new Player("Сева"), _first);

            Assert.That(_sender.Sent, Is.EqualTo(new[] { ((IClient)_first, 1) }));
        }

        [Test]
        public void BotsAreNeverTold()
        {
            var bot = new Player("Саня");
            _ids.AddLifetimed(_lifetime.Lifetime, bot);
            _drivers.AddServerDriven(_lifetime.Lifetime, bot);

            _match.Spawn();

            Assert.That(_sender.Sent, Is.Empty);
        }

        [Test]
        public void NobodyIsToldAfterTheMatchDespawns()
        {
            _match.Spawn();
            _match.Despawn();

            Admit(new Player("Сева"), _first);

            Assert.That(_sender.Sent, Is.Empty);
        }

        [Test]
        public void EverySpawnTellsAgain()
        {
            Admit(new Player("Сева"), _first);
            _match.Spawn();
            _match.Despawn();

            _match.Spawn();

            Assert.That(_sender.Sent, Has.Count.EqualTo(2));
        }

        private void Admit(IPlayer player, IClient client)
        {
            _ids.AddLifetimed(_lifetime.Lifetime, player);
            _drivers.AddClientDriven(_lifetime.Lifetime, player, client);
        }
    }
}
