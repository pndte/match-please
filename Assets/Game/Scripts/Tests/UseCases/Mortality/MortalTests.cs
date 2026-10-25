using System;
using System.Collections.Generic;
using Bw.Entities;
using Bw.Tests.UseCases.Mortality.Doubles;
using Bw.UseCases.Mortality;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Mortality
{
    public sealed class MortalTests
    {
        private LifetimeDefinition _lifetime;
        private MortalFixture _fixture;
        private Mortal _mortal;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _fixture = new MortalFixture(100f);
            _mortal = _fixture.Mortal;
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _fixture.Dispose();
        }

        [Test]
        public void NewMortalIsAliveAtFullHealth()
        {
            Assert.That(_mortal.State.Value, Is.EqualTo(LifeState.Alive));
            Assert.That(_mortal.Current.Health, Is.EqualTo(new HealthState(100f, 0f)));
            Assert.That(_mortal.Health.Max, Is.EqualTo(100f));
        }

        [Test]
        public void ApplySetsTheHealthAndTheLifeState()
        {
            _mortal.Apply(VitalsOf.Alive(60f, -40f));

            Assert.That(_mortal.Health.State.Value, Is.EqualTo(new HealthState(60f, -40f)));
            Assert.That(_mortal.State.Value, Is.EqualTo(LifeState.Alive));
        }

        [Test]
        public void CurrentKeepsNoChangeOfTheLastAction()
        {
            _mortal.Apply(VitalsOf.Alive(60f, -40f));

            Assert.That(_mortal.Current.Health, Is.EqualTo(new HealthState(60f, 0f)));
            Assert.That(_mortal.Current.State, Is.EqualTo(LifeState.Alive));
        }

        [Test]
        public void AppliedReportsTheVitalsAsGiven()
        {
            var applied = new List<Vitals>();
            _mortal.Applied.Advise(_lifetime.Lifetime, applied.Add);

            _mortal.Apply(VitalsOf.Dead(0f, -25f));

            Assert.That(applied, Has.Count.EqualTo(1));
            Assert.That(applied[0].Health, Is.EqualTo(new HealthState(0f, -25f)));
            Assert.That(applied[0].State, Is.EqualTo(LifeState.Dead));
        }

        [Test]
        public void AppliedFiresOnceHealthAndStateAreSet()
        {
            var seen = new List<(float Health, LifeState State)>();
            _mortal.Applied.Advise(_lifetime.Lifetime, _ => seen.Add((_mortal.Health.State.Value.Current, _mortal.State.Value)));

            _mortal.Apply(VitalsOf.Dead(0f, -100f));

            Assert.That(seen, Is.EqualTo(new[] { (0f, LifeState.Dead) }));
        }

        [Test]
        public void LifeStateChangesBeforeAppliedFires()
        {
            var order = new List<string>();
            _mortal.State.Advise(_lifetime.Lifetime, state => order.Add(state.ToString()));
            _mortal.Applied.Advise(_lifetime.Lifetime, _ => order.Add("applied"));

            _mortal.Apply(VitalsOf.Dead(0f, -100f));

            Assert.That(order, Is.EqualTo(new[] { "Alive", "Dead", "applied" }));
        }

        [Test]
        public void ActionChangeIsAnnouncedByTheHealth()
        {
            var changes = new List<float>();
            _mortal.Health.Changed.Advise(_lifetime.Lifetime, changes.Add);

            _mortal.Apply(VitalsOf.Alive(70f, -30f));
            _mortal.Apply(VitalsOf.Alive(80f, 10f));

            Assert.That(changes, Is.EqualTo(new[] { -30f, 10f }));
        }

        [Test]
        public void SnapshotWithoutChangeIsAppliedSilently()
        {
            var changes = new List<float>();
            _mortal.Health.Changed.Advise(_lifetime.Lifetime, changes.Add);

            _mortal.Apply(VitalsOf.Alive(40f));

            Assert.That(changes, Is.Empty);
            Assert.That(_mortal.Current.Health.Current, Is.EqualTo(40f));
        }

        [Test]
        public void DeadMortalCannotComeBackToLife()
        {
            _mortal.Apply(VitalsOf.Dead(0f, -100f));

            Assert.Throws<InvalidOperationException>(() => _mortal.Apply(VitalsOf.Alive(100f)));
        }

        [Test]
        public void RefusedRevivalChangesNothing()
        {
            _mortal.Apply(VitalsOf.Dead(0f, -100f));
            var applied = 0;
            _mortal.Applied.Advise(_lifetime.Lifetime, _ => applied++);

            Assert.Throws<InvalidOperationException>(() => _mortal.Apply(VitalsOf.Alive(100f, 100f)));

            Assert.That(_mortal.State.Value, Is.EqualTo(LifeState.Dead));
            Assert.That(_mortal.Current.Health.Current, Is.EqualTo(0f));
            Assert.That(applied, Is.Zero);
        }

        [Test]
        public void DeadMortalTakesAnotherDeadState()
        {
            _mortal.Apply(VitalsOf.Dead(0f, -100f));

            _mortal.Apply(VitalsOf.Dead());

            Assert.That(_mortal.State.Value, Is.EqualTo(LifeState.Dead));
        }

        [TestCase(-1f)]
        [TestCase(100.5f)]
        public void HealthOutsideZeroToMaxThrows(float health)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _mortal.Apply(VitalsOf.Alive(health)));
        }

        [Test]
        public void RefusedHealthChangesNothing()
        {
            var applied = 0;
            _mortal.Applied.Advise(_lifetime.Lifetime, _ => applied++);

            Assert.Throws<ArgumentOutOfRangeException>(() => _mortal.Apply(VitalsOf.Dead(-5f, -105f)));

            Assert.That(_mortal.State.Value, Is.EqualTo(LifeState.Alive));
            Assert.That(_mortal.Current.Health.Current, Is.EqualTo(100f));
            Assert.That(applied, Is.Zero);
        }

        [TestCase(0f)]
        [TestCase(100f)]
        public void BoundsOfTheHealthAreAllowed(float health)
        {
            _mortal.Apply(VitalsOf.Alive(health));

            Assert.That(_mortal.Current.Health.Current, Is.EqualTo(health));
        }
    }
}
