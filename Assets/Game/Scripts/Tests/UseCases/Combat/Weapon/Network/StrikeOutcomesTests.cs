using System.Collections.Generic;
using Bw.Entities.Extensions;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Players;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Mortality;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon.Network
{
    public sealed class StrikeOutcomesTests
    {
        private LifetimeDefinition _lifetime;
        private FakeWeapon _weapon;
        private FakeStrikes _strikes;
        private FakeCharacter _actor;
        private Player _player;
        private List<ActionOutcome<IReadonlyWeapon, IReadonlyMortal, Impacts>> _reported;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _weapon = new FakeWeapon();
            _strikes = new FakeStrikes();
            _actor = new FakeCharacter();
            _player = new Player("Гошак");
            var registry = new CharacterRegistry();
            registry.PlayerByCharacter.AddLifetimed(_lifetime.Lifetime, _actor, _player);
            var outcomes = new StrikeOutcomes(_lifetime.Lifetime, _weapon, _strikes, registry);
            _reported = new List<ActionOutcome<IReadonlyWeapon, IReadonlyMortal, Impacts>>();
            outcomes.Reported.Advise(_lifetime.Lifetime, _reported.Add);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void StrikeIsReportedAsTheWeaponsOutcomeByTheActorsPlayer()
        {
            var target = new FakeMortal();
            var strike = FakeStrikes.Of(42, _actor, target);

            _strikes.Strike(strike);

            Assert.That(_reported, Has.Count.EqualTo(1));
            Assert.That(_reported[0].Initiator, Is.SameAs(_weapon));
            Assert.That(_reported[0].Actor, Is.SameAs(_player));
            Assert.That(_reported[0].Tick, Is.EqualTo(42));
            Assert.That(_reported[0].Affected, Is.SameAs(strike.Affected));
        }

        [Test]
        public void EveryStrikeIsReportedOnce()
        {
            _strikes.Strike(FakeStrikes.Of(1, _actor, new FakeMortal()));
            _strikes.Strike(FakeStrikes.Of(2, _actor, new FakeMortal()));

            Assert.That(_reported, Has.Count.EqualTo(2));
        }

        [Test]
        public void StrikesAfterTheLifetimeAreNotReported()
        {
            _lifetime.Terminate();

            _strikes.Strike(FakeStrikes.Of(1, _actor, new FakeMortal()));

            Assert.That(_reported, Is.Empty);
        }
    }
}
