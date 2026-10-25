using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Events;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Characters;
using Bw.UseCases.Combat.Weapon.Network.Prediction;
using Bw.UseCases.Mortality;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon.Network.Prediction
{
    public sealed class StrikeDamagePredictionTests
    {
        private LifetimeDefinition _lifetime;
        private FakeStrikes _strikes;
        private RecordingPrediction _prediction;
        private ControlledCharacters _controlled;
        private FakeCharacter _actor;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _strikes = new FakeStrikes();
            _prediction = new RecordingPrediction();
            _controlled = new ControlledCharacters();
            _actor = new FakeCharacter();
            new StrikeDamagePrediction(_lifetime.Lifetime, _strikes, _prediction, _controlled);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void StrikeOfACharacterControlledHerePredictsTheDamageOfEveryTarget()
        {
            _controlled.Add(_lifetime.Lifetime, _actor);
            var first = new FakeMortal();
            var second = new FakeMortal();

            _strikes.Strike(FakeStrikes.Of(15, _actor, first, second));

            Assert.That(_prediction.Predicted, Has.Count.EqualTo(2));
            Assert.That(_prediction.Predicted[0].tick, Is.EqualTo(15));
            Assert.That(_prediction.Predicted[0].target, Is.SameAs(first));
            Assert.That(_prediction.Predicted[0].effect[0].Hit.Damage, Is.EqualTo(1f));
            Assert.That(_prediction.Predicted[1].target, Is.SameAs(second));
            Assert.That(_prediction.Predicted[1].effect[0].Hit.Damage, Is.EqualTo(2f));
        }

        [Test]
        public void StrikeOfACharacterControlledElsewherePredictsNothing()
        {
            _controlled.Add(_lifetime.Lifetime, new FakeCharacter());

            _strikes.Strike(FakeStrikes.Of(15, _actor, new FakeMortal()));

            Assert.That(_prediction.Predicted, Is.Empty);
        }

        [Test]
        public void CharacterNoLongerControlledHerePredictsNothing()
        {
            var controlled = new LifetimeDefinition();
            _controlled.Add(controlled.Lifetime, _actor);
            controlled.Terminate();

            _strikes.Strike(FakeStrikes.Of(15, _actor, new FakeMortal()));

            Assert.That(_prediction.Predicted, Is.Empty);
        }

        [Test]
        public void StrikeWithoutTargetsPredictsNothing()
        {
            _controlled.Add(_lifetime.Lifetime, _actor);

            _strikes.Strike(FakeStrikes.Of(15, _actor));

            Assert.That(_prediction.Predicted, Is.Empty);
        }

        private sealed class RecordingPrediction : IOutcomePrediction<IReadonlyMortal, Impacts>
        {
            public List<(int tick, IReadonlyMortal target, Impacts effect)> Predicted { get; } = new();

            public void Predict(int tick, IReadonlyMortal target, Impacts effect) =>
                Predicted.Add((tick, target, effect));
        }
    }
}
