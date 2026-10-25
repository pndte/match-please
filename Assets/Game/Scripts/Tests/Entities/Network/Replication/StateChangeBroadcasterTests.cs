using System.Linq;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Replication;
using Bw.Entities.Network.Rollback;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Replication
{
    public sealed class StateChangeBroadcasterTests
    {
        private LifetimeDefinition _lifetime;
        private ManualClock _clock;
        private WorldStep _worldStep;
        private SimulationValue<int> _simulation;
        private RecordingResults<TickedState<int>> _results;
        private WorldRollbackConfig _config;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _clock = new ManualClock();
            _worldStep = new WorldStep(_lifetime.Lifetime, _clock);
            _simulation = new SimulationValue<int>(10);
            _results = new RecordingResults<TickedState<int>>();
            _config = new WorldRollbackConfig { UnchangedStateTicks = 3 };
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void UnchangedStateWaitsForTheHeartbeat()
        {
            Start();

            _clock.Advance(2);

            Assert.That(Sent(), Is.Empty);
        }

        [Test]
        public void UnchangedStateIsRepeatedEveryHeartbeat()
        {
            Start();

            _clock.Advance(7);

            Assert.That(Sent(), Is.EqualTo(new[] { "3: 10", "6: 10" }));
        }

        [Test]
        public void HeartbeatCountsFromTheTickTheBroadcasterStarted()
        {
            _clock.Tick(20);
            Start();

            _clock.Advance(3);

            Assert.That(Sent(), Is.EqualTo(new[] { "23: 10" }));
        }

        [Test]
        public void ChangeRightAfterASendIsSentAlone()
        {
            Start();
            _simulation.Step(11);

            _clock.Advance();

            Assert.That(Sent(), Is.EqualTo(new[] { "1: 11" }));
        }

        [Test]
        public void ChangeAfterQuietTicksIsPrecededByTheTickBeforeIt()
        {
            Start();
            _clock.Advance(2);
            _simulation.Step(11);

            _clock.Advance();

            Assert.That(Sent(), Is.EqualTo(new[] { "2: 10", "3: 11" }));
        }

        [Test]
        public void TickBeforeAChangeCarriesTheSimulationsPreviousState()
        {
            Start();
            _clock.Advance(2);
            _simulation.Step(11);
            _simulation.Step(12);

            _clock.Advance();

            Assert.That(Sent(), Is.EqualTo(new[] { "2: 11", "3: 12" }));
        }

        [Test]
        public void ChangeEveryTickIsSentEveryTick()
        {
            Start();

            for (var state = 11; state <= 13; state++)
            {
                _simulation.Step(state);
                _clock.Advance();
            }

            Assert.That(Sent(), Is.EqualTo(new[] { "1: 11", "2: 12", "3: 13" }));
        }

        [Test]
        public void HeartbeatCountsFromTheLastChange()
        {
            Start();
            _simulation.Step(11);
            _clock.Advance();

            _clock.Advance(4);

            Assert.That(Sent(), Is.EqualTo(new[] { "1: 11", "4: 11" }));
        }

        [Test]
        public void StateBackToTheSentOneIsNoChange()
        {
            Start();
            _simulation.Step(11);
            _simulation.Step(10);

            _clock.Advance(2);

            Assert.That(Sent(), Is.Empty);
        }

        [Test]
        public void EndedBroadcasterSendsNothing()
        {
            var broadcaster = new LifetimeDefinition();
            new StateChangeBroadcaster<int>(broadcaster.Lifetime, _clock, _worldStep, _simulation, _results, _config);
            broadcaster.Terminate();
            _simulation.Step(11);

            _clock.Advance(5);

            Assert.That(Sent(), Is.Empty);
        }

        [Test]
        public void StatesAreBroadcastToEveryClient()
        {
            Start();
            _simulation.Step(11);

            _clock.Advance();

            Assert.That(_results.Broadcasts, Has.Count.EqualTo(1));
            Assert.That(_results.Sent, Is.Empty);
        }

        private void Start() =>
            new StateChangeBroadcaster<int>(_lifetime.Lifetime, _clock, _worldStep, _simulation, _results, _config);

        private string[] Sent() =>
            _results.Broadcasts.Select(state => $"{state.Tick.ToString()}: {state.State.ToString()}").ToArray();
    }
}
