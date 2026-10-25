using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Bubble;
using Bw.Tests.UseCases.Character.Doubles;
using Bw.Tests.UseCases.Mortality.Doubles;
using Bw.UseCases.Character.Network;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Character.Network
{
    public sealed class CharacterPredictedByTests
    {
        private LifetimeDefinition _lifetime;
        private MortalFixture _fixture;
        private FakeControlledBy _controlledBy;
        private Membership _membership;
        private CharacterPredictedBy _predictedBy;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _fixture = new MortalFixture();
            _controlledBy = new FakeControlledBy();
            _membership = new Membership();
            _predictedBy = new CharacterPredictedBy(_lifetime.Lifetime, _controlledBy, _membership, _fixture.Mortal);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _fixture.Dispose();
        }

        [Test]
        public void CharacterNeitherControlledHereNorInTheBubbleIsNotPredicted()
        {
            Assert.That(_predictedBy.Me.Value, Is.False);
        }

        [Test]
        public void CharacterControlledHereIsPredicted()
        {
            _controlledBy.Set(true);

            Assert.That(_predictedBy.Me.Value, Is.True);
        }

        [Test]
        public void CharacterInTheBubbleIsPredicted()
        {
            _membership.Set(true);

            Assert.That(_predictedBy.Me.Value, Is.True);
        }

        [Test]
        public void DeadCharacterIsNeverPredicted()
        {
            _controlledBy.Set(true);
            _membership.Set(true);

            _fixture.Mortal.Apply(VitalsOf.Dead(0f, -100f));

            Assert.That(_predictedBy.Me.Value, Is.False);
        }

        [Test]
        public void PredictionFollowsControlAndBubble()
        {
            var seen = new List<bool>();
            _predictedBy.Me.Advise(_lifetime.Lifetime, seen.Add);

            _membership.Set(true);
            _controlledBy.Set(true);
            _membership.Set(false);
            _controlledBy.Set(false);

            Assert.That(seen, Is.EqualTo(new[] { false, true, false }));
        }

        [Test]
        public void PredictionStopsFollowingWithItsLifetime()
        {
            _lifetime.Terminate();

            _controlledBy.Set(true);

            Assert.That(_predictedBy.Me.Value, Is.False);
        }

        private sealed class Membership : IBubbleMembership
        {
            public IReadonlyProperty<bool> Inside => _inside;

            private readonly ViewableProperty<bool> _inside = new(false);

            public void Set(bool inside) =>
                _inside.Value = inside;
        }
    }
}
