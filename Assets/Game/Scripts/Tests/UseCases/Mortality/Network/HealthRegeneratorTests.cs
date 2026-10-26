using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Rollback;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.Mortality.Doubles;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Mortality.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Mortality.Network
{
    public sealed class HealthRegeneratorTests
    {
        private const int DelayTicks = 4;
        private const int IntervalTicks = 2;

        private LifetimeDefinition _lifetime;
        private MortalFixture _fixture;
        private RegenerationConfig _config;
        private ManualClock _clock;
        private Heals _heals;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _fixture = new MortalFixture(100f);
            _config = ScriptableObject.CreateInstance<RegenerationConfig>();
            _config.Delay = DelayTicks * 0.25f;
            _config.Interval = IntervalTicks * 0.25f;
            _config.Amount = 10f;
            _clock = new ManualClock(0.25f);
            _heals = new Heals(_fixture.Mortal, _clock);
            var worldStep = new WorldStep(_lifetime.Lifetime, _clock);
            _ = new HealthRegenerator(_lifetime.Lifetime, _fixture.Mortal, _heals, _clock, worldStep, _config);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _fixture.Dispose();
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void HealthyMortalIsNeverHealed()
        {
            _clock.Advance(5 * DelayTicks);

            Assert.That(_heals.Ticks, Is.Empty);
        }

        [Test]
        public void HealingStartsTheDelayAfterTheLastDamage()
        {
            _clock.Tick(10);
            Hit(30f);

            _clock.Advance(DelayTicks);

            Assert.That(_heals.Ticks, Is.EqualTo(new[] { 10 + DelayTicks }));
            Assert.That(_fixture.Mortal.Current.Health.Current, Is.EqualTo(80f));
        }

        [Test]
        public void HealsComeOncePerIntervalUntilFull()
        {
            _clock.Tick(10);
            Hit(30f);

            _clock.Advance(DelayTicks + 4 * IntervalTicks);

            Assert.That(_heals.Ticks, Is.EqualTo(new[] { 14, 16, 18 }));
            Assert.That(_fixture.Mortal.Current.Health.Current, Is.EqualTo(100f));
        }

        [Test]
        public void EveryHealIsTheConfiguredAmountWithoutACause()
        {
            _clock.Tick(10);
            Hit(30f);

            _clock.Advance(DelayTicks + IntervalTicks);

            Assert.That(_heals.Amounts, Is.EqualTo(new[] { 10f, 10f }));
            Assert.That(_heals.CausedByAction, Is.EqualTo(new[] { false, false }));
        }

        [Test]
        public void NewDamageRestartsTheDelay()
        {
            _clock.Tick(10);
            Hit(30f);
            _clock.Advance(DelayTicks);
            Hit(20f);

            _clock.Advance(DelayTicks);

            Assert.That(_heals.Ticks, Is.EqualTo(new[] { 14, 18 }));
        }

        [Test]
        public void SnapshotWithoutChangeIsNoDamage()
        {
            _clock.Tick(10);
            Hit(30f);
            _clock.Advance(DelayTicks - 1);
            _fixture.Mortal.Apply(_fixture.Mortal.Current);

            _clock.Advance(1);

            Assert.That(_heals.Ticks, Is.EqualTo(new[] { 14 }));
        }

        [Test]
        public void TheDeadAreNeverHealed()
        {
            _clock.Tick(10);
            Hit(150f);

            _clock.Advance(3 * DelayTicks);

            Assert.That(_heals.Ticks, Is.Empty);
        }

        [Test]
        public void HealingStopsWithItsLifetime()
        {
            _clock.Tick(10);
            Hit(30f);
            _lifetime.Terminate();

            _clock.Advance(3 * DelayTicks);

            Assert.That(_heals.Ticks, Is.Empty);
        }

        private void Hit(float damage) =>
            _fixture.Mortal.Apply(_fixture.Mortal.Current.AfterHit(damage));

        private sealed class Heals : IAffectable<Heal>
        {
            public List<int> Ticks { get; } = new();
            public List<float> Amounts { get; } = new();
            public List<bool> CausedByAction { get; } = new();

            private readonly Mortal _mortal;
            private readonly ManualClock _clock;

            public Heals(Mortal mortal, ManualClock clock)
            {
                _mortal = mortal;
                _clock = clock;
            }

            public void Affect(StateCause cause, Heal effect)
            {
                Ticks.Add(_clock.Current);
                Amounts.Add(effect.Amount);
                cause.Switch(CausedByAction, static caused => caused.Add(false), static (caused, _) => caused.Add(true));
                _mortal.Apply(_mortal.Current.AfterHeal(effect.Amount, _mortal.Health.Max));
            }
        }
    }
}
