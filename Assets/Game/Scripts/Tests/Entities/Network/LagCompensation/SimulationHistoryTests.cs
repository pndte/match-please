using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Network.Ticks;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.LagCompensation
{
    public sealed class SimulationHistoryTests
    {
        private LifetimeDefinition _lifetime;
        private SteppedSimulation<int> _simulation;
        private ScriptedWorldStep _worldStep;
        private SimulationHistory<int> _history;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _simulation = new SteppedSimulation<int>(5);
            _worldStep = new ScriptedWorldStep();
            _history = new SimulationHistory<int>(_lifetime.Lifetime, _simulation, new ManualClock(1f / 30f), _worldStep, new LagCompensationConfig { MaxRewindSeconds = 0.4f });
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void HistoryHoldsTheRewindDepthAndTwoTicksMore()
        {
            Assert.That(_history.Capacity, Is.EqualTo(14));
        }

        [Test]
        public void PublishRecordsTheFinalStateOfItsTick()
        {
            _simulation.Step(6);
            _worldStep.Fire(TickPhase.Publish, 10);
            _simulation.Step(7);
            _worldStep.Fire(TickPhase.Publish, 11);

            Assert.That(_history.TryGetAt(10, out var first), Is.True);
            Assert.That(_history.TryGetAt(11, out var second), Is.True);
            Assert.That((first, second), Is.EqualTo((6, 7)));
            Assert.That(_history.NewestTick, Is.EqualTo(11));
        }

        [Test]
        public void UnchangedStateIsRecordedEveryTick()
        {
            _worldStep.Fire(TickPhase.Publish, 10);
            _worldStep.Fire(TickPhase.Publish, 11);

            Assert.That(_history.TryGetAt(11, out var state), Is.True);
            Assert.That(state, Is.EqualTo(5));
        }

        [TestCase(TickPhase.Prepare)]
        [TestCase(TickPhase.BeforePhysics)]
        [TestCase(TickPhase.Default)]
        [TestCase(TickPhase.Late)]
        public void OnlyPublishRecords(TickPhase phase)
        {
            _worldStep.Fire(phase, 10);

            Assert.That(_history.TryGetAt(10, out _), Is.False);
        }

        [Test]
        public void EndedHistoryStopsRecording()
        {
            _lifetime.Terminate();

            _worldStep.Fire(TickPhase.Publish, 10);

            Assert.That(_history.TryGetAt(10, out _), Is.False);
        }
    }
}
