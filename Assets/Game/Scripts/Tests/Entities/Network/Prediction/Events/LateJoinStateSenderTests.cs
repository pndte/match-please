using Bw.Entities.Infrastructure;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Prediction.Events.Requests;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Prediction.Events
{
    public sealed class LateJoinStateSenderTests
    {
        private LifetimeDefinition _lifetime;
        private Clients _clients;
        private SpawnedObject _object;
        private AppliedValue<int> _state;
        private RecordingResults<CausedState<int>> _results;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _clients = new Clients(_lifetime.Lifetime);
            _object = new SpawnedObject();
            _state = new AppliedValue<int>(80);
            _results = new RecordingResults<CausedState<int>>();
        }

        [TearDown]
        public void TearDown() =>
            _lifetime.Terminate();

        [Test]
        public void JoiningClientGetsTheCurrentStateWithoutACause()
        {
            _object.Spawn();
            Send(_lifetime.Lifetime);
            var client = new Client(default);
            _state.Apply(55);

            _clients.ByIds.Add(4UL, client);

            Assert.That(_results.Sent, Has.Count.EqualTo(1));
            Assert.That(_results.Sent[0].Recipient, Is.SameAs(client));
            Assert.That(_results.Sent[0].Result.State, Is.EqualTo(55));
            Assert.That(StateCauseTests.Described(_results.Sent[0].Result.Cause), Is.EqualTo("none"));
            Assert.That(_results.Broadcasts, Is.Empty);
        }

        [Test]
        public void ClientsAlreadyConnectedGetTheStateAtOnce()
        {
            _object.Spawn();
            _clients.ByIds.Add(1UL, new Client(default));
            _clients.ByIds.Add(2UL, new Client(default));

            Send(_lifetime.Lifetime);

            Assert.That(_results.Sent, Has.Count.EqualTo(2));
        }

        [Test]
        public void ClientJoiningAnUnspawnedObjectGetsNothing()
        {
            Send(_lifetime.Lifetime);

            _clients.ByIds.Add(4UL, new Client(default));

            Assert.That(_results.Sent, Is.Empty);
        }

        [Test]
        public void ClientJoiningAfterTheDespawnGetsNothing()
        {
            _object.Spawn();
            Send(_lifetime.Lifetime);
            _object.Despawn();

            _clients.ByIds.Add(4UL, new Client(default));

            Assert.That(_results.Sent, Is.Empty);
        }

        [Test]
        public void LeavingClientGetsNothingMore()
        {
            _object.Spawn();
            Send(_lifetime.Lifetime);
            _clients.ByIds.Add(4UL, new Client(default));

            _clients.ByIds.Remove(4UL);

            Assert.That(_results.Sent, Has.Count.EqualTo(1));
        }

        [Test]
        public void EndedSenderSendsNothing()
        {
            _object.Spawn();
            var ended = new LifetimeDefinition();
            Send(ended.Lifetime);

            ended.Terminate();
            _clients.ByIds.Add(4UL, new Client(default));

            Assert.That(_results.Sent, Is.Empty);
        }

        private void Send(Lifetime lifetime) =>
            new LateJoinStateSender<int>(lifetime, _object, _clients, _state, _results);

        private sealed class Clients : IClientCollection
        {
            public IViewableBiMap<ulong, IClient> ByIds { get; }

            public Clients(Lifetime lifetime)
            {
                ByIds = new ViewableBiMap<ulong, IClient>(lifetime);
            }
        }
    }
}
