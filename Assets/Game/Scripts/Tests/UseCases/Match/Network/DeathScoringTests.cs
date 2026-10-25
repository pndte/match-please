using System.Collections.Generic;
using Bw.Entities.Players;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.PropsMatchBots.Doubles;
using Bw.UseCases;
using Bw.UseCases.Match;
using Bw.UseCases.Match.Network;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Match.Network
{
    public sealed class DeathScoringTests
    {
        private LifetimeDefinition _lifetime;
        private PlayerCollection _players;
        private Scoreboard _scoreboard;
        private CharacterRegistry _registry;
        private ScoreKeeper _keeper;
        private List<Kill> _kills;
        private Player _shooter;
        private Player _victim;
        private StubCharacter _victimCharacter;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _players = new PlayerCollection();
            _scoreboard = new Scoreboard(_lifetime.Lifetime, _players);
            _registry = new CharacterRegistry();
            _keeper = new ScoreKeeper(new ManualClock(), new StubMatch(), _scoreboard, new Combatants(_registry));
            _ = new DeathScoring(_lifetime.Lifetime, _registry, _keeper);
            _kills = new List<Kill>();
            _keeper.Scored.Advise(_lifetime.Lifetime, kill => _kills.Add(kill));
            _shooter = new Player("Сева");
            _victim = new Player("Гошак");
            _victimCharacter = new StubCharacter();
            _players.AddLifetimed(_lifetime.Lifetime, _shooter);
            _players.AddLifetimed(_lifetime.Lifetime, _victim);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void DeathOfARegisteredCharacterIsCounted()
        {
            _registry.PlayerByCharacter.Add(_victimCharacter, _victim);

            _victimCharacter.Mortal.Die();

            Assert.That(_scoreboard.Scores[_victim].Deaths, Is.EqualTo(1));
        }

        [Test]
        public void DeathAfterABlowInTheSameTickIsAKill()
        {
            _registry.PlayerByCharacter.Add(_victimCharacter, _victim);
            _keeper.CountHit(_shooter, new StubWeapon(), _victimCharacter.Mortal, 100f);

            _victimCharacter.Mortal.Die();

            Assert.That(_kills, Has.Count.EqualTo(1));
            Assert.That(_kills[0].Victim, Is.SameAs(_victim));
        }

        [Test]
        public void UnregisteredCharacterIsForgotten()
        {
            _registry.PlayerByCharacter.Add(_victimCharacter, _victim);
            _keeper.CountHit(_shooter, new StubWeapon(), _victimCharacter.Mortal, 100f);

            _registry.PlayerByCharacter.Remove(_victimCharacter);
            _registry.PlayerByCharacter.Add(_victimCharacter, _victim);
            _victimCharacter.Mortal.Die();

            Assert.That(_kills, Is.Empty);
            Assert.That(_scoreboard.Scores[_victim].Deaths, Is.EqualTo(1));
        }

        [Test]
        public void DeathAfterTheCharacterLeftIsNotCounted()
        {
            _registry.PlayerByCharacter.Add(_victimCharacter, _victim);
            _registry.PlayerByCharacter.Remove(_victimCharacter);

            _victimCharacter.Mortal.Die();

            Assert.That(_scoreboard.Scores[_victim].Deaths, Is.Zero);
        }
    }
}
