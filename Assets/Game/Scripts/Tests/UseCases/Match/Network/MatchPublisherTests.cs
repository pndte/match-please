using System.Collections.Generic;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Entities.Players;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.PropsMatchBots.Doubles;
using Bw.UseCases.Match;
using Bw.UseCases.Match.Network;
using Bw.UseCases.Match.Network.Requests;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;
using Unity.Netcode;

namespace Bw.Tests.UseCases.Match.Network
{
    public sealed class MatchPublisherTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private StubMatch _match;
        private PlayerCollection _players;
        private Scoreboard _scoreboard;
        private PlayerIds _ids;
        private StubNetworkObject _matchObject;
        private MatchTableWriter _writer;
        private List<MatchTable> _written;
        private Player _player;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _match = new StubMatch();
            _players = new PlayerCollection();
            _scoreboard = new Scoreboard(_lifetime.Lifetime, _players);
            _ids = new PlayerIds();
            _matchObject = new StubNetworkObject(_lifetime.Lifetime);
            _player = new Player("Сева");
            _ids.AddLifetimed(_lifetime.Lifetime, _player);
            _players.AddLifetimed(_lifetime.Lifetime, _player);
            var schema = new NetEntriesSchemaBuilder();
            var declaration = schema.DeclareProperty(MatchTable.Empty, NetworkDelivery.ReliableFragmentedSequenced, NetworkPermissions.Everyone);
            var entries = UnspawnedNetEntries.Create(_lifetime.Lifetime, _objects.Create("Match"), schema.Build());
            _writer = new MatchTableWriter(new MatchTables(_match, _scoreboard, _ids, new SameCharacter()), entries, declaration);
            _written = new List<MatchTable>();
            entries.Get(declaration).Change.Advise(_lifetime.Lifetime, table => _written.Add(table));
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void WriterPutsANewTableIntoTheProperty()
        {
            _scoreboard.Credit(_player, new Score(1, 0, 10));

            _writer.Write();

            Assert.That(_written, Has.Count.EqualTo(1));
            Assert.That(_written[0].Entries[0].Player.Nickname, Is.EqualTo("Сева"));
            Assert.That(_written[0].Entries[0].Score.Kills, Is.EqualTo(1));
        }

        [Test]
        public void NothingIsWrittenBeforeTheSpawn()
        {
            Publish();

            _match.Enter(new MatchPeriod(MatchPhase.Ended, 50));
            _scoreboard.Credit(_player, new Score(1, 0, 0));

            Assert.That(_written, Is.Empty);
        }

        [Test]
        public void SpawnWritesTheCurrentTable()
        {
            Publish();

            _matchObject.Spawn();

            Assert.That(_written, Has.Count.EqualTo(1));
            Assert.That(_written[0].Period, Is.EqualTo(_match.Period.Value));
        }

        [Test]
        public void EveryPeriodChangeIsWritten()
        {
            Publish();
            _matchObject.Spawn();

            _match.Enter(new MatchPeriod(MatchPhase.Ended, 50));

            Assert.That(_written, Has.Count.EqualTo(2));
            Assert.That(_written[1].Period, Is.EqualTo(new MatchPeriod(MatchPhase.Ended, 50)));
        }

        [Test]
        public void EveryScoreChangeIsWritten()
        {
            Publish();
            _matchObject.Spawn();

            _scoreboard.Credit(_player, new Score(0, 0, 25));

            Assert.That(_written, Has.Count.EqualTo(2));
            Assert.That(_written[1].Entries[0].Score.Damage, Is.EqualTo(25));
        }

        [Test]
        public void NothingIsWrittenAfterTheDespawn()
        {
            Publish();
            _matchObject.Spawn();
            _matchObject.Despawn();

            _scoreboard.Credit(_player, new Score(0, 0, 25));

            Assert.That(_written, Has.Count.EqualTo(1));
        }

        private void Publish() =>
            _ = new MatchPublisher(_lifetime.Lifetime, _matchObject, _match, _scoreboard, _writer);

        private sealed class SameCharacter : IReadonlyPlayerCharacters
        {
            public int KeyOf(IPlayer player) =>
                7;
        }
    }
}
