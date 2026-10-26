using System;
using System.Collections.Generic;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Tests.Doubles;
using Bw.UseCases;
using JetBrains.Lifetimes;
using NUnit.Framework;
using Unity.Netcode;

namespace Bw.Tests.Entities.Network.Variables
{
    public sealed class NetEntriesClientTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private NetworkObject _networkObject;
        private Ownership _ownership;
        private NetEntriesSchema _schema;
        private NetRequestDeclaration<int> _request;
        private NetResultDeclaration<int> _result;
        private NetPropertyDeclaration<int> _serverProperty;
        private NetPropertyDeclaration<int> _everyoneProperty;
        private NetPropertyDeclaration<int> _otherEveryoneProperty;
        private NetEntries _entries;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _networkObject = _objects.Create("Net object").AddComponent<NetworkObject>();
            _ownership = new Ownership();
            var schema = new NetEntriesSchemaBuilder();
            _request = schema.DeclareRequest<int>(NetworkDelivery.Reliable);
            _result = schema.DeclareResult<int>(NetworkDelivery.Reliable);
            _serverProperty = schema.DeclareProperty(3, NetworkDelivery.Reliable, NetworkPermissions.Server);
            _everyoneProperty = schema.DeclareProperty(0, NetworkDelivery.Reliable, NetworkPermissions.Everyone);
            _otherEveryoneProperty = schema.DeclareProperty(0, NetworkDelivery.Reliable, NetworkPermissions.Everyone);
            _schema = schema.Build();
            _entries = EntriesOf(_schema);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void SchemaHashIsEntryZeroAndTheDeclarationsFollowInOrder()
        {
            Assert.That(_entries.At(0).Declaration, Is.SameAs(_schema.HashDeclaration));
            Assert.That(_entries.At(1).Declaration, Is.SameAs(_request));
            Assert.That(_entries.At(2).Declaration, Is.SameAs(_result));
            Assert.That(_entries.At(5).Declaration, Is.SameAs(_otherEveryoneProperty));
        }

        [Test]
        public void UnknownEntryNumberThrows()
        {
            Assert.Throws<InvalidOperationException>(() => _entries.At(6));
        }

        [Test]
        public void RequestIsSentWhileTheObjectIsMine()
        {
            _ownership.SetMine(_lifetime.Lifetime);
            var sent = new List<int>();
            _entries.RequestSender(_request).Sent.Advise(_lifetime.Lifetime, sent.Add);

            _entries.RequestSender(_request).Send(5);

            Assert.That(sent, Is.EqualTo(new[] { 5 }));
        }

        [Test]
        public void RequestOfAnObjectNotMineThrows()
        {
            var sent = new List<int>();
            _entries.RequestSender(_request).Sent.Advise(_lifetime.Lifetime, sent.Add);

            Assert.Throws<InvalidOperationException>(() => _entries.RequestSender(_request).Send(5));
            Assert.That(sent, Is.Empty);
        }

        [Test]
        public void RequestThrowsOnceTheObjectIsNoLongerMine()
        {
            var mine = new LifetimeDefinition();
            _ownership.SetMine(mine.Lifetime);
            _entries.RequestSender(_request).Send(1);

            mine.Terminate();

            Assert.Throws<InvalidOperationException>(() => _entries.RequestSender(_request).Send(2));
        }

        [Test]
        public void ResultIsNeverBroadcastByAClient()
        {
            _ownership.SetMine(_lifetime.Lifetime);

            Assert.Throws<InvalidOperationException>(() => _entries.ResultSender(_result).Broadcast(1));
        }

        [Test]
        public void ResultIsNeverSentToAClientByAClient()
        {
            _ownership.SetMine(_lifetime.Lifetime);
            var recipient = new Client(new ConnectionRequest("Сева", 4, System.Guid.NewGuid()));

            Assert.Throws<InvalidOperationException>(() => _entries.ResultSender(_result).SendTo(recipient, 1));
        }

        [Test]
        public void PropertyStartsAtItsDeclaredValue()
        {
            Assert.That(_entries.Property(_serverProperty).Value, Is.EqualTo(3));
        }

        [Test]
        public void ServerPropertyIsNeverWrittenByAClient()
        {
            _ownership.SetMine(_lifetime.Lifetime);
            var property = _entries.Property(_serverProperty);

            Assert.Throws<InvalidOperationException>(() => property.Value = 4);
            Assert.That(property.Value, Is.EqualTo(3));
        }

        [TestCase(NetworkPermissions.Owner)]
        [TestCase(NetworkPermissions.Client)]
        public void OwnedPropertyIsNotWrittenWhileNotMine(NetworkPermissions permissions)
        {
            var property = PropertyWith(permissions);

            Assert.Throws<InvalidOperationException>(() => property.Value = 4);
            Assert.That(property.Value, Is.EqualTo(0));
        }

        [TestCase(NetworkPermissions.Owner)]
        [TestCase(NetworkPermissions.Client)]
        public void OwnedPropertyIsWrittenWhileMine(NetworkPermissions permissions)
        {
            var property = PropertyWith(permissions);
            _ownership.SetMine(_lifetime.Lifetime);
            var seen = new List<int>();
            property.Change.Advise(_lifetime.Lifetime, seen.Add);

            property.Value = 4;

            Assert.That(property.Value, Is.EqualTo(4));
            Assert.That(seen, Is.EqualTo(new[] { 4 }));
        }

        [Test]
        public void PropertyIsObservedFromItsCurrentValue()
        {
            var property = _entries.Property(_everyoneProperty);
            var seen = new List<int>();
            property.Advise(_lifetime.Lifetime, seen.Add);

            property.Value = 9;

            Assert.That(seen, Is.EqualTo(new[] { 0, 9 }));
        }

        [Test]
        public void EveryonePropertyIsWrittenWithoutOwnership()
        {
            var property = _entries.Property(_everyoneProperty);

            property.Value = 9;

            Assert.That(property.Value, Is.EqualTo(9));
        }

        [Test]
        public void DeclarationsOfTheSameTypeAreSeparateEntries()
        {
            _entries.Property(_everyoneProperty).Value = 9;

            Assert.That(_entries.Property(_otherEveryoneProperty).Value, Is.EqualTo(0));
        }

        [Test]
        public void DeclarationOfAnotherSchemaIsRefused()
        {
            var other = new NetEntriesSchemaBuilder();
            var request = other.DeclareRequest<int>(NetworkDelivery.Reliable);
            var result = other.DeclareResult<int>(NetworkDelivery.Reliable);
            var property = other.DeclareProperty(0, NetworkDelivery.Reliable, NetworkPermissions.Everyone);
            other.Build();

            Assert.Throws<InvalidOperationException>(() => _entries.RequestSender(request));
            Assert.Throws<InvalidOperationException>(() => _entries.RequestReceiver(request));
            Assert.Throws<InvalidOperationException>(() => _entries.ResultSender(result));
            Assert.Throws<InvalidOperationException>(() => _entries.ResultReceiver(result));
            Assert.Throws<InvalidOperationException>(() => _entries.Property(property));
        }

        [Test]
        public void PropertyWithUnknownPermissionsIsRefused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PropertyWith((NetworkPermissions)0));
        }

        private INetProperty<int> PropertyWith(NetworkPermissions permissions)
        {
            var schema = new NetEntriesSchemaBuilder();
            var declaration = schema.DeclareProperty(0, NetworkDelivery.Reliable, permissions);
            return EntriesOf(schema.Build()).Property(declaration);
        }

        private NetEntries EntriesOf(NetEntriesSchema schema) =>
            new(schema, new NetSendGuardFactoryClient(_ownership, _networkObject));
    }
}
