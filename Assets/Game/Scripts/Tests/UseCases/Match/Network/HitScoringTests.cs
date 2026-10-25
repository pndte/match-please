using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Players;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.PropsMatchBots.Doubles;
using Bw.UseCases;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Match;
using Bw.UseCases.Match.Network;
using Bw.UseCases.Mortality;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Match.Network
{
    public sealed class HitScoringTests
    {
        private LifetimeDefinition _lifetime;
        private ManualOutcomes<IReadonlyWeapon, IReadonlyMortal, Impacts> _outcomes;
        private PlayerCollection _players;
        private Scoreboard _scoreboard;
        private CharacterRegistry _registry;
        private ScoreKeeper _keeper;
        private Player _shooter;
        private Player _victim;
        private StubCharacter _shooterCharacter;
        private StubCharacter _victimCharacter;
        private StubWeapon _gun;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _outcomes = new ManualOutcomes<IReadonlyWeapon, IReadonlyMortal, Impacts>();
            _players = new PlayerCollection();
            _scoreboard = new Scoreboard(_lifetime.Lifetime, _players);
            _registry = new CharacterRegistry();
            var combatants = new Combatants(_registry);
            _keeper = new ScoreKeeper(new ManualClock(), new StubMatch(), _scoreboard, combatants);
            _ = new HitScoring(_lifetime.Lifetime, _outcomes, combatants, _keeper);
            _shooter = new Player("Сева");
            _victim = new Player("Гошак");
            _shooterCharacter = new StubCharacter();
            _victimCharacter = new StubCharacter();
            _gun = new StubWeapon();
            Enter(_shooter, _shooterCharacter);
            Enter(_victim, _victimCharacter);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void HitOnAnotherPlayerCountsTheDamageOfEveryImpact()
        {
            _outcomes.Report(Outcome(_shooter, Affected(_victimCharacter.Mortal, 10f, 7f)));

            Assert.That(_scoreboard.Scores[_shooter].Damage, Is.EqualTo(17));
        }

        [Test]
        public void EveryAffectedPlayerIsCounted()
        {
            var third = new StubCharacter();
            Enter(new Player("Матвей"), third);

            _outcomes.Report(Outcome(_shooter, Affected(_victimCharacter.Mortal, 10f), Affected(third.Mortal, 5f)));

            Assert.That(_scoreboard.Scores[_shooter].Damage, Is.EqualTo(15));
        }

        [Test]
        public void HitOnOnesOwnCharacterIsNotCounted()
        {
            _outcomes.Report(Outcome(_shooter, Affected(_shooterCharacter.Mortal, 10f)));

            Assert.That(_scoreboard.Scores[_shooter].Damage, Is.Zero);
        }

        [Test]
        public void HitOnACrateIsNotCounted()
        {
            _outcomes.Report(Outcome(_shooter, Affected(new StubMortal(), 10f)));

            Assert.That(_scoreboard.Scores[_shooter].Damage, Is.Zero);
        }

        [Test]
        public void KillingHitIsCreditedToTheActorWithTheInitiatingWeapon()
        {
            var kills = new List<Kill>();
            _keeper.Scored.Advise(_lifetime.Lifetime, kill => kills.Add(kill));

            _outcomes.Report(Outcome(_shooter, Affected(_victimCharacter.Mortal, 100f)));
            _keeper.CountDeath(_victimCharacter);

            Assert.That(kills, Has.Count.EqualTo(1));
            Assert.That(kills[0].Killer, Is.SameAs(_shooter));
            Assert.That(kills[0].Weapon, Is.SameAs(_gun));
        }

        private ActionOutcome<IReadonlyWeapon, IReadonlyMortal, Impacts> Outcome(
            IPlayer actor,
            params AffectedTarget<IReadonlyMortal, Impacts>[] affected) =>
            new(_gun, actor, 0, affected);

        private static AffectedTarget<IReadonlyMortal, Impacts> Affected(IReadonlyMortal target, params float[] damages)
        {
            var impacts = new Impact[damages.Length];
            for (var index = 0; index < damages.Length; index++)
                impacts[index] = new Impact(new Hit(damages[index], Vector2.right), default);

            return new AffectedTarget<IReadonlyMortal, Impacts>(target, new Impacts(impacts));
        }

        private void Enter(IPlayer player, StubCharacter character)
        {
            _players.AddLifetimed(_lifetime.Lifetime, player);
            _registry.PlayerByCharacter.Add(character, player);
        }
    }
}
