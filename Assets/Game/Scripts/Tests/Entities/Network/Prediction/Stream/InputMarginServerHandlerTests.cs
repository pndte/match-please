using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Ticks;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Prediction.Stream
{
    public sealed class InputMarginServerHandlerTests
    {
        private const int Interval = 15;

        private LifetimeDefinition _lifetime;
        private ManualClock _clock;
        private ScriptedWorldStep _worldStep;
        private ManualResults<TickedInput<int>> _requests;
        private RecordingResults<int> _margins;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _clock = new ManualClock();
            _worldStep = new ScriptedWorldStep();
            _requests = new ManualResults<TickedInput<int>>();
            _margins = new RecordingResults<int>();
            new InputMarginServerHandler<int>(
                _lifetime.Lifetime,
                _clock,
                _worldStep,
                _requests,
                _margins,
                new NetworkTicksConfig { MarginReportIntervalTicks = Interval });
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void WorstMarginOfTheWindowIsBroadcast()
        {
            _clock.Tick(10);
            Hear(14);
            Hear(12);
            Hear(13);

            _worldStep.Fire(TickPhase.Publish, Interval);

            Assert.That(_margins.Broadcasts, Is.EqualTo(new[] { 1 }));
            Assert.That(_margins.Sent, Is.Empty);
        }

        [TestCase(10, 11, 0)]
        [TestCase(10, 10, -1)]
        [TestCase(10, 7, -4)]
        [TestCase(10, 30, 19)]
        public void MarginIsHowManyTicksAheadOfTheNextTickTheInputCame(int current, int tick, int margin)
        {
            _clock.Tick(current);
            Hear(tick);

            _worldStep.Fire(TickPhase.Publish, Interval);

            Assert.That(_margins.Broadcasts, Is.EqualTo(new[] { margin }));
        }

        [Test]
        public void MarginIsReportedOnlyOnIntervalTicks()
        {
            Hear(3);

            _worldStep.Fire(TickPhase.Publish, Interval - 1);
            _worldStep.Fire(TickPhase.Publish, Interval + 1);

            Assert.That(_margins.Broadcasts, Is.Empty);
        }

        [Test]
        public void WindowWithoutInputsReportsNothing()
        {
            _worldStep.Fire(TickPhase.Publish, Interval);

            Assert.That(_margins.Broadcasts, Is.Empty);
        }

        [Test]
        public void EveryWindowStartsAfresh()
        {
            _clock.Tick(10);
            Hear(12);
            _worldStep.Fire(TickPhase.Publish, Interval);

            Hear(16);
            _worldStep.Fire(TickPhase.Publish, 2 * Interval);
            _worldStep.Fire(TickPhase.Publish, 3 * Interval);

            Assert.That(_margins.Broadcasts, Is.EqualTo(new[] { 1, 5 }));
        }

        [TestCase(TickPhase.Prepare)]
        [TestCase(TickPhase.BeforePhysics)]
        [TestCase(TickPhase.Default)]
        [TestCase(TickPhase.Late)]
        public void OnlyPublishReports(TickPhase phase)
        {
            Hear(3);

            _worldStep.Fire(phase, Interval);

            Assert.That(_margins.Broadcasts, Is.Empty);
        }

        private void Hear(int tick) =>
            _requests.Deliver(new TickedInput<int>(tick, 0, 0));
    }
}
