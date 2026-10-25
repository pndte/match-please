using System;
using System.Collections.Generic;
using Bw.Entities.Players;
using Bw.UseCases.Match;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Match
{
    public sealed class ScoreboardTests
    {
        private LifetimeDefinition _lifetime;
        private PlayerCollection _players;
        private Scoreboard _scoreboard;
        private List<IPlayer> _changed;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _players = new PlayerCollection();
            _scoreboard = new Scoreboard(_lifetime.Lifetime, _players);
            _changed = new List<IPlayer>();
            _scoreboard.ScoreChanged.Advise(_lifetime.Lifetime, player => _changed.Add(player));
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void JoiningPlayerStartsFromZero()
        {
            var player = new Player("Сева");

            _players.AddLifetimed(_lifetime.Lifetime, player);

            Assert.That(_scoreboard.Scores.ContainsKey(player), Is.True);
            Assert.That(_scoreboard.Scores[player].Kills, Is.Zero);
            Assert.That(_changed, Is.EqualTo(new[] { player }));
        }

        [Test]
        public void PlayersAlreadyInTheGameAreOnTheBoard()
        {
            var player = new Player("Сева");
            var players = new PlayerCollection();
            players.AddLifetimed(_lifetime.Lifetime, player);

            var scoreboard = new Scoreboard(_lifetime.Lifetime, players);

            Assert.That(scoreboard.Scores.ContainsKey(player), Is.True);
        }

        [Test]
        public void CreditAddsToTheScoreAndAnnouncesThePlayer()
        {
            var player = new Player("Сева");
            Join(player);
            _changed.Clear();

            _scoreboard.Credit(player, new Score(1, 0, 20));
            _scoreboard.Credit(player, new Score(0, 1, 5));

            Assert.That(_scoreboard.Scores[player].Kills, Is.EqualTo(1));
            Assert.That(_scoreboard.Scores[player].Deaths, Is.EqualTo(1));
            Assert.That(_scoreboard.Scores[player].Damage, Is.EqualTo(25));
            Assert.That(_changed, Is.EqualTo(new[] { player, player }));
        }

        [Test]
        public void CreditToAPlayerOutsideTheGameThrows()
        {
            Assert.Throws<InvalidOperationException>(() => _scoreboard.Credit(new Player("Чужой"), new Score(1, 0, 0)));
        }

        [Test]
        public void LeavingPlayerLeavesTheBoard()
        {
            var playerLifetime = new LifetimeDefinition();
            var player = new Player("Сева");
            _players.AddLifetimed(playerLifetime.Lifetime, player);
            _changed.Clear();

            playerLifetime.Terminate();

            Assert.That(_scoreboard.Scores.ContainsKey(player), Is.False);
            Assert.That(_changed, Is.EqualTo(new[] { player }));
        }

        [Test]
        public void PlayerWhoLeftCannotBeCredited()
        {
            var playerLifetime = new LifetimeDefinition();
            var player = new Player("Сева");
            _players.AddLifetimed(playerLifetime.Lifetime, player);
            playerLifetime.Terminate();

            Assert.Throws<InvalidOperationException>(() => _scoreboard.Credit(player, new Score(1, 0, 0)));
        }

        [Test]
        public void ResetZeroesEveryScoreAndAnnouncesEachPlayer()
        {
            var first = new Player("Сева");
            var second = new Player("Гошак");
            Join(first);
            Join(second);
            _scoreboard.Credit(first, new Score(3, 1, 90));
            _scoreboard.Credit(second, new Score(1, 2, 30));
            _changed.Clear();

            _scoreboard.Reset();

            Assert.That(_scoreboard.Scores[first].Kills, Is.Zero);
            Assert.That(_scoreboard.Scores[first].Damage, Is.Zero);
            Assert.That(_scoreboard.Scores[second].Deaths, Is.Zero);
            Assert.That(_changed, Is.EquivalentTo(new[] { first, second }));
        }

        [Test]
        public void ResetKeepsThePlayers()
        {
            var player = new Player("Сева");
            Join(player);

            _scoreboard.Reset();

            Assert.That(_scoreboard.Scores.Keys, Is.EqualTo(new[] { player }));
        }

        private void Join(IPlayer player) =>
            _players.AddLifetimed(_lifetime.Lifetime, player);
    }
}
