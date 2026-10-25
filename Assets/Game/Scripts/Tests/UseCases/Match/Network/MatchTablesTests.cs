using System.Collections.Generic;
using System.Linq;
using Bw.Entities.Network;
using Bw.Entities.Players;
using Bw.Tests.UseCases.PropsMatchBots.Doubles;
using Bw.UseCases.Match;
using Bw.UseCases.Match.Network;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Match.Network
{
    public sealed class MatchTablesTests
    {
        private LifetimeDefinition _lifetime;
        private StubMatch _match;
        private PlayerCollection _players;
        private Scoreboard _scoreboard;
        private PlayerIds _ids;
        private CharacterKeys _characters;
        private MatchTables _tables;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _match = new StubMatch();
            _players = new PlayerCollection();
            _scoreboard = new Scoreboard(_lifetime.Lifetime, _players);
            _ids = new PlayerIds();
            _characters = new CharacterKeys();
            _tables = new MatchTables(_match, _scoreboard, _ids, _characters);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void EmptyGameMakesAnEmptyTableOfTheCurrentPeriod()
        {
            _match.Enter(new MatchPeriod(MatchPhase.Running, 640));

            var table = _tables.Create();

            Assert.That(table.Period, Is.EqualTo(new MatchPeriod(MatchPhase.Running, 640)));
            Assert.That(table.Entries, Is.Empty);
        }

        [Test]
        public void EveryPlayerOnTheBoardIsAnEntryWithItsTagCharacterAndScore()
        {
            var seva = new Player("Сева");
            var goshak = new Player("Гошак");
            Enter(seva, 7);
            Enter(goshak, 9);
            _scoreboard.Credit(goshak, new Score(2, 1, 60));

            var entries = _tables.Create().Entries.ToDictionary(entry => entry.Player.Nickname);

            Assert.That(entries.Keys, Is.EquivalentTo(new[] { "Сева", "Гошак" }));
            Assert.That(entries["Сева"].Player.Id, Is.EqualTo(1));
            Assert.That(entries["Сева"].CharacterKey, Is.EqualTo(7));
            Assert.That(entries["Гошак"].Player.Id, Is.EqualTo(2));
            Assert.That(entries["Гошак"].CharacterKey, Is.EqualTo(9));
            Assert.That(entries["Гошак"].Score.Kills, Is.EqualTo(2));
            Assert.That(entries["Гошак"].Score.Deaths, Is.EqualTo(1));
            Assert.That(entries["Гошак"].Score.Damage, Is.EqualTo(60));
        }

        [Test]
        public void PlayerWhoLeftIsNotInTheTable()
        {
            var left = new LifetimeDefinition();
            var seva = new Player("Сева");
            _ids.AddLifetimed(_lifetime.Lifetime, seva);
            _characters.Pick(seva, 7);
            _players.AddLifetimed(left.Lifetime, seva);

            left.Terminate();

            Assert.That(_tables.Create().Entries, Is.Empty);
        }

        private void Enter(IPlayer player, int characterKey)
        {
            _ids.AddLifetimed(_lifetime.Lifetime, player);
            _characters.Pick(player, characterKey);
            _players.AddLifetimed(_lifetime.Lifetime, player);
        }

        private sealed class CharacterKeys : IReadonlyPlayerCharacters
        {
            private readonly Dictionary<IPlayer, int> _keys = new();

            public void Pick(IPlayer player, int key) =>
                _keys.Add(player, key);

            public int KeyOf(IPlayer player) =>
                _keys[player];
        }
    }
}
