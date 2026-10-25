using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Replication;
using Bw.Entities.Network.Rollback;
using Bw.Tests.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Replication
{
    public sealed class ReplicatedStatesTests
    {
        private LifetimeDefinition _lifetime;
        private ManualResults<TickedState<int>> _received;
        private WorldRollbackConfig _config;
        private ReplicatedStates<int> _states;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _received = new ManualResults<TickedState<int>>();
            _config = new WorldRollbackConfig { MaxReplayTicks = 4 };
            _states = new ReplicatedStates<int>(_lifetime.Lifetime, _received, _config);
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void HistoryHoldsTheRollbackWindow()
        {
            Assert.That(_states.Capacity, Is.EqualTo(_config.HistoryTicks));
        }

        [Test]
        public void ReceivedStateIsKeptAtItsTick()
        {
            _received.Deliver(new TickedState<int>(10, 100));

            Assert.That(_states.TryGetAt(10, out var state), Is.True);
            Assert.That(state, Is.EqualTo(100));
            Assert.That(_states.NewestTick, Is.EqualTo(10));
        }

        [Test]
        public void NewerStateMovesTheNewestTick()
        {
            _received.Deliver(new TickedState<int>(10, 100));

            _received.Deliver(new TickedState<int>(12, 120));

            Assert.That(_states.NewestTick, Is.EqualTo(12));
            Assert.That(_states.TryGetAt(10, out _), Is.True);
        }

        [Test]
        public void LateStateInsideTheWindowIsKept()
        {
            _received.Deliver(new TickedState<int>(10, 100));

            _received.Deliver(new TickedState<int>(6, 60));

            Assert.That(_states.TryGetAt(6, out var state), Is.True);
            Assert.That(state, Is.EqualTo(60));
            Assert.That(_states.NewestTick, Is.EqualTo(10));
        }

        [Test]
        public void StateOlderThanTheWindowIsDroppedQuietly()
        {
            _received.Deliver(new TickedState<int>(10, 100));

            _received.Deliver(new TickedState<int>(5, 50));
            _received.Deliver(new TickedState<int>(-100, 1));

            Assert.That(_states.TryGetAt(5, out _), Is.False);
            Assert.That(_states.TryGetAt(10, out var state), Is.True);
            Assert.That(state, Is.EqualTo(100));
        }

        [Test]
        public void EndedHistoryKeepsNothingNew()
        {
            var lifetime = new LifetimeDefinition();
            var states = new ReplicatedStates<int>(lifetime.Lifetime, _received, _config);
            lifetime.Terminate();

            _received.Deliver(new TickedState<int>(10, 100));

            Assert.That(states.TryGetAt(10, out _), Is.False);
        }
    }
}
