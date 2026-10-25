using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Ticks;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Prediction.Stream
{
    public sealed class PredictionStateBroadcasterTests
    {
        private LifetimeDefinition _lifetime;
        private ScriptedWorldStep _worldStep;
        private SteppedSimulation<int> _simulation;
        private RecordingResults<TickedState<int>> _results;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _worldStep = new ScriptedWorldStep();
            _simulation = new SteppedSimulation<int>(7);
            _results = new RecordingResults<TickedState<int>>();
            new PredictionStateBroadcaster<int>(_lifetime.Lifetime, _worldStep, _simulation, _results);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void FirstPublishSendsTheStartingState()
        {
            _worldStep.Fire(TickPhase.Publish, 10);

            Assert.That(_results.Broadcasts, Is.EqualTo(new[] { new TickedState<int>(10, 7) }));
        }

        [Test]
        public void UnchangedSimulationSendsNothing()
        {
            _worldStep.Fire(TickPhase.Publish, 10);

            _worldStep.Fire(TickPhase.Publish, 11);

            Assert.That(_results.Broadcasts, Has.Count.EqualTo(1));
        }

        [Test]
        public void SteppedStateIsSentUnderItsTick()
        {
            _worldStep.Fire(TickPhase.Publish, 10);
            _simulation.Step(8);

            _worldStep.Fire(TickPhase.Publish, 11);

            Assert.That(_results.Broadcasts[^1], Is.EqualTo(new TickedState<int>(11, 8)));
        }

        [Test]
        public void StepThatChangesNothingIsStillSent()
        {
            _worldStep.Fire(TickPhase.Publish, 10);
            _simulation.Step(7);

            _worldStep.Fire(TickPhase.Publish, 11);

            Assert.That(_results.Broadcasts[^1], Is.EqualTo(new TickedState<int>(11, 7)));
        }

        [Test]
        public void AppliedStateIsSent()
        {
            _worldStep.Fire(TickPhase.Publish, 10);
            _simulation.Apply(20);

            _worldStep.Fire(TickPhase.Publish, 11);

            Assert.That(_results.Broadcasts[^1], Is.EqualTo(new TickedState<int>(11, 20)));
        }

        [Test]
        public void TickSendsOnceWithItsFinalState()
        {
            _worldStep.Fire(TickPhase.Publish, 10);
            _simulation.Step(8);
            _simulation.Step(9);

            _worldStep.Fire(TickPhase.Publish, 11);

            Assert.That(_results.Broadcasts, Has.Count.EqualTo(2));
            Assert.That(_results.Broadcasts[^1], Is.EqualTo(new TickedState<int>(11, 9)));
        }

        [TestCase(TickPhase.Prepare)]
        [TestCase(TickPhase.Early)]
        [TestCase(TickPhase.Default)]
        [TestCase(TickPhase.Late)]
        public void OnlyPublishSends(TickPhase phase)
        {
            _simulation.Step(8);

            _worldStep.Fire(phase, 10);

            Assert.That(_results.Broadcasts, Is.Empty);
        }

        [Test]
        public void StateIsNeverSentToOneClient()
        {
            _simulation.Step(8);

            _worldStep.Fire(TickPhase.Publish, 10);

            Assert.That(_results.Sent, Is.Empty);
        }
    }
}
