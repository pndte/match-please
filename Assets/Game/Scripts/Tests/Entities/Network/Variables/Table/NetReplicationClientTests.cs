using Bw.Entities.Network.Variables;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Network.Doubles;
using Bw.UseCases;
using JetBrains.Lifetimes;
using NUnit.Framework;
using Unity.Netcode;

namespace Bw.Tests.Entities.Network.Variables
{
    public sealed class NetReplicationClientTests
    {
        private const ulong ObjectId = 42;

        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private Ownership _ownership;
        private RecordingSenders _senders;
        private SpawnedObject _spawned;
        private NetRequestDeclaration<int> _request;
        private NetPropertyDeclaration<int> _everyoneProperty;
        private NetEntries _entries;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _ownership = new Ownership();
            _senders = new RecordingSenders();
            _spawned = new SpawnedObject(ObjectId);
            var schema = new NetEntriesSchemaBuilder();
            _request = schema.DeclareRequest<int>(NetworkDelivery.Unreliable);
            _everyoneProperty = schema.DeclareProperty(0, NetworkDelivery.ReliableSequenced, NetworkPermissions.Everyone);
            _entries = new NetEntries(schema.Build(), new NetSendGuardFactoryClient(_ownership, _objects.Create("Net object").AddComponent<NetworkObject>()));
            new NetReplicationClient(_lifetime.Lifetime, _entries, _spawned, _senders);
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
            _ownership.SetMine(_lifetime.Lifetime);

            _entries.RequestSender(_request).Send(5);
            _entries.Property(_everyoneProperty).Value = 9;

            Assert.That(_senders.All, Is.Empty);
        }

        [Test]
        public void RequestGoesToTheServerUnderItsNumberAndDelivery()
        {
            _spawned.Spawn();
            _ownership.SetMine(_lifetime.Lifetime);

            _entries.RequestSender(_request).Send(5);

            Assert.That(_senders.All, Has.Count.EqualTo(1));
            Assert.That(_senders.All[0].Route, Is.EqualTo(RecordingSenders.Route.ToServer));
            Assert.That(_senders.All[0].Header.NetworkObjectId, Is.EqualTo(ObjectId));
            Assert.That(_senders.All[0].Header.VarId, Is.EqualTo(1));
            Assert.That(_senders.All[0].Payload, Is.EqualTo(5));
            Assert.That(_senders.All[0].Delivery, Is.EqualTo(NetworkDelivery.Unreliable));
        }

        [Test]
        public void PropertyWrittenBeforeTheSpawnIsNotSentLater()
        {
            _entries.Property(_everyoneProperty).Value = 9;

            _spawned.Spawn();

            Assert.That(_senders.All, Is.Empty);
        }

        [Test]
        public void PropertyWrittenWhileSpawnedGoesToTheServer()
        {
            _spawned.Spawn();

            _entries.Property(_everyoneProperty).Value = 9;

            Assert.That(_senders.All, Has.Count.EqualTo(1));
            Assert.That(_senders.All[0].Header.VarId, Is.EqualTo(2));
            Assert.That(_senders.All[0].Payload, Is.EqualTo(9));
            Assert.That(_senders.All[0].Delivery, Is.EqualTo(NetworkDelivery.ReliableSequenced));
        }

        [Test]
        public void DespawnedObjectSendsNothing()
        {
            _spawned.Spawn();
            _spawned.Despawn();

            _entries.Property(_everyoneProperty).Value = 9;

            Assert.That(_senders.All, Is.Empty);
        }
    }
}
