using System.Linq;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Network.Variables;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Doubles;
using Bw.Tests.Entities.Network.Prediction.Doubles;
using Bw.UseCases;
using JetBrains.Lifetimes;
using NUnit.Framework;
using Unity.Netcode;
using SpawnedObject = Bw.Tests.Entities.Network.Doubles.SpawnedObject;

namespace Bw.Tests.Entities.Network.Variables
{
    public sealed class NetReplicationServerTests
    {
        private const ulong ObjectId = 42;

        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private ClientsById _clients;
        private RecordingSenders _senders;
        private SpawnedObject _spawned;
        private ScriptedWorldStep _worldStep;
        private NetResultDeclaration<int> _result;
        private NetPropertyDeclaration<int> _serverProperty;
        private NetEntries _entries;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _clients = new ClientsById(_lifetime.Lifetime);
            _senders = new RecordingSenders();
            _spawned = new SpawnedObject(ObjectId);
            _worldStep = new ScriptedWorldStep();
            var schema = new NetEntriesSchemaBuilder();
            _result = schema.DeclareResult<int>(NetworkDelivery.Unreliable);
            _serverProperty = schema.DeclareProperty(3, NetworkDelivery.ReliableSequenced, NetworkPermissions.Server);
            _entries = new NetEntries(schema.Build(), new NetSendGuardFactoryServer(new Ownership(), _objects.Create("Net object").AddComponent<NetworkObject>()));
            new NetReplicationServer(_lifetime.Lifetime, _entries, _spawned, _worldStep, _senders, _clients);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void UnspawnedObjectSendsNothing()
        {
            _clients.Connect(7);

            _entries.ResultSender(_result).Broadcast(1);
            _entries.Property(_serverProperty).Value = 4;
            _worldStep.Fire(TickPhase.Publish, 10);

            Assert.That(_senders.All, Is.Empty);
        }

        [Test]
        public void SpawnedObjectWaitsForThePublishPhaseItsClientsLearnOfItBy()
        {
            _clients.Connect(7);
            _spawned.Spawn();

            _entries.Property(_serverProperty).Value = 4;

            Assert.That(_senders.All, Is.Empty);
        }

        [Test]
        public void FirstPublishSendsTheStateToEveryConnectedClient()
        {
            var first = _clients.Connect(7);
            var second = _clients.Connect(8);
            _entries.Property(_serverProperty).Value = 4;
            _spawned.Spawn();

            _worldStep.Fire(TickPhase.Publish, 10);

            var states = _senders.All.Where(sent => sent.Route == RecordingSenders.Route.ToClient).ToList();
            Assert.That(states.Select(sent => sent.Recipient), Is.EquivalentTo(new[] { first, second }));
            Assert.That(states.Select(sent => sent.Payload), Is.All.EqualTo(4));
            Assert.That(states.Select(sent => sent.Delivery), Is.All.EqualTo(NetworkDelivery.Reliable));
            Assert.That(states.Select(sent => sent.Header.VarId), Is.All.EqualTo(2));
        }

        [Test]
        public void ResultStagedBeforeThePublishIsSentAtIt()
        {
            _spawned.Spawn();
            _entries.ResultSender(_result).Broadcast(1);

            _worldStep.Fire(TickPhase.Publish, 10);

            var broadcast = _senders.All.Single(sent => sent.Route == RecordingSenders.Route.Broadcast);
            Assert.That(broadcast.Header.NetworkObjectId, Is.EqualTo(ObjectId));
            Assert.That(broadcast.Header.VarId, Is.EqualTo(1));
            Assert.That(broadcast.Payload, Is.EqualTo(1));
        }

        [Test]
        public void ClientJoiningLaterGetsTheState()
        {
            _spawned.Spawn();
            _worldStep.Fire(TickPhase.Publish, 10);

            var late = _clients.Connect(9);

            var state = _senders.All.Single(sent => sent.Route == RecordingSenders.Route.ToClient);
            Assert.That(state.Recipient, Is.SameAs(late));
            Assert.That(state.Payload, Is.EqualTo(3));
        }

        [Test]
        public void LaterPublishesSendTheStateNoMore()
        {
            _clients.Connect(7);
            _spawned.Spawn();
            _worldStep.Fire(TickPhase.Publish, 10);
            var announced = _senders.All.Count;

            _worldStep.Fire(TickPhase.Publish, 11);

            Assert.That(_senders.All, Has.Count.EqualTo(announced));
        }

        [Test]
        public void ChangeAfterTheAnnouncementIsBroadcastAtOnce()
        {
            _spawned.Spawn();
            _worldStep.Fire(TickPhase.Publish, 10);

            _entries.Property(_serverProperty).Value = 5;

            var broadcast = _senders.All.Single(sent => sent.Route == RecordingSenders.Route.Broadcast);
            Assert.That(broadcast.Payload, Is.EqualTo(5));
            Assert.That(broadcast.Delivery, Is.EqualTo(NetworkDelivery.ReliableSequenced));
        }

        [Test]
        public void ResultForOneClientGoesOnlyToIt()
        {
            var recipient = _clients.Connect(7);
            _spawned.Spawn();
            _worldStep.Fire(TickPhase.Publish, 10);
            var announced = _senders.All.Count;

            _entries.ResultSender(_result).SendTo(recipient, 2);

            var sent = _senders.All.Skip(announced).Single();
            Assert.That(sent.Route, Is.EqualTo(RecordingSenders.Route.ToClient));
            Assert.That(sent.Recipient, Is.SameAs(recipient));
            Assert.That(sent.Payload, Is.EqualTo(2));
        }

        [Test]
        public void ResultForAClientThatLeftBeforeThePublishIsDropped()
        {
            var recipient = _clients.Connect(7);
            _spawned.Spawn();
            _entries.ResultSender(_result).SendTo(recipient, 2);
            _clients.Disconnect(7);

            _worldStep.Fire(TickPhase.Publish, 10);

            Assert.That(_senders.All.Where(sent => sent.Recipient == recipient), Is.Empty, "a client that left has no id to send to");
        }

        [Test]
        public void ResultsOfAnEndedSpawnAreNeverSent()
        {
            _spawned.Spawn();
            _entries.ResultSender(_result).Broadcast(1);
            _spawned.Despawn();
            _entries.ResultSender(_result).Broadcast(2);
            _spawned.Spawn();

            _worldStep.Fire(TickPhase.Publish, 10);

            Assert.That(_senders.All.Where(sent => sent.Route == RecordingSenders.Route.Broadcast), Is.Empty);
        }

        [Test]
        public void RespawnAnnouncesAgainAtTheNextPublish()
        {
            var client = _clients.Connect(7);
            _spawned.Spawn();
            _worldStep.Fire(TickPhase.Publish, 10);
            _spawned.Despawn();
            _spawned.Spawn();
            var before = _senders.All.Count;

            _worldStep.Fire(TickPhase.Publish, 11);

            var state = _senders.All.Skip(before).Single();
            Assert.That(state.Recipient, Is.SameAs(client));
        }
    }
}
