using System.Collections.Generic;
using Bw.Tests.UseCases.Mortality.Doubles;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Mortality.Extensions
{
    public sealed class MortalExtensionsTests
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
        public void WhenAliveRunsAtOnceForTheLiving()
        {
            var lives = new List<Lifetime>();

            _mortal.State.WhenAlive(_lifetime.Lifetime, lives.Add);

            Assert.That(lives, Has.Count.EqualTo(1));
            Assert.That(lives[0].IsAlive, Is.True);
        }

        [Test]
        public void LifeLifetimeEndsOnDeath()
        {
            var lives = new List<Lifetime>();
            _mortal.State.WhenAlive(_lifetime.Lifetime, lives.Add);

            _mortal.Apply(VitalsOf.Dead(0f, -100f));

            Assert.That(lives, Has.Count.EqualTo(1));
            Assert.That(lives[0].IsAlive, Is.False);
        }

        [Test]
        public void LifeLifetimeEndsWithTheOuterLifetime()
        {
            var lives = new List<Lifetime>();
            var outer = _lifetime.Lifetime.CreateNested();
            _mortal.State.WhenAlive(outer.Lifetime, lives.Add);

            outer.Terminate();

            Assert.That(lives[0].IsAlive, Is.False);
        }

        [Test]
        public void WhenDeadWaitsForTheDeath()
        {
            var deaths = new List<Lifetime>();
            _mortal.State.WhenDead(_lifetime.Lifetime, deaths.Add);
            var beforeDeath = deaths.Count;

            _mortal.Apply(VitalsOf.Dead(0f, -100f));

            Assert.That(beforeDeath, Is.Zero);
            Assert.That(deaths, Has.Count.EqualTo(1));
            Assert.That(deaths[0].IsAlive, Is.True);
        }

        [Test]
        public void WhenDeadRunsAtOnceForTheDead()
        {
            _mortal.Apply(VitalsOf.Dead(0f, -100f));
            var deaths = new List<Lifetime>();

            _mortal.State.WhenDead(_lifetime.Lifetime, deaths.Add);

            Assert.That(deaths, Has.Count.EqualTo(1));
        }

        [Test]
        public void WhenDeadAndWhenAliveFollowAnyLifeStateProperty()
        {
            var state = new ViewableProperty<LifeState>(LifeState.Dead);
            var alive = 0;
            var dead = 0;
            state.WhenAlive(_lifetime.Lifetime, _ => alive++);
            state.WhenDead(_lifetime.Lifetime, _ => dead++);

            state.Value = LifeState.Alive;
            state.Value = LifeState.Dead;

            Assert.That(alive, Is.EqualTo(1));
            Assert.That(dead, Is.EqualTo(2));
        }

        [Test]
        public void LethalHitIsAKill()
        {
            var kills = 0;
            _mortal.AdviseKilled(_lifetime.Lifetime, () => kills++);

            _mortal.Apply(_mortal.Current.AfterHit(150f));

            Assert.That(kills, Is.EqualTo(1));
        }

        [Test]
        public void SurvivedHitIsNoKill()
        {
            var kills = 0;
            _mortal.AdviseKilled(_lifetime.Lifetime, () => kills++);

            _mortal.Apply(_mortal.Current.AfterHit(99f));

            Assert.That(kills, Is.Zero);
        }

        [Test]
        public void LateJoinDeathSnapshotIsNoKill()
        {
            var kills = 0;
            _mortal.AdviseKilled(_lifetime.Lifetime, () => kills++);

            _mortal.Apply(VitalsOf.Dead());

            Assert.That(kills, Is.Zero);
        }

        [Test]
        public void ServerDeathAfterTheShootersPredictedLethalHitIsAKill()
        {
            var kills = 0;
            _mortal.AdviseKilled(_lifetime.Lifetime, () => kills++);
            _mortal.Apply(VitalsOf.Alive(0f, -100f));

            _mortal.Apply(VitalsOf.Dead());

            Assert.That(kills, Is.EqualTo(1));
        }

        [Test]
        public void KillAfterAdvisingOnAPredictedLethalHitCounts()
        {
            _mortal.Apply(VitalsOf.Alive(0f, -100f));
            var kills = 0;
            _mortal.AdviseKilled(_lifetime.Lifetime, () => kills++);

            _mortal.Apply(VitalsOf.Dead());

            Assert.That(kills, Is.EqualTo(1));
        }

        [Test]
        public void TheDeadAreKilledOnce()
        {
            var kills = 0;
            _mortal.AdviseKilled(_lifetime.Lifetime, () => kills++);
            _mortal.Apply(_mortal.Current.AfterHit(100f));

            _mortal.Apply(VitalsOf.Dead(0f, -10f));
            _mortal.Apply(VitalsOf.Dead());

            Assert.That(kills, Is.EqualTo(1));
        }

        [Test]
        public void KilledStopsWithItsLifetime()
        {
            var kills = 0;
            var advised = _lifetime.Lifetime.CreateNested();
            _mortal.AdviseKilled(advised.Lifetime, () => kills++);
            advised.Terminate();

            _mortal.Apply(_mortal.Current.AfterHit(100f));

            Assert.That(kills, Is.Zero);
        }
    }
}
