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
    public sealed class ScoreKeeperTests
    {
        private LifetimeDefinition _lifetime;
        private ManualClock _clock;
        private StubMatch _match;
        private PlayerCollection _players;
        private Scoreboard _scoreboard;
        private CharacterRegistry _registry;
        private ScoreKeeper _keeper;
        private List<Kill> _kills;
        private Player _shooter;
        private Player _victim;
        private StubCharacter _shooterCharacter;
        private StubCharacter _victimCharacter;
        private StubWeapon _gun;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _clock = new ManualClock();
            _match = new StubMatch();
            _players = new PlayerCollection();
            _scoreboard = new Scoreboard(_lifetime.Lifetime, _players);
            _registry = new CharacterRegistry();
            _keeper = new ScoreKeeper(_clock, _match, _scoreboard, new Combatants(_registry));
            _kills = new List<Kill>();
            _keeper.Scored.Advise(_lifetime.Lifetime, kill => _kills.Add(kill));
            _shooter = new Player("Сева");
            _victim = new Player("Гошак");
            _shooterCharacter = new StubCharacter();
            _victimCharacter = new StubCharacter();
            _gun = new StubWeapon();
            Enter(_shooter, _shooterCharacter);
            Enter(_victim, _victimCharacter);
            _clock.Tick(100);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void HitCreditsItsRoundedDamage()
        {
            _keeper.CountHit(_shooter, _gun, _victimCharacter.Mortal, 12.6f);

            Assert.That(_scoreboard.Scores[_shooter].Damage, Is.EqualTo(13));
            Assert.That(_scoreboard.Scores[_shooter].Kills, Is.Zero);
        }

        [Test]
        public void DeathCreditsTheVictimADeath()
        {
            _keeper.CountDeath(_victimCharacter);

            Assert.That(_scoreboard.Scores[_victim].Deaths, Is.EqualTo(1));
        }

        [Test]
        public void HitThenDeathInTheSameTickIsAKill()
        {
            _keeper.CountHit(_shooter, _gun, _victimCharacter.Mortal, 30f);
            _keeper.CountDeath(_victimCharacter);

            Assert.That(_scoreboard.Scores[_shooter].Kills, Is.EqualTo(1));
            Assert.That(_kills, Has.Count.EqualTo(1));
            Assert.That(_kills[0].Killer, Is.SameAs(_shooter));
            Assert.That(_kills[0].Victim, Is.SameAs(_victim));
            Assert.That(_kills[0].Weapon, Is.SameAs(_gun));
        }

        [Test]
        public void DeathThenHitInTheSameTickIsAKill()
        {
            _keeper.CountDeath(_victimCharacter);
            _keeper.CountHit(_shooter, _gun, _victimCharacter.Mortal, 30f);

            Assert.That(_scoreboard.Scores[_shooter].Kills, Is.EqualTo(1));
            Assert.That(_scoreboard.Scores[_victim].Deaths, Is.EqualTo(1));
            Assert.That(_kills, Has.Count.EqualTo(1));
            Assert.That(_kills[0].Victim, Is.SameAs(_victim));
        }

        [Test]
        public void HitInAnEarlierTickIsNoKill()
        {
            _keeper.CountHit(_shooter, _gun, _victimCharacter.Mortal, 30f);
            _clock.Advance();

            _keeper.CountDeath(_victimCharacter);

            Assert.That(_scoreboard.Scores[_shooter].Kills, Is.Zero);
            Assert.That(_kills, Is.Empty);
        }

        [Test]
        public void HitAfterAnEarlierTicksDeathIsNoKill()
        {
            _keeper.CountDeath(_victimCharacter);
            _clock.Advance();

            _keeper.CountHit(_shooter, _gun, _victimCharacter.Mortal, 30f);

            Assert.That(_kills, Is.Empty);
        }

        [Test]
        public void DeathIsClaimedOnlyOnce()
        {
            _keeper.CountDeath(_victimCharacter);
            _keeper.CountHit(_shooter, _gun, _victimCharacter.Mortal, 30f);

            _keeper.CountHit(_shooter, _gun, _victimCharacter.Mortal, 30f);

            Assert.That(_scoreboard.Scores[_shooter].Kills, Is.EqualTo(1));
            Assert.That(_kills, Has.Count.EqualTo(1));
        }

        [Test]
        public void LastBlowOfTheTickTakesTheKill()
        {
            var other = new Player("Матвей");
            var knife = new StubWeapon();
            Enter(other, new StubCharacter());
            _keeper.CountHit(_shooter, _gun, _victimCharacter.Mortal, 30f);
            _keeper.CountHit(other, knife, _victimCharacter.Mortal, 10f);

            _keeper.CountDeath(_victimCharacter);

            Assert.That(_kills, Has.Count.EqualTo(1));
            Assert.That(_kills[0].Killer, Is.SameAs(other));
            Assert.That(_kills[0].Weapon, Is.SameAs(knife));
        }

        [TestCase(MatchPhase.Starting)]
        [TestCase(MatchPhase.Ended)]
        public void NothingCountsOutsideARunningMatch(MatchPhase phase)
        {
            _match.Enter(new MatchPeriod(phase, 200));

            _keeper.CountHit(_shooter, _gun, _victimCharacter.Mortal, 30f);
            _keeper.CountDeath(_victimCharacter);

            Assert.That(_scoreboard.Scores[_shooter].Damage, Is.Zero);
            Assert.That(_scoreboard.Scores[_victim].Deaths, Is.Zero);
            Assert.That(_kills, Is.Empty);
        }

        [Test]
        public void AttackerWhoLeftGetsNothing()
        {
            var gone = new Player("Ушедший");

            _keeper.CountHit(gone, _gun, _victimCharacter.Mortal, 30f);
            _keeper.CountDeath(_victimCharacter);

            Assert.That(_kills, Is.Empty);
            Assert.That(_scoreboard.Scores[_victim].Deaths, Is.EqualTo(1));
        }

        [Test]
        public void DeathOfACharacterWithoutAPlayerIsNotCounted()
        {
            var stranger = new StubCharacter();
            _keeper.CountHit(_shooter, _gun, stranger.Mortal, 30f);

            _keeper.CountDeath(stranger);

            Assert.That(_scoreboard.Scores[_shooter].Kills, Is.Zero);
            Assert.That(_scoreboard.Scores[_shooter].Damage, Is.EqualTo(30));
            Assert.That(_kills, Is.Empty);
        }

        [Test]
        public void ForgottenCharacterLosesItsLastBlow()
        {
            _keeper.CountHit(_shooter, _gun, _victimCharacter.Mortal, 30f);

            _keeper.Forget(_victimCharacter);
            _keeper.CountDeath(_victimCharacter);

            Assert.That(_kills, Is.Empty);
        }

        [Test]
        public void ForgottenCharacterLosesItsUnclaimedDeath()
        {
            _keeper.CountDeath(_victimCharacter);

            _keeper.Forget(_victimCharacter);
            _keeper.CountHit(_shooter, _gun, _victimCharacter.Mortal, 30f);

            Assert.That(_kills, Is.Empty);
        }

        private void Enter(IPlayer player, StubCharacter character)
        {
            _players.AddLifetimed(_lifetime.Lifetime, player);
            _registry.PlayerByCharacter.Add(character, player);
        }
    }
}
