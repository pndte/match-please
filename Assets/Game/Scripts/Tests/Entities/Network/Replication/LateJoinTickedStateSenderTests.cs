using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Replication;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Replication
{
    public sealed class LateJoinTickedStateSenderTests
    {
        private LifetimeDefinition _lifetime;
        private SpawnedObject _object;
        private ClientsById _clients;
        private ManualClock _clock;
        private SimulationValue<int> _simulation;
        private RecordingResults<TickedState<int>> _results;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _object = new SpawnedObject();
            _clients = new ClientsById(_lifetime.Lifetime);
            _clock = new ManualClock();
            _simulation = new SimulationValue<int>(10);
            _results = new RecordingResults<TickedState<int>>();
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void JoiningClientGetsTheCurrentStateAtTheCurrentTick()
        {
            Start();
            _object.Spawn();
            _clock.Tick(40);
            _simulation.Step(7);

            var client = _clients.Connect(3);

            Assert.That(_results.Sent, Has.Count.EqualTo(1));
            Assert.That(_results.Sent[0].Recipient, Is.SameAs(client));
            Assert.That(_results.Sent[0].Value.Tick, Is.EqualTo(40));
            Assert.That(_results.Sent[0].Value.State, Is.EqualTo(7));
            Assert.That(_results.Broadcasts, Is.Empty);
        }

        [Test]
        public void EveryJoiningClientGetsItsOwnState()
        {
            Start();
            _object.Spawn();

            var first = _clients.Connect(3);
            _simulation.Step(8);
            var second = _clients.Connect(4);

            Assert.That(_results.Sent, Has.Count.EqualTo(2));
            Assert.That(_results.Sent[0].Recipient, Is.SameAs(first));
            Assert.That(_results.Sent[0].Value.State, Is.EqualTo(10));
            Assert.That(_results.Sent[1].Recipient, Is.SameAs(second));
            Assert.That(_results.Sent[1].Value.State, Is.EqualTo(8));
        }

        [Test]
        public void ClientJoiningBeforeTheSpawnIsSentNothing()
        {
            Start();

            _clients.Connect(3);

            Assert.That(_results.Sent, Is.Empty);
        }

        [Test]
        public void ClientJoiningAfterTheDespawnIsSentNothing()
        {
            Start();
            _object.Spawn();
            _object.Despawn();

            _clients.Connect(3);

            Assert.That(_results.Sent, Is.Empty);
        }

        [Test]
        public void ClientsAlreadyThereGetTheStateWhenTheSenderStartsOnASpawnedObject()
        {
            _object.Spawn();
            var client = _clients.Connect(3);

            Start();

            Assert.That(_results.Sent, Has.Count.EqualTo(1));
            Assert.That(_results.Sent[0].Recipient, Is.SameAs(client));
        }

        [Test]
        public void LeavingClientIsSentNothing()
        {
            Start();
            _object.Spawn();
            _clients.Connect(3);

            _clients.ByIds.Remove(3);

            Assert.That(_results.Sent, Has.Count.EqualTo(1));
        }

        [Test]
        public void EndedSenderSendsNothing()
        {
            var sender = new LifetimeDefinition();
            new LateJoinTickedStateSender<int>(sender.Lifetime, _object, _clients, _clock, _simulation, _results);
            _object.Spawn();
            sender.Terminate();

            _clients.Connect(3);

            Assert.That(_results.Sent, Is.Empty);
        }

        private void Start() =>
            new LateJoinTickedStateSender<int>(_lifetime.Lifetime, _object, _clients, _clock, _simulation, _results);
    }
}
