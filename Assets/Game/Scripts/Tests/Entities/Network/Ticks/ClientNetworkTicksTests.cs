using Bw.Entities.Network;
using Bw.Entities.Network.Ticks;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Ticks
{
    public sealed class ClientNetworkTicksTests
    {
        private LifetimeDefinition _lifetime;
        private NetworkTicksConfig _config;
        private ClientNetworkTicks _ticks;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _config = new NetworkTicksConfig { InitialLeadTicks = 2.5f };
            _ticks = new ClientNetworkTicks(_lifetime.Lifetime, new NetworkHolder(), _config);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void LeadStartsAtTheInitialGuess()
        {
            Assert.That(((IInputLead)_ticks).Ticks, Is.EqualTo(2.5f));
        }

        [Test]
        public void ClockStaysAtTickZeroUntilTheNetworkIsKnown()
        {
            var ticked = 0;
            _ticks.Ticked.Advise(_lifetime.Lifetime, _ => ticked++);

            Assert.That(_ticks.Current, Is.EqualTo(0));
            Assert.That(ticked, Is.EqualTo(0));
        }

        [Test]
        public void ReportedMarginIsKept()
        {
            _ticks.Report(3);

            Assert.That(((IInputLead)_ticks).MarginTicks, Is.EqualTo(3f));
        }

        [Test]
        public void ReportsOfOneFrameKeepTheWorstMargin()
        {
            _ticks.Report(3);
            _ticks.Report(5);
            _ticks.Report(-2);
            _ticks.Report(1);

            Assert.That(((IInputLead)_ticks).MarginTicks, Is.EqualTo(-2f));
        }

        [Test]
        public void ReportsAloneDoNotMoveTheLead()
        {
            _ticks.Report(-10);

            Assert.That(((IInputLead)_ticks).Ticks, Is.EqualTo(2.5f));
        }
    }
}
