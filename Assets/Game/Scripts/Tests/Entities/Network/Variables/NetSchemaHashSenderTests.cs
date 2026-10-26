using System.Linq;
using Bw.Entities.Network.Variables;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Doubles;
using Bw.UseCases;
using JetBrains.Lifetimes;
using NUnit.Framework;
using Unity.Netcode;

namespace Bw.Tests.Entities.Network.Variables
{
    public sealed class NetSchemaHashSenderTests
    {
        private const ulong ObjectId = 42;

        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private RecordingSenders _senders;
        private SpawnedObject _spawned;
        private NetEntriesSchema _schema;
        private NetEntries _entries;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _senders = new RecordingSenders();
            _spawned = new SpawnedObject(ObjectId);
            var schema = new NetEntriesSchemaBuilder();
            schema.DeclareResult<int>(NetworkDelivery.Reliable);
            _schema = schema.Build();
            _entries = new NetEntries(_schema, new NetSendGuardFactoryClient(new Ownership(), _objects.Create("Net object").AddComponent<NetworkObject>()));
            new NetReplicationClient(_lifetime.Lifetime, _entries, _spawned, _senders);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void NothingIsSentBeforeTheObjectSpawns()
        {
            new NetSchemaHashSender(_lifetime.Lifetime, _schema, _spawned, _entries);

            Assert.That(_senders.All, Is.Empty);
        }

        [Test]
        public void SpawnSendsTheHashReliablyAsEntryZero()
        {
            new NetSchemaHashSender(_lifetime.Lifetime, _schema, _spawned, _entries);

            _spawned.Spawn();

            var sent = _senders.All.Single();
            Assert.That(sent.Route, Is.EqualTo(RecordingSenders.Route.ToServer));
            Assert.That(sent.Header.NetworkObjectId, Is.EqualTo(ObjectId));
            Assert.That(sent.Header.VarId, Is.EqualTo(0));
            Assert.That(sent.Delivery, Is.EqualTo(NetworkDelivery.Reliable));
            Assert.That(sent.Payload, Is.EqualTo(_schema.Hash));
        }

        [Test]
        public void EverySpawnSendsTheHashAgain()
        {
            new NetSchemaHashSender(_lifetime.Lifetime, _schema, _spawned, _entries);

            _spawned.Spawn();
            _spawned.Despawn();
            _spawned.Spawn();

            Assert.That(_senders.All, Has.Count.EqualTo(2));
        }

        [Test]
        public void EndedSenderSendsNothing()
        {
            var sender = new LifetimeDefinition();
            new NetSchemaHashSender(sender.Lifetime, _schema, _spawned, _entries);
            sender.Terminate();

            _spawned.Spawn();

            Assert.That(_senders.All, Is.Empty);
        }
    }
}
