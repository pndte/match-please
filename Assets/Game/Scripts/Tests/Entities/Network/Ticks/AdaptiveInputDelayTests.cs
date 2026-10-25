using System.Collections.Generic;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Tests.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Ticks
{
    public sealed class AdaptiveInputDelayTests
    {
        private const float RaisingLead = 5f;
        private const float SteadyLead = 4f;
        private const float LoweringLead = 2f;
        private const int HoldTicks = 4;

        private LifetimeDefinition _lifetime;
        private ManualClock _clock;
        private WorldStep _worldStep;
        private InputLead _lead;
        private NetworkTicksConfig _config;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _clock = new ManualClock(0.25f);
            _worldStep = new WorldStep(_lifetime.Lifetime, _clock);
            _lead = new InputLead();
            _config = new NetworkTicksConfig
            {
                FreeRoundTripSeconds = 0.5f,
                MinInputDelayTicks = 1,
                MaxInputDelayTicks = 4,
                InputDelayHysteresisTicks = 0.25f,
                InputDelayHoldSeconds = 1f
            };
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void DelayStartsAtTheMinimum()
        {
            _config.MinInputDelayTicks = 2;

            var delay = Start();

            Assert.That(delay.Ticks.Value, Is.EqualTo(2));
        }

        [Test]
        public void LongRoundTripRaisesTheDelayOnlyAfterTheHold()
        {
            var delay = Start();
            _lead.Ticks = RaisingLead;

            _clock.Advance(HoldTicks - 1);

            Assert.That(delay.Ticks.Value, Is.EqualTo(1));
            _clock.Advance();
            Assert.That(delay.Ticks.Value, Is.EqualTo(2));
        }

        [Test]
        public void DelayGrowsOneTickPerHold()
        {
            var delay = Start();
            var seen = new List<int>();
            delay.Ticks.Advise(_lifetime.Lifetime, seen.Add);
            _lead.Ticks = RaisingLead;

            _clock.Advance(3 * HoldTicks);

            Assert.That(seen, Is.EqualTo(new[] { 1, 2, 3, 4 }));
        }

        [Test]
        public void DelayStopsAtTheMaximum()
        {
            var delay = Start();
            _lead.Ticks = RaisingLead;

            _clock.Advance(20 * HoldTicks);

            Assert.That(delay.Ticks.Value, Is.EqualTo(4));
        }

        [Test]
        public void ShortRoundTripLowersTheDelayDownToTheMinimum()
        {
            var delay = Start();
            _lead.Ticks = RaisingLead;
            _clock.Advance(2 * HoldTicks);
            var seen = new List<int>();
            delay.Ticks.Change.Advise(_lifetime.Lifetime, seen.Add);
            _lead.Ticks = LoweringLead;

            _clock.Advance(20 * HoldTicks);

            Assert.That(seen, Is.EqualTo(new[] { 2, 1 }));
        }

        [TestCase(SteadyLead)]
        [TestCase(4.25f)]
        [TestCase(3f)]
        [TestCase(2.75f)]
        public void RoundTripInsideTheHysteresisKeepsTheDelay(float lead)
        {
            var delay = Start();
            _lead.Ticks = RaisingLead;
            _clock.Advance(HoldTicks);
            _lead.Ticks = lead;

            _clock.Advance(20 * HoldTicks);

            Assert.That(delay.Ticks.Value, Is.EqualTo(2));
        }

        [Test]
        public void ReportedMarginShortensTheRoundTrip()
        {
            var delay = Start();
            _lead.Ticks = RaisingLead;
            _lead.MarginTicks = RaisingLead - SteadyLead;

            _clock.Advance(20 * HoldTicks);

            Assert.That(delay.Ticks.Value, Is.EqualTo(1));
        }

        [Test]
        public void WishInterruptedBeforeTheHoldEndsStartsOver()
        {
            var delay = Start();
            _lead.Ticks = RaisingLead;
            _clock.Advance(HoldTicks - 1);
            _lead.Ticks = SteadyLead;
            _clock.Advance();
            _lead.Ticks = RaisingLead;

            _clock.Advance(HoldTicks - 1);

            Assert.That(delay.Ticks.Value, Is.EqualTo(1));
            _clock.Advance();
            Assert.That(delay.Ticks.Value, Is.EqualTo(2));
        }

        [Test]
        public void FreeRoundTripIsMeasuredInTicksOfTheClock()
        {
            _clock = new ManualClock(0.125f);
            _worldStep = new WorldStep(_lifetime.Lifetime, _clock);
            var delay = Start();
            _lead.Ticks = RaisingLead;

            _clock.Advance(40);

            Assert.That(delay.Ticks.Value, Is.EqualTo(1));
        }

        [Test]
        public void EndedDelaySettlesNoMore()
        {
            var settling = new LifetimeDefinition();
            var delay = new AdaptiveInputDelay(settling.Lifetime, _worldStep, _clock, _lead, _config);
            settling.Terminate();
            _lead.Ticks = RaisingLead;

            _clock.Advance(20 * HoldTicks);

            Assert.That(delay.Ticks.Value, Is.EqualTo(1));
        }

        private AdaptiveInputDelay Start() =>
            new(_lifetime.Lifetime, _worldStep, _clock, _lead, _config);

        private sealed class InputLead : IInputLead
        {
            public float Ticks { get; set; } = SteadyLead;
            public float MarginTicks { get; set; }
        }
    }
}
